using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TableView : MonoBehaviour
{
    public Transform ItemViewContainer;
    public TableHeaderView TableHeaderPrefab;
    public TableItemView ItemViewPrefab;
    public TMP_Text TextPrefab;
    public GameObject CategoryPrefab;

    private TableHeaderView m_currentHeader;
    private Dictionary<int, TMP_Text[]> m_currentItems = new Dictionary<int, TMP_Text[]>();

    public void Reset()
    {
        Destroy(m_currentHeader.gameObject);
        m_currentHeader = null;
        m_currentItems.Clear();
        foreach (Transform ctrl in ItemViewContainer)
        {
            Destroy(ctrl.gameObject);
        }
    }

    public void CreateHeader(params string[] rows)
    {
        if (m_currentHeader == null)
        {
            m_currentHeader = Instantiate(TableHeaderPrefab, ItemViewContainer);
        }
        foreach (var v in rows)
        {
            var r = Instantiate(TextPrefab, m_currentHeader.transform);
            r.text = v;
        }
    }
    public void Hide(int id)
    {
        if (m_currentItems.TryGetValue(id, out TMP_Text[] arr))
            arr[0].transform.parent.gameObject.SetActive(false);
    }
    public void Show(int id)
    {
        if (m_currentItems.TryGetValue(id, out TMP_Text[] arr))
            arr[0].transform.parent.gameObject.SetActive(true);
    }
    public void Refresh(int id, Action onClicked, Action onCreated = null, params string[] rows)
    {
        if (m_currentItems.TryGetValue(id, out TMP_Text[] arr))
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i].text = rows[i];
            }
            return;
        }
        else
        {
            onCreated?.Invoke();
            CreateItem(id, onClicked, rows);
        }
    }
    public void CreateItem(int id, Action onClicked, params string[] rows)
    {
        var arr = new TMP_Text[rows.Length];
        m_currentItems.Add(id, arr);
        var container = Instantiate(ItemViewPrefab, ItemViewContainer.transform);
        container.Button.onClick.AddListener(() => { onClicked?.Invoke(); });
        for (int i = 0; i < rows.Length; i++)
        {
            string v = rows[i];
            var tmpText = Instantiate(TextPrefab, container.transform);
            tmpText.text = v;
            arr[i] = tmpText;
        }
    }

    public void CreateCategory(string title)
    {
        var category = Instantiate(CategoryPrefab, ItemViewContainer.transform);
        category.transform.GetComponentInChildren<TMP_Text>().text = title;
    }
}
