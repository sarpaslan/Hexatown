using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Events;
using KeuGames.Utils;

public class FoundItemBehaviour : MonoBehaviour
{
    public SpriteRenderer Content;
    public SpriteRenderer Background;
    public FoundItemView FoundItemViewPrefab;

    [NonSerialized]
    public ExplorationFoundable Foundable;

    public UnityEvent OnClicked = new();
    private int id;
    public bool Clicked;
    private bool collected;
    private void Start()
    {
        id = GameScreen.Instance.ShowLabel(transform, Foundable.Name, 0.5f);
    }

    private void OnDestroy()
    {
        GameScreen.Instance.HideLabel(id);
    }

    public void OnMouseDown()
    {
        if (CameraMovement.IsPointerOverUIObject()) return;
        OnClicked?.Invoke();
        if (!Foundable.Collectable) return;
        if (collected) return;
        collected = true;
        GameController.Resources.SoundPlayer.Play("pop");
        GameScreen.Instance.HideLabel(id);
        transform.DOKill();
        Background.transform.gameObject.SetActive(false);
        float jumpPower = 0.4f;
        int numJumps = 1;
        float duration = 0.3f;
        Content.transform.DOJump(LittleRandom.XY(Content.transform.position, 0.7f), jumpPower, numJumps, duration).
        SetUpdate(true).OnComplete(OnCompleted);
    }

    public void OnMouseEnter()
    {
        if (collected) return;
        transform.DOScale(1.375f, 0.2f);
    }
    public void OnMouseExit()
    {
        if (collected) return;
        transform.DOScale(1.25f, 0.2f);
    }

    public void SetContent(ExplorationFoundable targ)
    {
        Content.sprite = targ.Sprite;
        Foundable = targ;
    }

    private async void OnCompleted()
    {
        await UniTask.WaitForSeconds(0.2f, true);
        var view = Instantiate(FoundItemViewPrefab, GameScreen.Instance.InfoBoxContainer);
        view.Description.text = this.Foundable.FoundText;
        var gm = view.Found;
        gm.sprite = Content.sprite;
        gm.transform.SetParent(GameScreen.Instance.InfoBoxContainer, false);
        gm.transform.position = GameController.Camera.WorldToScreenPoint(Content.transform.position);
        gm.transform.SetParent(view.transform);
        Destroy(gameObject);
    }
}
