using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngineInternal;

public enum BiomeSoundType
{
    Nothing,
    Grass,
    Swamp,
    Desert,
    Ocean,
    Forest,
}
public class BiomeSoundPlayer
{
    private AudioSource audioSource;
    private float transitionDuration;
    private AudioClip currentClip;
    public float Volume;

    public BiomeSoundPlayer(AudioSource source, float transitionDuration)
    {
        this.audioSource = source;
        this.audioSource.loop = true;
        this.transitionDuration = transitionDuration;
    }

    public void PlayClip(AudioClip clip)
    {
        if (currentClip == clip) return;

        audioSource.DOKill();
        transition = true;
        if (audioSource.isPlaying)
        {
            audioSource.DOFade(0.0f, transitionDuration).SetUpdate(true).OnComplete(() =>
            {
                audioSource.clip = clip;
                audioSource.volume = 0.0f;
                audioSource.Play();
                audioSource.time = Random.Range(0, audioSource.clip.length);
                audioSource.DOFade(Volume, transitionDuration).SetUpdate(true).OnComplete(() =>
                {
                    transition = false;
                });
            });
        }
        else
        {
            audioSource.clip = clip;
            audioSource.volume = 0.0f;
            audioSource.Play();
            audioSource.time = Random.Range(0, audioSource.clip.length);
            audioSource.DOFade(Volume, transitionDuration).SetUpdate(true)
                .OnComplete(() =>
                {
                    transition = false;
                });
        }
        currentClip = clip;
    }
    private bool transition;

    public void SetVolume(float volume)
    {
        if (transition)
        {
            return;
        }
        this.Volume = volume;
        audioSource.volume = volume;
    }
}

public class SoundEffectController : MonoBehaviour
{
    private BiomeSoundPlayer soundPlayer;

    public BiomeSoundType CurrentBiomeSoundType;

    public float TransitionDuration = 0.5f;
    public void OnEnable()
    {
        SeasonController.OnSeasonChanged += OnSeasonChanged;

    }
    public void OnDisable()
    {

        SeasonController.OnSeasonChanged -= OnSeasonChanged;
    }

    private void Start()
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        soundPlayer = new BiomeSoundPlayer(audioSource, TransitionDuration);
        CurrentBiomeSoundType = BiomeSoundType.Nothing;
        SetSoundEffectByBiome().Forget();
    }

    private void OnSeasonChanged(Season season)
    {
        CurrentBiomeSoundType = BiomeSoundType.Nothing;
    }

    void Update()
    {

        if (CurrentBiomeSoundType != BiomeSoundType.Nothing)
        {
            soundPlayer.SetVolume(Mathf.Clamp01(1f / GameController.Camera.orthographicSize));
        }
    }

    public async UniTaskVoid SetSoundEffectByBiome()
    {
        while (!destroyCancellationToken.IsCancellationRequested)
        {
            while (!Application.isFocused)
            {
                await UniTask.WaitForSeconds(1, ignoreTimeScale: true);
            }
            var waitTime = 0.15f;
            await UniTask.WaitForSeconds(waitTime, true);
            var cell = TileMapController.Instance.GetCellScreen(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            var neighbours = TileMapController.Instance.GetNeighbours(cell);

            var center = Biome.GetBiome((Tile)TileMapController.Instance.Foreground.GetTile(cell));
            var biomeCounts = new Dictionary<BiomeSoundType, int>
        {
            { BiomeSoundType.Grass, 0 },
            { BiomeSoundType.Swamp, 0 },
            { BiomeSoundType.Desert, 0 },
            { BiomeSoundType.Ocean, 0 },
            { BiomeSoundType.Forest, 0 },
        };

            UpdateBiomeCount(center, biomeCounts, 2);
            foreach (var n in neighbours)
            {
                var neighbourBiome = Biome.GetBiome((Tile)TileMapController.Instance.Foreground.GetTile(n));
                UpdateBiomeCount(neighbourBiome, biomeCounts, 1);
            }

            var mostPrevalentBiome = GetMostPrevalentBiome(biomeCounts);

            if (mostPrevalentBiome != CurrentBiomeSoundType)
            {
                switch (mostPrevalentBiome)
                {
                    case BiomeSoundType.Grass:
                        soundPlayer.PlayClip(SeasonController.Instance.CurrentSeasonProperties.GrassSoundEffect);
                        break;
                    case BiomeSoundType.Swamp:
                        soundPlayer.PlayClip(SeasonController.Instance.CurrentSeasonProperties.SwampSoundEffect);
                        break;
                    case BiomeSoundType.Desert:
                        soundPlayer.PlayClip(SeasonController.Instance.CurrentSeasonProperties.DesertSoundEffect);
                        break;
                    case BiomeSoundType.Ocean:
                        soundPlayer.PlayClip(SeasonController.Instance.CurrentSeasonProperties.OceanSoundEffect);
                        break;
                    case BiomeSoundType.Forest:
                        soundPlayer.PlayClip(SeasonController.Instance.CurrentSeasonProperties.ForestSoundEffect);
                        break;
                    default:
                        break;
                }
                CurrentBiomeSoundType = mostPrevalentBiome;
            }
        }
    }
    private void UpdateBiomeCount(BiomeType biome, Dictionary<BiomeSoundType, int> counts, int increment)
    {
        switch (biome)
        {
            case BiomeType.None:
                counts[BiomeSoundType.Ocean] += increment;
                break;
            case BiomeType.Woodlands:
            case BiomeType.Plains:
            case BiomeType.Forest:
            case BiomeType.Highlands:
            case BiomeType.Mountains:
            case BiomeType.Hills:
            case BiomeType.Scrublands:
                counts[BiomeSoundType.Grass] += increment;
                break;
            case BiomeType.Marsh:
            case BiomeType.Wetlands:
                counts[BiomeSoundType.Swamp] += increment;
                break;
            case BiomeType.RedDesertForest:
            case BiomeType.RedDesertMountain:
            case BiomeType.RedDirt:
            case BiomeType.YellowDesertForest:
            case BiomeType.YellowDesertForestBurned:
            case BiomeType.YellowDesertHills:
            case BiomeType.YellowDesertMountain:
            case BiomeType.Wasteland:
            case BiomeType.Desert:
                counts[BiomeSoundType.Desert] += increment + 2;
                break;
            case BiomeType.SwampJungle:
            case BiomeType.Jungle:
                counts[BiomeSoundType.Forest] += increment;
                break;
            default:
                // Handle unexpected biome types if needed
                break;
        }
    }
    private BiomeSoundType GetMostPrevalentBiome(Dictionary<BiomeSoundType, int> counts)
    {
        BiomeSoundType mostPrevalent = BiomeSoundType.Nothing;
        int maxCount = 0;

        foreach (var kvp in counts)
        {
            if (kvp.Value > maxCount)
            {
                mostPrevalent = kvp.Key;
                maxCount = kvp.Value;
            }
        }

        return mostPrevalent;
    }
}
