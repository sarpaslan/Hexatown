using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
public class MilestoneSelection
{
    public string Name;
    public string Description;
    public SpriteIcon Icon;
}


public class Milestone
{
    public int Id;
    public Action<int> OnSelected;
    public MilestoneSelection[] Selections;
}

public class ScreenButton
{
    public UnityEngine.Sprite Icon;
    public bool Interactable;
    public string Text;
}

public class ScreenIcon
{
    public int Id;
    public UnityEngine.Sprite Icon;
    public Vector3 WorldPosition;
    public GameObject Object;

    public bool IsExist;
}
public class TransformLabel
{
    public int Id;
    public Transform Target;
    public string Text;
    public GameObject Object;
    public bool IsExist;
    public TMP_Text TargetText;
    public float yOffset;
}
public class GameScreen : MonoBehaviour
{
    private Dictionary<int, ScreenIcon> m_screenIcons = new();
    public Camera Camera;
    public Image ScreenIconTemplate;
    public Transform Canvas;
    public UnlockedBiomesView UnlockedBiomesViewTemplate;
    private UnlockedBiomesView m_unlockedBiomesView;
    public static GameScreen Instance;
    public TMP_Text FLoatingTextTemplate;
    public PopupItemBehaviour PopupItemPrefab;
    public Button CircleButton;
    public GameObject LabelTextPrefab;
    public MilestoneReachedView m_mileStoneReachedPrefab;
    public GameControlView GameControlPrefab;
    private List<TransformLabel> m_transformLabels = new List<TransformLabel>();
    private Dictionary<int, Button[]> m_screenSelectionButtonIcons = new Dictionary<int, Button[]>();
    private int m_temp;

    [Header("Speed")]
    public Button m_increaseSpeedButton;
    public Button m_slowDownSpeedButton;
    public Button m_stopButton;
    public UnityEngine.Sprite m_playIcon;
    public UnityEngine.Sprite m_pauseIcon;
    public TMP_Text m_speedInfoText;
    public InfoBox InfoBoxPrefab;
    public Transform InfoBoxContainer;

    public GameObject Cards;
    public ObjectPool<PopupItemBehaviour> m_popupItems;

    public InfoBox ShowInfo(string text, string description)
    {
        var info = Instantiate(InfoBoxPrefab, InfoBoxContainer);
        info.Title.text = text;
        info.Description.text = description;
        return info;
    }

    public void ShowInput(bool interactable, string title, string description, string input, Action<string> onValueSet = null)
    {
        var box = ShowInfo(title, description);
        var inp = box.CreateInput();
        inp.InputField.text = input;
        inp.InputField.interactable = interactable;
        inp.InputField.onEndEdit.AddListener(_ =>
         {
             onValueSet?.Invoke(inp.InputField.text);
             Destroy(box.gameObject);
         });
    }


    public void CreateGameControlView()
    {
        Instantiate(GameControlPrefab, transform);
    }
    public void OnPlaceCreated(Place place)
    {
        if (m_temp == -1)
            return;

        // var total = GameController.CountOf(PlaceType.VILLAGE) + GameController.CountOf(PlaceType.FARMLAND);
        // if (total > 12)
        // {
        //     m_temp = -1;
        //     var milestone = new Milestone();
        //     milestone.Selections = new MilestoneSelection[2]
        //     {
        //             new MilestoneSelection(){
        //                Name = "Eat the peels" ,
        //                Description = "Villagers eat %10 less food",
        //                Icon = SpriteIcon.CowMeat
        //             },
        //             new MilestoneSelection()
        //             {
        //                 Name = "Less talking while working",
        //                 Description = "Villagers walks and carries 20% faster",
        //                 Icon = SpriteIcon.Gather
        //             }
        //     };
        //     milestone.OnSelected += (i) =>
        //     {
        //         if (i == 0)
        //         {
        //             GameConfig.VILLAGER_REQEST_CONSUME_TICK -= 1;
        //         }
        //         else
        //         {
        //             GameConfig.VILLAGER_SPEED += 0.20f;
        //         }
        //     };
        //     ShowMilestoneReached(milestone);
        // }
    }
    private void Start()
    {
        GameController.OnPlaceCreated += OnPlaceCreated;
        m_popupItems = new ObjectPool<PopupItemBehaviour>(Create, Get, Release);
        PopupItemBehaviour Create()
        {
            return Instantiate(PopupItemPrefab);
        }
        void Get(PopupItemBehaviour behaviour)
        {
            behaviour.Background.color = Color.white;
            behaviour.Icon.color = Color.white;
            behaviour.gameObject.SetActive(true);
        }

        void Release(PopupItemBehaviour behaviour)
        {
            behaviour.gameObject.SetActive(false);
        }
    }

    private async UniTaskVoid PopupItemAtLocation(Vector3 world, SpriteIcon icon)
    {
        var popupItem = m_popupItems.Get();
        popupItem.transform.position = world + new Vector3(0, 0.2f, 0);
        popupItem.Icon.sprite = icon.ToSprite();
        popupItem.transform.DOMoveY(popupItem.transform.position.y + 0.5f, 0.5f).SetUpdate(true);
        popupItem.Background.DOColor(Color.clear, 0.5f).SetUpdate(true).SetDelay(0.5f).SetUpdate(true);
        popupItem.Icon.DOColor(Color.clear, 0.5f).SetUpdate(true).SetDelay(0.5f).SetUpdate(true);
        await UniTask.WaitForSeconds(1, true);
        m_popupItems.Release(popupItem);
    }

    private void OnDestroy()
    {
        GameController.OnPlaceCreated -= OnPlaceCreated;
    }
    void Awake()
    {
        Instance = this;

        m_increaseSpeedButton.onClick.AddListener(() =>
        {
            SpeedUpTime();
        });

        m_slowDownSpeedButton.onClick.AddListener(() =>
        {
            SlowDownTime();
        });
        m_stopButton.onClick.AddListener(() =>
        {
            if (Time.timeScale > 0)
            {
                Time.timeScale = 0;
                m_speedInfoText.text = "Paused";
                m_increaseSpeedButton.gameObject.SetActive(false);
                m_slowDownSpeedButton.gameObject.SetActive(false);
                m_stopButton.transform.GetChild(0).
                GetComponent<Image>().sprite = m_playIcon;
                Cards.gameObject.SetActive(false);
            }
            else
            {
                Time.timeScale = 1;
                m_increaseSpeedButton.gameObject.SetActive(true);
                m_slowDownSpeedButton.gameObject.SetActive(true);
                m_stopButton.transform.GetChild(0).
                GetComponent<Image>().sprite = m_pauseIcon;
                Cards.gameObject.SetActive(true);
                m_speedInfoText.text = "x" + Time.timeScale;
            }
        });
    }
    private void SpeedUpTime()
    {
        if (Time.timeScale == 3)
        {
            Time.timeScale = 1;
            m_increaseSpeedButton.gameObject.SetActive(true);
            m_slowDownSpeedButton.gameObject.SetActive(false);
            m_speedInfoText.text = "x" + Time.timeScale;
            return;
        }

        if (Time.timeScale < 3)
        {
            Time.timeScale += 1;
            m_slowDownSpeedButton.gameObject.SetActive(true);
        }
        if (Time.timeScale == 3)
            m_increaseSpeedButton.gameObject.SetActive(false);

        m_speedInfoText.text = "x" + Time.timeScale;
    }

    private void SlowDownTime()
    {
        if (Time.timeScale > 1)
        {
            Time.timeScale -= 1;
            m_increaseSpeedButton.gameObject.SetActive(true);
        }

        if (Time.timeScale == 1)
            m_slowDownSpeedButton.gameObject.SetActive(false);

        m_speedInfoText.text = "x" + Time.timeScale;
    }

    public async void ShowLabelAsync(Villager villager, string text, float yOffset = 0)
    {
        villager.HasLabel = true;
        var transform = villager.Transform;
        var id = ShowLabel(transform, text, yOffset);
        var vlg = transform.GetComponent<VillagerBehaviour>();
        vlg.LabelId = id;
        await UniTask.WaitForSeconds(6, true);
        HideLabel(id);
        villager.HasLabel = false;
    }

    public int ShowLabel(Transform transform, string text, float yOffset = 0)
    {
        int id = UnityEngine.Random.Range(0, 100000000);
        var labelInst = new TransformLabel()
        {
            Id = id,
            Target = transform,
            Text = text,
            yOffset = yOffset,
        };
        m_transformLabels.Add(labelInst);
        return id;
    }

    public void UpdateLabel(int labelId, string v)
    {
        for (int i = 0; i < m_transformLabels.Count; i++)
        {
            if (m_transformLabels[i].Id == labelId)
            {
                m_transformLabels[i].Text = v;
                break;
            }
        }
    }
    public void ShowError(int id, Vector3 target)
    {
        if (m_screenIcons.ContainsKey(id))
            return;

        m_screenIcons.Add(id, new ScreenIcon()
        {
            Icon = GameController.Resources.GetIcon(SpriteIcon.Error),
            Id = id,
            WorldPosition = target,
            IsExist = false,
            Object = null
        });
    }


    public Image ShowImage(Vector3 target, SpriteIcon icon)
    {
        var gm = Instantiate(ScreenIconTemplate, Canvas.transform);
        gm.sprite = GameController.Resources.GetIcon(icon);
        var position = Camera.WorldToScreenPoint(target);
        gm.transform.position = position;
        return gm;
    }

    public void ShowUnlockedBiomes()
    {
        if (m_unlockedBiomesView != null)
        {
            return;
        }
        m_unlockedBiomesView = Instantiate(UnlockedBiomesViewTemplate, transform);
    }
    public void HideUnlockedBiomes()
    {
        if (m_unlockedBiomesView != null)
        {
            Destroy(m_unlockedBiomesView.gameObject);
            m_unlockedBiomesView = null;
        }
    }

    public void Hide(int id)
    {
        if (m_screenIcons.TryGetValue(id, out var icon))
        {
            if (icon.IsExist)
            {
                Destroy(icon.Object);
                m_screenIcons.Remove(id);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.timeScale > 0)
                SpeedUpTime();
        }
    }
    public void ShowMilestoneReached(Milestone mileStone)
    {
        var view = Instantiate(m_mileStoneReachedPrefab, transform);
        view.SetMilestone(mileStone);
    }

    public void LateUpdate()
    {
        foreach (var icon in m_screenIcons)
        {
            var screenIcon = icon.Value;
            if (!screenIcon.IsExist)
            {
                var gm = Instantiate(ScreenIconTemplate, Canvas.transform);
                gm.sprite = screenIcon.Icon;
                gm.transform.SetAsFirstSibling();
                screenIcon.Object = gm.gameObject;
                screenIcon.IsExist = true;
            }
            var position = Camera.WorldToScreenPoint(screenIcon.WorldPosition);
            float padding = 16;
            if (position.x > Screen.width)
                position.x = Screen.width - padding;
            if (position.x < padding)
                position.x = padding;
            if (position.y > Screen.height - padding)
                position.y = Screen.height - padding;
            if (position.y < padding)
                position.y = padding;

            screenIcon.Object.transform.position = position;
        }

        for (int i = 0; i < m_transformLabels.Count; i++)
        {
            if (!m_transformLabels[i].IsExist)
            {
                var label = Instantiate(LabelTextPrefab, Canvas.transform);
                m_transformLabels[i].TargetText = label.transform.GetChild(1).GetComponent<TMP_Text>();
                m_transformLabels[i].Object = label;
                m_transformLabels[i].IsExist = true;
            }
            if (m_transformLabels[i].Target == null)
            {
                Debug.Log("target is null");
                HideLabel(m_transformLabels[i].Id);
                continue;
            }
            var targetPos = m_transformLabels[i].Target.position;
            var position = Camera.WorldToScreenPoint(new Vector3(targetPos.x, targetPos.y + m_transformLabels[i].yOffset, targetPos.z));
            m_transformLabels[i].Object.transform.position = position;
            m_transformLabels[i].TargetText.text = m_transformLabels[i].Text;
        }
    }
    public void ShowText(string text, Color color, Vector3 worldPosition)
    {
        var txtObj = Instantiate(FLoatingTextTemplate, transform);
        txtObj.text = text;
        txtObj.color = color;
        txtObj.transform.position = worldPosition;
        txtObj.transform.DOMoveY(txtObj.transform.position.y + 2, 2f).SetDelay(0.5f);
        txtObj.DOFade(0, 2).SetDelay(1.5f);
        Destroy(txtObj.gameObject, 3f);
    }
    public int ShowButtonIconCircle(Selection[] selections, Vector3 center, Action<Selection> onResult)
    {
        float angleStep = 360f / selections.Length;
        float radius = 50;
        if (Screen.height > Screen.width)
        {
            radius = 100;
        }
        int id = LittleRandom.GetRandomInt();
        Button[] buttons = new Button[selections.Length];
        for (int i = 0; i < selections.Length; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            var targetPos = new Vector3(center.x + Mathf.Cos(angle) * radius, center.y + Mathf.Sin(angle) * radius, center.z);
            buttons[i] = Instantiate(CircleButton, GameScreen.Instance.transform);
            buttons[i].name = selections[i].Name;
            buttons[i].transform.localScale = Vector3.zero;
            buttons[i].transform.DOScale(1f, 0.2f);
            buttons[i].transform.GetChild(1).GetComponent<Image>().sprite = selections[i].Icon;
            int x = i;
            buttons[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                HideSelection(ref id, selections[x]);
                onResult?.Invoke(selections[x]);
            });
            buttons[i].transform.position = targetPos;
        }
        m_screenSelectionButtonIcons.Add(id, buttons);
        return id;
    }

    public void HideButtonIcon(int id)
    {
        HideSelection(ref id, null);
    }

    public void HideSelection(ref int id, Selection selected)
    {
        if (id == -1)
            return;
        var buttons = m_screenSelectionButtonIcons[id];
        for (int i1 = 0; i1 < buttons.Length; i1++)
        {
            Button btn = buttons[i1];
            if (btn)
            {
                if (selected != null)
                {
                    btn.interactable = false;
                    if (btn.name == selected.Name)
                    {
                        btn.transform.DOScale(Vector3.one * 1.2f, 0.2f).SetUpdate(true).SetEase(Ease.InBounce).OnComplete(() =>
                        {
                            btn.transform.DOScale(Vector3.zero, 0.1f).SetUpdate(true);
                        });
                    }
                    else
                    {
                        btn.transform.DOScale(Vector3.zero, 0.1f).SetUpdate(true);
                        Destroy(btn.gameObject, 0.1f);
                    }
                }
                else
                {
                    Destroy(btn.gameObject);
                }
            }
        }
        m_screenSelectionButtonIcons.Remove(id);
        id = -1;
    }

    public int ShowButtonIcon(Selection selection, Vector3 center, Action value)
    {
        var clickableButton = Instantiate(CircleButton, transform);
        int index = LittleRandom.GetRandomInt();
        m_screenSelectionButtonIcons.Add(index, new Button[1]
        {
            clickableButton
        });
        clickableButton.transform.position = center;
        clickableButton.transform.GetChild(1).GetComponent<Image>().sprite = selection.Icon;
        clickableButton.transform.localScale = Vector3.zero;
        clickableButton.transform.DOScale(0.8f, 0.2f);
        clickableButton.GetComponent<Button>().onClick.AddListener(() =>
         {
             value?.Invoke();
             Destroy(clickableButton.gameObject);
         });
        return index;
    }

    internal void HideLabel(int id)
    {
        var first = m_transformLabels.FirstOrDefault(t => t.Id == id);
        if (first != null)
        {
            if (first.IsExist)
            {
                Destroy(first.Object);
            }
            m_transformLabels.Remove(first);
        }
    }
    public static void PopupItem(Vector3 location, Stats stats)
    {
        PopupItem(location, GameController.Resources.ToIconType(stats));
    }
    public static void PopupItem(Vector3 location, SpriteIcon icon)
    {
        Instance.PopupItemAtLocation(location, icon).Forget(); ;
    }
}
