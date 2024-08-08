using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeysController : MonoBehaviour
{
    public const KeyCode SHIFT = KeyCode.LeftShift;
    public const KeyCode KILL = KeyCode.Q;
    public const KeyCode ADD_RESOURCES = KeyCode.A;
    public const KeyCode ADD_GOLD = KeyCode.G;
    public const KeyCode FIRE_PLACE = KeyCode.F;
    public const KeyCode DESTROY_PLACE = KeyCode.X;
    public const KeyCode BURNT_PLACE = KeyCode.B;
    public const KeyCode NEXT_SEASON = KeyCode.N;
    public const KeyCode SHOW_PLACE_INFO = KeyCode.I;
    public const KeyCode DISPLAY_PLACE_COUNTS = KeyCode.Alpha0;
    public const KeyCode SUPER_KEY = KeyCode.Return;
    public const KeyCode UNLOCK_ALL = KeyCode.Alpha9;
    public const KeyCode SAVE_TEXT = KeyCode.T;
    public TMP_InputField SaveTextObject;

    private void AddResources()
    {
        GameController.Stats.Add(global::Stats.WOOD, 1000);
        GameController.Stats.Add(global::Stats.BREAD, 1000);
        GameController.Stats.Add(global::Stats.FISH, 1000);
        GameController.Stats.Add(global::Stats.CLOTHES, 1000);
        GameController.Stats.Add(global::Stats.POTATO, 1000);
        GameController.Stats.Add(global::Stats.WOOL, 1000);
        GameController.Stats.Add(global::Stats.WHEAT, 1000);
        GameController.Stats.Add(global::Stats.SOAP, 1000);
        GameController.Stats.Add(global::Stats.COAL, 1000);
        GameController.Stats.Add(global::Stats.EGG, 1000);
        GameController.Stats.Add(global::Stats.CLAY, 1000);
        GameController.Stats.Add(global::Stats.BRICK, 1000);
    }
    private void RemoveResources()
    {
        GameController.Stats.Set(global::Stats.WOOD, 0);
        GameController.Stats.Set(global::Stats.BREAD, 0);
        GameController.Stats.Set(global::Stats.FISH, 0);
        GameController.Stats.Set(global::Stats.CLOTHES, 0);
        GameController.Stats.Set(global::Stats.POTATO, 0);
        GameController.Stats.Set(global::Stats.WOOL, 0);
        GameController.Stats.Set(global::Stats.WHEAT, 0);
        GameController.Stats.Set(global::Stats.SOAP, 0);
        GameController.Stats.Set(global::Stats.COAL, 0);
        GameController.Stats.Set(global::Stats.EGG, 0);
        GameController.Stats.Set(global::Stats.CLAY, 0);
        GameController.Stats.Set(global::Stats.BRICK, 0);
    }
    public void AddGold()
    {
        GameController.Stats.Add(global::Stats.GOLD, 1000);
    }

    private void KillRandomVillager()
    {
        var place = GameController.GetPlace(TileSelectionController.Instance.SelectedTile);
        Villager villager = null;

        if (place is PopulationPlace populationPlace)
        {
            villager = populationPlace.Villagers.SelectRandom();
        }
        else if (place is WorkPlace workPlace)
        {
            villager = workPlace.Villagers.SelectRandom();
        }

        if (villager != null)
        {
            VillagerController.Instance.Kill(villager, "By command", true);
        }
    }

    private void ShowTileInfo()
    {
        if (TileInfoController.lastSelection != null)
        {
            GameScreen.Instance.ShowInfo("Tile Info", TileInfoController.lastSelection.ToUsefullString());
        }
    }
    private void Update()
    {
        if (Input.GetKey(SHIFT))
        {
            HandleDebugInput();
            var pos = TileMapController.Instance.GetCellScreen(Input.mousePosition);
            var biome = Biome.GetBiome(pos);
            TileInfoController.Instance.m_hoverTileInfoText.text = $"{biome} | {pos.x},{pos.y}";
        }
        if (Input.GetKeyDown(SHIFT))
        {
            VillagerBehaviour[] array = FindObjectsOfType<VillagerBehaviour>();
            for (int i = 0; i < array.Length; i++)
            {
                VillagerBehaviour p = array[i];
                p.ShowLabel();
            }
        }
        if (Input.GetKeyUp(SHIFT))
        {
            VillagerBehaviour[] array = FindObjectsOfType<VillagerBehaviour>();
            for (int i = 0; i < array.Length; i++)
            {
                VillagerBehaviour p = array[i];
                p.HideLabel();
            }
            TileInfoController.Instance.m_hoverTileInfoText.text = $"";
        }
    }

    public static bool IsInputFieldFocused()
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        return obj != null && obj.GetComponent<TMP_InputField>() != null;
    }
    int fps = 0;
    int frameCounter;

    public void OnGUI()
    {
        if (Input.GetKey(SHIFT))
        {
            frameCounter++;
            if (frameCounter >= fps)
            {
                fps = (int)(1 / Time.unscaledDeltaTime);
                frameCounter = 0;
            }
            GUILayout.Label(fps + " fps");

            GUILayout.BeginVertical();
            if (GUILayout.Button("Info (" + SHOW_PLACE_INFO + ")"))
            {
                ShowTileInfo();
            }
            GUILayout.EndVertical();
        }
        if (!GameConfig.CHEATS_ENABLED) return;

        if (Input.GetKey(SHIFT))
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Resources(" + ADD_RESOURCES + ")"))
            {
                AddResources();
            }
            if (GUILayout.Button("Add Gold(" + ADD_GOLD + ")"))
            {
                AddGold();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();

            if (GUILayout.Button("Next Season(" + NEXT_SEASON + ")"))
            {
                NextSeason();
            }
            if (GUILayout.Button("Unlock All(" + UNLOCK_ALL + ")"))
            {
                UnlockAllCards();
            }
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fire(" + FIRE_PLACE + ")"))
            {
                StartFireCurrentPlace();
            }
            if (GUILayout.Button("Destroy(" + DESTROY_PLACE + ")"))
            {
                DestroyCurrentPlace();
            }
            GUILayout.EndHorizontal();
        }
    }

    private void HandleDebugInput()
    {
        if (IsInputFieldFocused()) return;

        if (Input.GetKeyDown(SHOW_PLACE_INFO))
        {
            ShowTileInfo();
        }

        if (!GameConfig.CHEATS_ENABLED)
        {
            return;
        }

        if (Input.GetKeyDown(ADD_RESOURCES))
        {
            AddResources();
        }

        if (Input.GetKeyDown(ADD_GOLD))
        {
            AddGold();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            Application.targetFrameRate = 0;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            Application.targetFrameRate = 7;

        if (Input.GetKeyDown(KeyCode.Alpha3))
            Application.targetFrameRate = 15;

        if (Input.GetKeyDown(KeyCode.Alpha4))
            Application.targetFrameRate = 30;

        if (Input.GetKeyDown(FIRE_PLACE))
        {
            StartFireCurrentPlace();
        }

        if (Input.GetKeyDown(KILL))
        {
            KillRandomVillager();
        }

        if (Input.GetKeyDown(DESTROY_PLACE))
        {
            DestroyCurrentPlace();
        }

        if (Input.GetKeyDown(NEXT_SEASON))
        {
            NextSeason();
        }

        if (Input.GetKeyDown(SUPER_KEY))
        {
            if (prev == -1)
            {
                prev = GameConfig.GAME_TICK_PER_SECONDS;
                GameConfig.GAME_TICK_PER_SECONDS = 0.1f;
            }
            else
            {
                GameConfig.GAME_TICK_PER_SECONDS = prev;
                prev = -1;
            }
        }

        if (Input.GetKeyDown(SAVE_TEXT))
        {
            SaveTextObject.gameObject.SetActive(!SaveTextObject.gameObject.activeSelf);
            SaveTextObject.text = PlayerPrefs.GetString("gameSave");
        }

        if (Input.GetKeyDown(UNLOCK_ALL))
        {
            UnlockAllCards();
        }
    }

    private static void UnlockAllCards()
    {
        foreach (var card in CardController.Instance.ALL_CARDS)
        {
            CardController.Instance.Unlock(card, true);
            CardController.Instance.CreateCardView(card);
        }
    }

    private void StartFireCurrentPlace()
    {
        var place = GameController.GetPlace(TileSelectionController.Instance.SelectedTile);
        if (place == null) return;
        FireController.Instance.CreateFire(place);
    }


    private static void NextSeason()
    {
        SeasonController.Instance.NextSeason();
        SeasonController.Instance.UpdateTiles();
    }

    private static void DestroyCurrentPlace()
    {
        var place = GameController.GetPlace(TileSelectionController.Instance.SelectedTile);
        if (place == null) return;
        place.DestroyPlace();
    }

    float prev = -1;

}
