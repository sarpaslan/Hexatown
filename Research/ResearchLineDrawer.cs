using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CanvasRenderer))]
public class ResearchLineDrawer : Graphic
{
    public float lineThickness = 5f;
    public ResearchView ResearchView;
    private List<ResearchNodeView> m_nodes = new List<ResearchNodeView>();

    private CanvasScaler canvasScaler;
    public Camera Camera => FindObjectOfType<Camera>();
    public float Scale;
    public Color UnlockableColor;

    protected override void Start()
    {
        base.Start();
        canvasScaler = GetComponentInParent<CanvasScaler>();
    }

    public new void OnValidate()
    {
        if (ResearchView != null)
        {
            m_nodes = ResearchView.Nodes;
        }
    }

    void Update()
    {
        SetAllDirty();
    }

    private Vector2 WorldToLocal(Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        return new Vector2(localPoint.x, localPoint.y);
    }
    public void DrawConnections(VertexHelper vh, ResearchNodeView connection)
    {
        if (connection.Connection == null)
            return;
        var start = WorldToLocal(connection.transform.position);
        start.Scale(Vector3.one * Scale);
        var end = WorldToLocal(connection.Connection.transform.position);
        end.Scale(Vector3.one * Scale);
        var colour = color;
        if (connection.Connection.Unlocked)
        {
            colour = UnlockableColor;
        }
        DrawLine(vh, start, end, lineThickness, colour);
        DrawConnections(vh, connection.Connection);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (m_nodes == null || m_nodes.Count < 2)
        {
            m_nodes = FindObjectsByType<ResearchNodeView>(FindObjectsSortMode.None).ToList();
            return;
        }

        foreach (var n in m_nodes)
            DrawConnections(vh, n);
    }

    private void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, float thickness, Color colour)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * (thickness / 2f);

        Vector2 v1 = start - perpendicular;
        Vector2 v2 = start + perpendicular;
        Vector2 v3 = end + perpendicular;
        Vector2 v4 = end - perpendicular;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = colour;

        vertex.position = v1;
        vh.AddVert(vertex);
        vertex.position = v2;
        vh.AddVert(vertex);
        vertex.position = v3;
        vh.AddVert(vertex);
        vertex.position = v4;
        vh.AddVert(vertex);

        int index = vh.currentVertCount - 4;
        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index + 2, index + 3, index);
    }

    public override bool Raycast(Vector2 sp, Camera eventCamera)
    {
        return false;
    }
}

