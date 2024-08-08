using UnityEngine;

public class PlaceMenu : MonoBehaviour
{
    protected Place Place;
    public virtual void SetPlace(Place place)
    {
        this.Place = place;
    }
}
