using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;

//Todo what am i doing
//Idk about this lol
public static class Register
{
    public static int int1 = 0;
    public static int int2 = 0;
    public static int int3 = 0;
    public static int int4 = 0;
    public static int int5 = 0;
    public static int int6 = 0;
    public static int int7 = 0;
    public static int int8 = 0;
    public static int int9 = 0;
    public static int int10 = 0;


    public static float float1 = 0;
    public static float float2 = 0;
    public static float float3 = 0;


    public static bool bool1 = false;
    public static bool bool2 = false;

    public static Vector3 vector1 = Vector3.zero;
    public static Vector3 vector2 = Vector3.zero;
}

public static class Helper
{

    private static readonly Dictionary<Type, Func<string, object>> TypeParsers = new Dictionary<Type, Func<string, object>>()
    {
        { typeof(int), str => int.TryParse(str, out int i) ? i : null },
        { typeof(float), str => float.TryParse(str, out float f) ? f : null },
        { typeof(bool), str => bool.TryParse(str, out bool b) ? b : null },
        { typeof(short), str => short.TryParse(str, out short s) ? s : null },
        { typeof(long), str => long.TryParse(str, out long l) ? l : null },
        { typeof(double), str => double.TryParse(str, out double d) ? d : null },
        { typeof(char), str => char.TryParse(str, out char c) ? c : null },
        { typeof(string), str => str }
    };
    public static void SetFieldValue(object target, FieldInfo field, string value)
    {
        if (TypeParsers.TryGetValue(field.FieldType, out Func<string, object> parser))
        {
            object parsedValue = parser(value);
            if (parsedValue != null)
            {
                field.SetValue(target, parsedValue);
            }
        }
    }
    public static async void DestroyUnscaled(GameObject obj, float seconds)
    {
        await UniTask.WaitForSeconds(seconds, true);
        UnityEngine.Object.Destroy(obj);
    }
    public static Sprite ToSprite(this SpriteIcon spriteIcon)
    {
        return GameController.Resources.GetIcon(spriteIcon);
    }

    public static Sprite ToSprite(this Stats spriteIcon)
    {
        return GameController.Resources.ToIconType(spriteIcon).ToSprite();
    }


    public static Sprite ToSprite(this WorkTier tier)
    {
        return ToSprite(GameController.Resources.GetVillagerTierIcon(tier));
    }

    public static string ToReadableString(this WorkTier tier)
    {
        switch (tier)
        {
            case WorkTier.FARMER:
                return "Farmer";
            case WorkTier.WORKER:
                return "Worker";
            case WorkTier.ARTISAN:
                return "Artisan";
        }
        return null;
    }
}