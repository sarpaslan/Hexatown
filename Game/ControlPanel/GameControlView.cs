using System;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
public class TableItem
{
    public int Id;
    public string[] Items;
}

public class Table
{
    public string[] Header;
    public TableItem[] Items;
}


public enum TableDisplay
{
    NONE,
    PEOPLES,
    PLACES,
    GRAVE,
    CONFIG,
}
public class GameControlView : MonoBehaviour
{

    public TMP_Text TableTextPrefab;
    public Transform TableContainer;

    public TableItemView TableItemPefab;
    public TableView TableViewPrefab;

    private TableView m_currentTableView;
    public static TableDisplay tableDisplay = TableDisplay.PEOPLES;

    public TMP_InputField m_searchBar;

    public TMP_Text m_countText;


    void OnEnable()
    {
        GameController.Instance.OnGameTick += OnGameTick;
        if (tableDisplay == TableDisplay.PEOPLES)
            DisplayVillagers();
        else if (tableDisplay == TableDisplay.PLACES)
            DisplayPlaces();
        else if (tableDisplay == TableDisplay.GRAVE)
            DisplayGraves();
        else if (tableDisplay == TableDisplay.CONFIG)
            DisplayGameConfig();
    }



    private void CreateGravesTable()
    {
        var villagers = VillagerController.Villagers.Where(t => t.Dead).ToList();
        int total = villagers.Count;
        for (int i = villagers.Count - 1; i >= 0; i--)
        {
            int x = i;
            m_currentTableView.Refresh(villagers[i].Id, () =>
            {
                DisplayFields(villagers[x].GetType(), villagers[x]);
            },
             null,
            villagers[i].Name, villagers[i].Age + "", villagers[i].DeadReason);
            if (!string.IsNullOrEmpty(m_searchBar.text))
            {
                string concat = villagers[i].Name + " " + villagers[i].Age + " " + villagers[i].DeadReason;
                if (concat.Contains(m_searchBar.text, StringComparison.InvariantCultureIgnoreCase))
                {
                    m_currentTableView.Show(villagers[i].Id);
                }
                else
                {
                    m_currentTableView.Hide(villagers[i].Id);
                    total--;
                }
            }
            else
            {
                m_currentTableView.Show(villagers[i].Id);
            }
        }
        m_countText.text = total + "/" + villagers.Count;
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    private void Start()
    {
    }


    public void CreateVillagerTable()
    {
        var villagers = VillagerController.Villagers.Where(t => !t.Dead).ToList();
        int total = villagers.Count;
        for (int i = villagers.Count - 1; i >= 0; i--)
        {
            if (villagers[i].Dead) continue;
            int x = i;
            m_currentTableView.Refresh(villagers[i].Id, () =>
            {
                DisplayFields(villagers[x].GetType(), villagers[x]);
            }
            , null,
            villagers[i].Name, villagers[i].Age + "", villagers[i].Hunger + "", villagers[i].Work?.ToString(), villagers[i].Query);
            if (!string.IsNullOrEmpty(m_searchBar.text))
            {
                string concat = villagers[i].Name + " " + villagers[i].Age + " " + villagers[i].Query;
                if (concat.Contains(m_searchBar.text, StringComparison.InvariantCultureIgnoreCase))
                {
                    m_currentTableView.Show(villagers[i].Id);
                }
                else
                {
                    m_currentTableView.Hide(villagers[i].Id);
                    total--;
                }
            }
            else
            {
                m_currentTableView.Show(villagers[i].Id);
            }
        }
        m_countText.text = total + "/" + villagers.Count;
    }
    public void CreatePlacesTable()
    {
        var places = GameController.Places;
        int total = places.Count;
        for (int i = places.Count - 1; i >= 0; i--)
        {
            int x = i;
            m_currentTableView.Refresh(places[i].Id, () =>
            {
                DisplayFields(places[x].GetType(), places[x]);
            },
             null,
            places[i].Name, places[i].Tax > 0 ? "+" + places[i].Tax : places[i].Tax + "", places[i].BiomeType + "", places[i].Fire + "");
            if (!string.IsNullOrEmpty(m_searchBar.text))
            {
                string concat = places[i].Card.Name + " " + places[i].BiomeType;
                if (concat.Contains(m_searchBar.text, StringComparison.InvariantCultureIgnoreCase))
                {
                    m_currentTableView.Show(places[i].Id);
                }
                else
                {
                    m_currentTableView.Hide(places[i].Id);
                    total--;
                }
            }
            else
            {
                m_currentTableView.Show(places[i].Id);
            }
        }
        m_countText.text = total + "/" + places.Count;
    }
    private void OnGameTick(int obj)
    {
        if (tableDisplay == TableDisplay.PEOPLES)
        {
            CreateVillagerTable();
        }
        else if (tableDisplay == TableDisplay.PLACES)
        {
            CreatePlacesTable();
        }
        else if (tableDisplay == TableDisplay.CONFIG)
        {
            CreateGameConfigTable();
        }
    }
    public void DisplayFields(Type type, System.Object obj)
    {
        var properties = GameScreen.Instance.ShowInfo($"Properties", "");
        foreach (var field in type.GetFields())
        {
            if (field.HasAttribute<UsefullInfoAttribute>())
            {
                var attr = field.GetAttribute<UsefullInfoAttribute>();
                if (attr.EditorOnly && !Application.isEditor) continue;
                var str = field.GetValue(obj);
                if (str == null)
                {
                    str = "Null";
                }
                var btn = properties.CreateInput();
                btn.Title = field.Name + (attr.EditorOnly ? "*" : "");
                btn.InputField.interactable = attr.IsCheatToChange ? GameConfig.CHEATS_ENABLED : !attr.ReadOnly;

                btn.InputField.text = str.ToString();
                btn.InputField.onEndEdit.AddListener((str) =>
                {
                    Helper.SetFieldValue(obj, field, str);
                });
            }
        }
    }
    public void DisplayGameConfig()
    {
        tableDisplay = TableDisplay.CONFIG;
        if (m_currentTableView == null)
        {
            m_currentTableView = Instantiate(TableViewPrefab, TableContainer);
        }
        else
        {
            m_currentTableView.Reset();
        }
        m_currentTableView.CreateHeader("Name", "Value");
        CreateGameConfigTable();
    }

    private void CreateGameConfigTable()
    {
        var type = typeof(GameConfig);

        var fields = type.GetFields().Where(t => t.HasAttribute<UsefullInfoAttribute>()).ToArray();
        int total = fields.Length;
        for (var v = 0; v < fields.Length; v++)
        {
            var field = fields[v];
            var attr = field.GetAttribute<UsefullInfoAttribute>();
            if (attr.EditorOnly)
            {
                if (!Application.isEditor)
                    continue;
            }

            var x = v;
            m_currentTableView.Refresh(v, () =>
            {
                GameScreen.Instance.ShowInput(attr.IsCheatToChange ? GameConfig.CHEATS_ENABLED : !attr.ReadOnly, fields[x].Name,
                attr.Description, fields[x].GetValue(null).ToString(), (str) =>
                {
                    Helper.SetFieldValue(null, fields[x], str);
                });
            }, () =>
            {
                if (field.HasAttribute<CategoryAttribute>())
                {
                    var category = field.GetAttribute<CategoryAttribute>();
                    m_currentTableView.CreateCategory(category.Title);
                }
            },
             $"{fields[x].Name}", $"{fields[x].GetValue(null)}");

            if (!string.IsNullOrEmpty(m_searchBar.text))
            {
                string concat = field.Name;
                if (concat.Contains(m_searchBar.text, StringComparison.InvariantCultureIgnoreCase))
                {
                    m_currentTableView.Show(v);
                }
                else
                {
                    m_currentTableView.Hide(v);
                    total--;
                }
            }
            else
            {
                m_currentTableView.Show(v);
            }
        }
        m_countText.text = total + "/" + fields.Length;

    }

    public void DisplayVillagers()
    {
        tableDisplay = TableDisplay.PEOPLES;
        if (m_currentTableView == null)
        {
            m_currentTableView = Instantiate(TableViewPrefab, TableContainer);
        }
        else
        {
            m_currentTableView.Reset();
        }
        m_currentTableView.CreateHeader("Name", "Age", "Hunger", "Work", "Action");
        CreateVillagerTable();
    }
    public void DisplayPlaces()
    {
        tableDisplay = TableDisplay.PLACES;
        if (m_currentTableView == null)
        {
            m_currentTableView = Instantiate(TableViewPrefab, TableContainer);
        }
        else
        {
            m_currentTableView.Reset();
        }
        m_currentTableView.CreateHeader("Name", "Cost", "Biome", "Fire");
        CreatePlacesTable();
    }
    public void DisplayGraves()
    {
        tableDisplay = TableDisplay.GRAVE;
        if (m_currentTableView == null)
        {
            m_currentTableView = Instantiate(TableViewPrefab, TableContainer);
        }
        else
        {
            m_currentTableView.Reset();
        }
        m_currentTableView.CreateHeader("Name", "Age", "Died from");
        CreateGravesTable();
    }


    void OnDisable()
    {
        GameController.Instance.OnGameTick -= OnGameTick;
    }

}
