
using UnityEditor;
using System.Linq;
using UnityEngine;
using System;
public enum ShowType
{
    Resources,
    Biomes,
}

public class GameResourcesWindow : EditorWindow
{
    private Vector2 scroll;
    private GameResources m_resources;
    private static ShowType m_showType;
    [MenuItem("Medieval/Game Resources")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow(typeof(GameResourcesWindow)) as GameResourcesWindow;
        window.m_resources = Resources.LoadAll<GameResources>("Resources/")[0];
    }
    public void OnGUI()
    {
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Resources")) m_showType = ShowType.Resources;
        if (GUILayout.Button("Biomes")) m_showType = ShowType.Biomes;
        GUILayout.EndHorizontal();
        if (m_showType == ShowType.Resources)
        {
            ShowResources();
        }
        else if (m_showType == ShowType.Biomes)
        {
            ShowBiomes();
        }
        GUILayout.EndScrollView();
    }

    private void ShowBiomes()
    {
        foreach (var res in m_resources.m_biomes)
        {
            GUILayout.BeginHorizontal();
            res.Icon = (Sprite)EditorGUILayout.ObjectField(res.Type.ToString(), res.Icon, typeof(Sprite), false);
            res.Type = (BiomeType)EditorGUILayout.EnumPopup(res.Type);
            GUILayout.EndHorizontal();
        }
    }

    public void ShowResources()
    {
        var names = Enum.GetNames(typeof(SpriteIcon)).ToList();
        var errorText = "";
        foreach (var n in names)
        {
            var found = false;
            foreach (var x in m_resources.m_sprites)
            {
                if (n == x.Type.ToString())
                {
                    if (found)
                    {
                        errorText += n + " is added twice\n";
                    }
                    found = true;
                }
            }

            if (!found)
            {
                errorText += n + " is not exist\n";
            }
        }
        GUI.color = Color.red;
        GUILayout.Label(errorText);
        GUI.color = Color.white;

        GUILayout.Space(20);
        names = names.Where(t => m_resources.m_sprites.Any(x => t == x.Type.ToString())).ToList();
        var errors = m_resources.m_sprites.Where(t => !names.Contains(t.Type.ToString()));
        if (GUILayout.Button("Add"))
            AddNew();
        GUILayout.BeginVertical();
        foreach (var v in m_resources.m_sprites)
        {
            GUILayout.BeginHorizontal();
            v.Icon = (Sprite)EditorGUILayout.ObjectField(v.Type.ToString(), v.Icon, typeof(Sprite), false);
            v.Type = (SpriteIcon)EditorGUILayout.EnumPopup(v.Type);
            GUILayout.Label(((int)v.Type).ToString());
            if (GUILayout.Button("X"))
            {
                var list = m_resources.m_sprites.ToList();
                list.Remove(v);
                m_resources.m_sprites = list.ToArray();
                GUILayout.EndHorizontal();
                break;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Add"))
            AddNew();
        GUI.color = Color.red;
        GUILayout.Label(errorText);
        GUI.color = Color.white;
        GUILayout.EndVertical();
    }
    public void AddNew()
    {
        var list = m_resources.m_sprites.ToList();
        list.Add(new SpriteResource());
        m_resources.m_sprites = list.ToArray();
        UnityEditor.EditorUtility.SetDirty(m_resources);
        scroll = new Vector2(0, 50000);
    }

}
