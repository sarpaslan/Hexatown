using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoBox : MonoBehaviour
{
    public TMP_Text Title;
    public TMP_Text Description;
    public InfoBoxButton ButtonPrefab;
    public InfoBoxInput InputPrefab;
    public Transform Container;
    public void Close()
    {
        Destroy(gameObject);
    }
    public InfoBoxInput CreateInput()
    {
        return Instantiate(InputPrefab, Container);
    }
    public InfoBoxButton CreateButton()
    {
        return Instantiate(ButtonPrefab, Container);
    }
}
