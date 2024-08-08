using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;

[Flags]
public enum Season
{
    None = 0,
    Spring = 1 << 0,
    Summer = 1 << 1,
    Autumn = 1 << 2,
    Winter = 1 << 3,
}

[Serializable]
public struct KeyValueStringFloat
{
    public string Key;
    public AnimationCurve Curve;
}
[Serializable]
public class SeasonMaterialProperties
{
    public Season Season;
    public Material Material;
    public KeyValueStringFloat[] Properties;
    public ParticleSystem SeasonParticleObject;
    public float SeasonFireChance = 1;

    [SerializeField]
    public AudioClip GrassSoundEffect;
    [SerializeField]
    public AudioClip SwampSoundEffect;
    [SerializeField]
    public AudioClip DesertSoundEffect;
    [SerializeField]
    public AudioClip OceanSoundEffect;

    [SerializeField]
    public AudioClip ForestSoundEffect;
}
public class SeasonController : MonoBehaviour
{
    public SeasonMaterialProperties CurrentSeasonProperties;
    public Season CurrentSeason = Season.Spring;
    public TMP_Text SeasonText;
    [NonSerialized]
    public int Day = 1;
    [NonSerialized]
    public int DaysPerSeason = 30;

    [NonSerialized]
    public int Year = 0;
    private int m_seasonPassed;
    private bool m_started;
    public SeasonMaterialProperties[] Seasons;
    private static SeasonController m_intance;
    public static SeasonController Instance
    {
        get
        {
            if (m_intance == null)
            {
                m_intance = FindObjectOfType<SeasonController>();
            }
            return m_intance;
        }
    }
    public static Action<Season> OnSeasonChanged;
    public void Awake()
    {
        m_intance = this;
    }
    public void Start()
    {
        GameController.Instance.OnGameTick += OnTickListener;
        ResetSeasonMaterials();
        UpdateTiles();
    }

    public void OnApplicationQuit()
    {
        ResetSeasonMaterials();
    }

    public void SetSeason(Season season)
    {
        CurrentSeason = season;
        CurrentSeasonProperties = Seasons.FirstOrDefault(s => s.Season == CurrentSeason);
        ResetSeasonMaterials();
    }

    public void ResetSeasonMaterials()
    {
        foreach (var property in CurrentSeasonProperties.Properties)
        {
            float normalizedDay = (float)Day / DaysPerSeason;
            var propertyValue = property.Curve.Evaluate(normalizedDay);
            CurrentSeasonProperties.Material.SetFloat(property.Key, propertyValue);
        }
    }

    public void UpdateTileMaterial(Material material)
    {
        var foreground = TileMapController.Instance.Foreground.GetComponent<TilemapRenderer>();
        var places = TileMapController.Instance.Placed.GetComponent<TilemapRenderer>();
        foreground.material = places.material = material;
    }

    public void OnDisable()
    {
        GameController.Instance.OnGameTick -= OnTickListener;
    }

    private void OnTickListener(int tick)
    {
        if (!m_started) return;
        if (tick % GameConfig.DAY_CHANGE_PER_TICK != 0)
        {
            return;
        }
        Day += 1;

        if (Day >= DaysPerSeason)
        {
            NextSeason();
        }
        UpdateTiles();
    }
    public void NextSeason()
    {
        ResetSeasonMaterials();
        Day = 1;
        m_seasonPassed++;
        if (m_seasonPassed % 4 == 0)
        {
            Year++;
            m_seasonPassed = 0;
        }
        switch (CurrentSeason)
        {
            case Season.None:
                CurrentSeason = Season.Spring;
                break;
            case Season.Spring:
                CurrentSeason = Season.Summer;
                break;
            case Season.Summer:
                CurrentSeason = Season.Autumn;
                break;
            case Season.Autumn:
                CurrentSeason = Season.Winter;
                break;
            case Season.Winter:
                CurrentSeason = Season.Spring;
                break;
        }
        OnSeasonChanged?.Invoke(CurrentSeason);
    }

    public Material GetCurrentMaterial() => CurrentSeasonProperties.Material;
    public AnimationCurve ParticleCountAnimationCurve;

    public void UpdateTiles()
    {
        foreach (var s in Seasons)
        {
            if (s.Season != CurrentSeason)
            {
                if (s.SeasonParticleObject)
                {
                    s.SeasonParticleObject.gameObject.SetActive(false);
                }
            }
        }

        CurrentSeasonProperties = Seasons.FirstOrDefault(s => s.Season == CurrentSeason);

        if (CurrentSeasonProperties.SeasonParticleObject != null)
        {
            CurrentSeasonProperties.SeasonParticleObject.gameObject.SetActive(true);
            var emmision = CurrentSeasonProperties.SeasonParticleObject.emission;
            var main = CurrentSeasonProperties.SeasonParticleObject.main;
            main.maxParticles = (int)ParticleCountAnimationCurve.Evaluate((float)Day / DaysPerSeason);
        }

        if (CurrentSeasonProperties != null)
        {
            foreach (var property in CurrentSeasonProperties.Properties)
            {
                float normalizedDay = (float)Day / DaysPerSeason;
                var propertyValue = property.Curve.Evaluate(normalizedDay);
                CurrentSeasonProperties.Material.DOFloat(propertyValue, property.Key, GameConfig.DAY_CHANGE_PER_TICK / 2f).
                SetEase(Ease.Linear);
                //TODO REFACTOR LATER
            }
        }

        var seasonText = "";
        switch (CurrentSeasonProperties.Season)
        {
            case Season.Spring:
                seasonText += $"<color=yellow>{CurrentSeasonProperties.Season}</color>";
                break;
            case Season.Summer:
                seasonText += $"<color=green>{CurrentSeasonProperties.Season}</color>";
                break;
            case Season.Autumn:
                seasonText += $"<color=orange>{CurrentSeasonProperties.Season}</color>";
                break;
            case Season.Winter:
                seasonText += $"<color=white>{CurrentSeasonProperties.Season}</color>";
                break;
        }
        SeasonText.text = $"Day {Day} of {seasonText}\nYear {GameConfig.START_YEAR + Year}";
        UpdateTileMaterial(CurrentSeasonProperties.Material);
    }

    public void StartSeason()
    {
        m_started = true;
    }

}