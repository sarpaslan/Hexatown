using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileSelectionController : MonoBehaviour
{

    [SerializeField]
    private bool m_canSelect = true;
    private SpriteRenderer m_highlight;
    private List<SpriteRenderer> m_createdOutlines = new List<SpriteRenderer>();
    public SpriteRenderer OutlineTile;
    public SpriteRenderer HighlightTile;
    public Vector3Int SelectedTile;

    public static TileSelectionController Instance;

    public void Awake()
    {
        Instance = this;
    }

    public void SelectTile(Vector3Int position)
    {
        if (position != SelectedTile)
        {
            UnselectTile();
        }
        SelectedTile = position;
        TileInfoController.Instance.Show(position);

        var gm = Instantiate(OutlineTile);
        var center = TileMapController.Instance.Map.CellToWorld(position);
        gm.transform.position = new Vector3(center.x, center.y - GameConfig.MAGIC_Y, center.z);
        m_createdOutlines.Add(gm);
    }

    public SpriteRenderer Highlight(Vector3Int pos, Color color)
    {
        var highlight = Instantiate(HighlightTile);
        highlight.name = "Highlight";
        highlight.color = color;


        Destroy(highlight, 10);


        var center = TileMapController.Instance.Map.CellToWorld(pos);
        highlight.transform.position = new Vector3(center.x, center.y - GameConfig.MAGIC_Y, center.z);
        return highlight;

    }
    public void Outline(Vector3Int pos, Color color)
    {
        if (m_highlight != null)
        {
            Destroy(m_highlight.gameObject);
        }
        m_highlight = Instantiate(OutlineTile);
        m_highlight.name = "Outline";
        m_highlight.color = color;
        var center = TileMapController.Instance.Map.CellToWorld(pos);
        m_highlight.transform.position = new Vector3(center.x, center.y - GameConfig.MAGIC_Y, center.z);
    }

    public void ClearHighlight()
    {
        if (m_highlight != null)
        {
            Destroy(m_highlight.gameObject);
            m_highlight = null;
        }
    }

    public void UnselectTile()
    {
        ClearNeighbours();
        TileInfoController.Instance.Hide();
    }
    private void ClearNeighbours()
    {
        for (int i = 0; i < m_createdOutlines.Count; i++)
        {
            Destroy(m_createdOutlines[i].gameObject);
        }
        m_createdOutlines.Clear();
    }

    private void ShowNeighbours(Vector3Int cell)
    {
        ClearNeighbours();
        foreach (var position in TileMapController.Instance.GetNeighbours(cell))
        {
            var gm = Instantiate(OutlineTile);
            var center = TileMapController.Instance.Map.CellToWorld(position);
            gm.transform.position = new Vector3(center.x, center.y - GameConfig.MAGIC_Y, center.z);
            m_createdOutlines.Add(gm);
        }
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_canSelect = !CameraMovement.IsPointerOverUIObject();
            if (!m_canSelect)
            {
                return;
            }
            var cell = TileMapController.Instance.GetCellScreen(Input.mousePosition);
            if (!TileMapController.Instance.Placed.HasTile(cell))
            {
                UnselectTile();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!m_canSelect || CameraMovement.IsPointerOverUIObject())
            {
                return;
            }
            if (!CameraMovement.MovedLastFrame)
            {
                var cell = TileMapController.Instance.GetCellScreen(Input.mousePosition);
                if (TileMapController.Instance.Placed.HasTile(cell))
                {
                    SelectTile(cell);
                }
            }
        }
    }
}
