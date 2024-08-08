using System;
using DG.Tweening;
using KeuGames.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SpriteRenderer DragObjectTemplate;
    private Vector3Int _previousPointerPos;
    private SpriteRenderer _dragObject;
    public Image CardImage;
    public Transform Container;
    [NonSerialized]
    public Card Card;
    public UnityEvent<TileCard> OnDragBegin;
    public UnityEvent<TileCard> OnDragEnd = new();
    public Transform PriceContainer;
    public GameObject PriceTemplate;
    private TMP_Text[] m_priceTexs;
    public bool CanUse = true;
    private bool m_dragging;

    public void SetCard(Card card)
    {
        Container = transform.GetChild(0);
        if (card == null) throw new InvalidOperationException($"Card is null");
        if (card.Tile == null) Debug.LogError($"You are trying to set a tile without sprite {card.Name}");
        CardImage.sprite = card.Tile.sprite;
        Card = card;
        m_priceTexs = new TMP_Text[card.Prices.Length];
        for (int i = 0; i < card.Prices.Length; i++)
        {
            CardPrice price = card.Prices[i];
            var priceTag = Instantiate(PriceTemplate, PriceContainer);
            priceTag.transform.GetChild(0).GetComponent<Image>().sprite =
            GameController.Resources.GetIcon(GameController.Resources.ToIconType(price.Stats));
            var text = priceTag.transform.GetChild(1).GetComponent<TMP_Text>();
            priceTag.transform.GetChild(1).GetComponent<TMP_Text>().text = price.Value + "";
            priceTag.SetActive(true);
            m_priceTexs[i] = text;
        }
        CheckPrices();
    }
    public void CheckPrices()
    {
        var stats = GameController.Stats;
        bool isOn = true;

        for (int i = 0; i < Card.Prices.Length; i++)
        {
            CardPrice price = Card.Prices[i];
            var global = stats.Get(price.Stats);
            if (global < price.Value)
            {
                CardImage.color = Color.white.Alpha(0.5f);
                m_priceTexs[i].color = Color.red;
                isOn = false;
            }
            else
            {
                if (isOn)
                    CardImage.color = Color.white;

                m_priceTexs[i].color = Color.white;
            }
        }
        CanUse = isOn;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanUse)
            return;

        foreach (var price in Card.Prices)
        {
            GameController.Stats.Add(price.Stats, -price.Value);
        }
        if (_dragObject != null)
        {
            Destroy(_dragObject.gameObject);
        }
        _dragObject = Instantiate(DragObjectTemplate);
        Destroy(_dragObject, 40);
        _dragObject.gameObject.SetActive(true);
        _dragObject.material = SeasonController.Instance.GetCurrentMaterial();
        _dragObject.sprite = Card.Tile.sprite;
        m_dragging = true;
        OnDragBegin?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_dragging) return;
        var worldPoint = GameController.Camera.ScreenToWorldPoint(eventData.position);
        _dragObject.transform.position = new Vector3(worldPoint.x, worldPoint.y - 0.64f, 0);
        var pos = TileMapController.Instance.GetCellScreen(eventData.position);
        if (pos != _previousPointerPos)
        {
            TileMapController.Instance.SetBackgroundColor(_previousPointerPos, new Color(1, 1, 1, 0.25f));
            if (TileMapController.Instance.CanPlace(pos, Card))
            {
                TileSelectionController.Instance.Outline(pos, Color.green);
                _dragObject.transform.DOKill();
                _dragObject.transform.localScale = Vector3.one * 0.95f;
                _dragObject.transform.DOScale(Vector3.one * 1.2f, 0.05f).SetEase(Ease.OutBounce).OnComplete(() =>
                {
                    _dragObject.transform.DOScale(Vector3.one * 0.95f, 0.05f);
                });
            }
            else
            {
                TileSelectionController.Instance.Outline(pos, Color.red);
            }
            _previousPointerPos = pos;
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (!m_dragging) return;
        m_dragging = false;
        if (_dragObject != null)
        {
            Destroy(_dragObject.gameObject);
            _dragObject = null;
        }
        var pos = TileMapController.Instance.GetCellScreen(eventData.position);
        if (!CardController.Instance.Place(pos, Card, false))
        {
            //Can't place so we refund 
            foreach (var price in Card.Prices)
            {
                GameController.Stats.Add(price.Stats, price.Value);
            }
        }
        TileSelectionController.Instance.ClearHighlight();
        OnDragEnd?.Invoke(this);
    }


    public void Hide()
    {
        Container.gameObject.SetActive(false);
        PriceContainer.gameObject.SetActive(false);
    }
    public void Show()
    {
        Container.gameObject.SetActive(true);
        PriceContainer.gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(Vector3.one * 1.25f, 0.2f);
        TileInfoController.Instance.m_hoverTileInfoText.text
         = "<b>" + Card.Name + "</b>\n" + Card.Description;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBounce);
        TileInfoController.Instance.m_hoverTileInfoText.text = "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TileInfoController.Instance.m_hoverTileInfoText.text
         = "<b>" + Card.Name + "</b>\n" + Card.Description;

    }
}
