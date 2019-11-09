using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

public static class Helper
{
    public static int Normalize(int number)
    {
        if (number == 0)
        {
            return 0;
        }
        return 1;
    }
    public static float Round(float n, float dp)
    {
        float stage1 = n * Mathf.Pow(10, dp);
        float stage2 = Mathf.Round(stage1);
        return stage2 / Mathf.Pow(10, dp);
    }

    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * Clamp01(t);
    }

    public static float Clamp01(float value)
    {
        if ((double)value < 0.0)
            return 0.0f;
        if ((double)value > 1.0)
            return 1f;

        return value;
    }

    public static float Clamp(float value, float min, float max)
    {
        if ((double)value < min)
            return min;
        if ((double)value > max)
            return max;

        return value;
    }

    public static string Serialize<T>(this T toSerialize)
    {
        XmlSerializer xml = new XmlSerializer(typeof(T));
        Debug.Log("IEI");
        StringWriter writer = new StringWriter();
        xml.Serialize(writer, toSerialize);
        return writer.ToString();
    }

    public static T Deserialize<T>(this string toDeserialize)
    {
        XmlSerializer xml = new XmlSerializer(typeof(T));
        StringReader reader = new StringReader(toDeserialize);
        return (T)xml.Deserialize(reader);
    }
}
