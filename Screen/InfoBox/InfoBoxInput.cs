using System;
using TMPro;
using UnityEngine;

public class InfoBoxInput : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_title;
    public string Title
    {
        get
        {
            return m_title.text;
        }
        set
        {
            m_title.text = value;
            m_title.gameObject.SetActive(true);
        }
    }

    public TMP_InputField InputField;
}
