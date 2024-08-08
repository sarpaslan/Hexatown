using UnityEngine;

public abstract class InfoView : MonoBehaviour
{
    protected Place Place;
    public virtual void SetPlace(Place place)
    {
        Place = place;
    }
    public abstract void Refresh();
}
