using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using KeuGames.Sound;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public class CardScenario
{
    public Card[] Cards;
}

[Serializable]
public class BiomeTypeSound
{
    public BiomeType BiomeType;
    public string Clip;
}

public class CardController : MonoBehaviour
{
    public Transform CardContainer;
    public TileCard CardPrefab;
    public List<Card> ALL_CARDS = new List<Card>();

    [SerializeField]
    public List<Card> UnlockedCards = new List<Card>();
    public static CardController Instance;
    private Dictionary<PlaceType, TileCard> m_instantiatedCards = new Dictionary<PlaceType, TileCard>();
    public BiomeTypeSound[] Sounds;
    public GameObject CardUnlockedPrefab;
    public Transform CardUnlockContainer;
    public Transform CardScrollView;
    private Dictionary<PlaceType, Card> m_typeToCardMap;
    private Dictionary<PlaceType, Card> TypeToCardMap
    {
        get
        {
            if (m_typeToCardMap == null)
            {
                m_typeToCardMap = ALL_CARDS.ToDictionary(card => card.Type);
            }
            return m_typeToCardMap;
        }
    }

    public void Awake()
    {
        Instance = this;
        m_instantiatedCards.Clear();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    void OnEnable()
    {
        GameController.Stats.OnValueChanged.AddListener(OnValueChanged);
    }
    public void Unlock(PlaceType type, bool silent)
    {
        if (type == PlaceType.CASTLE) return;
        foreach (var v in this.ALL_CARDS)
            if (v.Type == type)
            {
                Unlock(v, silent);
                break;
            }
    }
    public void Unlock(Card card, bool silent = false)
    {
        if (UnlockedCards.Contains(card))
        {
            Debug.Log("The card is already unlocked" + card.Name);
            return;
        }
        UnlockedCards.Add(card);
        if (!silent)
        {
            var view = Instantiate(CardUnlockedPrefab, CardUnlockContainer);
            view.transform.GetChild(1).GetComponent<TMP_Text>().text = card.Name + "\nUnlocked";
            view.transform.GetChild(0).GetComponent<Image>().sprite = card.Tile.sprite;
            Helper.DestroyUnscaled(view.gameObject, 3);
            GameController.Resources.SoundPlayer.Play("new_card_unlocked");
            CreateCardView(card);
        }
    }

    private void Start()
    {
        CreateCardViews();
    }

    public void CreateCardView(Card card)
    {
        if (m_instantiatedCards.ContainsKey(card.Type))
            return;

        TileCard cardObj = Instantiate(CardPrefab, CardContainer);
        cardObj.name = UnityEngine.Random.Range(10, 2500000) + "";
        cardObj.SetCard(card);
        cardObj.OnDragBegin.RemoveAllListeners();
        cardObj.OnDragBegin.AddListener(OnDragBegin);
        cardObj.OnDragEnd.AddListener(OnDragEnd);
        m_instantiatedCards.Add(card.Type, cardObj);
        OnValueChanged(Stats.NONE);
    }

    private void OnDragEnd(TileCard card)
    {
        if (card.Card.OnlyAllowedOnce)
        {
            var ob = m_instantiatedCards[card.Card.Type];
            Destroy(ob.gameObject);
            m_instantiatedCards.Remove(card.Card.Type);
        }
        else
        {
            card.Show();
        }
    }

    private void OnDragBegin(TileCard card)
    {
        card.Hide();
    }
    public bool Place(Vector3Int pos, PlaceType type, bool silent)
    {
        return Place(pos, TypeToCardMap[type], silent);
    }

    public bool Place(Vector3Int pos, Card card, bool silent)
    {
        if (!TileMapController.Instance.CanPlace(pos, card))
        {
            return false;
        }
        if (!silent)
            TileMapController.Instance.RevealAreaAsync(pos, card.RevealDepth).Forget();
        GameController.CreatePlace(pos, card, silent);
        var biome = Biome.GetBiome(pos);
        if (!silent)
        {
            foreach (var s in Sounds)
            {
                if (s.BiomeType.HasFlag(biome))
                {
                    GameController.Resources.SoundPlayer.Play(s.Clip);
                    break;
                }
            }
        }
        return true;
    }


    //TODO OPTIMIZE THIS LATER
    private void OnValueChanged(Stats changedValue)
    {
        foreach (var cardView in m_instantiatedCards)
        {
            cardView.Value.CheckPrices();
        }
    }

    static IEnumerable<Enum> GetFlags(Enum input)
    {
        IList list = Enum.GetValues(input.GetType());
        for (int i = 0; i < list.Count; i++)
        {
            if (i == 0) continue;
            Enum value = (Enum)list[i];
            if (input.HasFlag(value))
            {
                yield return value;
            }
        }
    }
    private void CreateCardViews()
    {
        ClearCardViews();
        var newList = UnlockedCards.OrderByDescending(t => t.SortOrder);
        foreach (var card in newList)
        {
            CreateCardView(card);
        }
    }
    private void ClearCardViews()
    {
        KeyValuePair<PlaceType, TileCard>[] arr = m_instantiatedCards.ToArray();
        foreach (KeyValuePair<PlaceType, TileCard> ar in arr)
            Destroy(ar.Value.gameObject);
        m_instantiatedCards.Clear();
    }

    public Card GetUnlockedCard(PlaceType type)
    {
        for (int i = 0; i < UnlockedCards.Count; i++)
            if (UnlockedCards[i].Type == type)
                return UnlockedCards[i];
        return null;
    }
    public Card GetCard(PlaceType type)
    {
        for (int i = 0; i < ALL_CARDS.Count; i++)
            if (ALL_CARDS[i].Type == type)
                return ALL_CARDS[i];
        return null;
    }
    public static bool IsUnlocked(PlaceType requiredPlace)
    {
        return Instance.GetUnlockedCard(requiredPlace) != null;
    }

    public static T GetCard<T>() where T : Card
    {
        foreach (var card in Instance.ALL_CARDS)
            if (card.GetType() == typeof(T))
                return (T)card;
        return null;
    }
}
