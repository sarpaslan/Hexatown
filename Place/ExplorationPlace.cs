
using System;
using DG.Tweening;
using UnityEngine;

public enum ExplorationType
{
    Cave,
    Tent,
    Pyramid,
    StiltHouse
}

public class ExplorationPlace : Place
{
    ExplorationType type;
    FoundItemBehaviour m_foundItemBehaviour;
    public ExplorationPlace(Card card, Vector3Int pos) : base(card, pos)
    {
    }

    public override void OnTick()
    {
        if (m_foundItemBehaviour == null)
        {
            m_foundItemBehaviour = ExplorationController.Instance.CreateFoundablePlace(this);
            if (Name == "Cave")
            {
                type = ExplorationType.Cave;
                OnInitCave();
            }
            else if (Name == "Tent")
            {
                type = ExplorationType.Tent;
                OnInitTent();
            }
            else if (Name == "Pyramid")
            {
                type = ExplorationType.Pyramid;
                OnInitPyramid();
            }
            else if (Name == "Stilt House")
            {
                type = ExplorationType.StiltHouse;
                OnInitStiltHouse();
            }
        }
        switch (type)
        {
            case ExplorationType.Cave:
                UpdateCave();
                break;
            case ExplorationType.Tent:
                UpdateTent();
                break;
            case ExplorationType.Pyramid:
                UpdatePyramid();
                break;
            case ExplorationType.StiltHouse:
                UpdateStiltHouse();
                break;
        }
    }
    public override void OnSelect()
    {
        m_foundItemBehaviour.Background.gameObject.SetActive(false);
        switch (type)
        {
            case ExplorationType.Cave:
                break;
            case ExplorationType.Tent:
                break;
            case ExplorationType.Pyramid:
                break;
            case ExplorationType.StiltHouse:
                m_foundItemBehaviour.transform.DOScale(Vector3.zero, 0.3f);
                GameObject.Destroy(m_foundItemBehaviour.gameObject, 0.3f);
                DestroyPlace();
                break;
        }
    }

    private void UpdateStiltHouse()
    {
    }

    private void UpdatePyramid()
    {
    }

    private void UpdateTent()
    {
    }

    private void UpdateCave()
    {
    }

    private void OnInitStiltHouse()
    {
    }

    private void OnInitPyramid()
    {
    }

    private void OnInitTent()
    {
    }

    public void OnInitCave()
    {
        GoblinController.Instance.AddCave(this);
    }

    public override Sprite GetSprite()
    {
        switch (type)
        {
            case ExplorationType.Cave:
                return SpriteIcon.Cave.ToSprite();
            case ExplorationType.Tent:
                return SpriteIcon.Tent.ToSprite();
            case ExplorationType.Pyramid:
                return SpriteIcon.Pyramid.ToSprite();
            case ExplorationType.StiltHouse:
                return SpriteIcon.StiltHouse.ToSprite();
        }
        return base.GetSprite();
    }

    public override int Tax => 0;
}