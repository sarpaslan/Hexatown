using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class CategoryAttribute : System.Attribute
{
    public string Title;
    public CategoryAttribute(string title)
    {
        Title = title;
    }
}


[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
sealed class UsefullInfoAttribute : System.Attribute
{
    public bool IsCheatToChange;
    public bool ReadOnly;
    public bool EditorOnly;
    public string Description;
    public UsefullInfoAttribute(string description = null, bool editorOnly = false, bool readOnly = false, bool isCheatToChange = false)
    {
        this.Description = description;
        this.ReadOnly = readOnly;
        this.EditorOnly = editorOnly;
        this.IsCheatToChange = isCheatToChange;
    }
}

public class LogNameValuePair
{
    public string Name { get; set; }
    public string Value { get; set; }
}
public static class UsefullInfoUtils
{

    public static List<LogNameValuePair> GetUsefullInfoCollection(System.Object o)
    {
        var coll = new List<LogNameValuePair>();
        var type = o.GetType();
        var props = type.GetProperties().Where(t => t.HasAttribute<UsefullInfoAttribute>());
        foreach (var prop in props)
        {
            coll.Add(new LogNameValuePair() { Name = prop.Name, Value = prop.GetValue(o)?.ToString() });
        }
        var fields = type.GetFields().Where(t => t.HasAttribute<UsefullInfoAttribute>());
        foreach (var field in fields)
        {
            coll.Add(new LogNameValuePair() { Name = field.Name, Value = field.GetValue(o)?.ToString() });
        }
        return coll;
    }
}