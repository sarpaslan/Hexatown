
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class LittleRandom
{
    public static Vector3 XY(Vector3 value, float val)
    {
        var randomX = Random.Range(-val, val);
        var randomY = Random.Range(-val, val);
        return new Vector3(value.x + randomX, value.y + randomY, value.z);
    }
    public static int GetRandomInt()
    {
        return Random.Range(0, 1000000000);
    }
    public static T SelectRandom<T>(this List<T> list)
    {
        if (list.Count == 0)
            return default;

        return list[Random.Range(0, list.Count)];
    }

    public static T SelectRandom<T>(this T[] arr)
    {
        if (arr.Length == 0)
            return default;
        return arr[Random.Range(0, arr.Length)];
    }
}