using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GrassObjectChunk
{
    public GrassBladeData[] GrassBlades;
    public Bounds ObjectBounds;
    public GrassObjectChunk(Bounds bounds)
    {
        GrassBlades = new GrassBladeData[0];
        ObjectBounds = bounds;
    }

    public static Vector3 ClampVector(Vector3 v, float min, float max)
    {
        Vector3 temp = new Vector3(0, 0, 0);
        temp.x = Mathf.Clamp(v.x, min, max);
        temp.y = Mathf.Clamp(v.y, min, max);
        temp.z = Mathf.Clamp(v.z, min, max);
        return temp;
    }

    internal void AddGrassBlades(List<GrassBladeData> grassBladeDatas)
    {
        List<GrassBladeData> previousBlades = GrassBlades.ToList();
        for (int i = 0; i < grassBladeDatas.Count; i++)
        {
            previousBlades.Add(grassBladeDatas[i]);
        }
        GrassBlades = previousBlades.ToArray();
    }

    [Serializable]
    public struct GrassBladeData
    {
        public Vector3 Position;
        public float Light
        {
            get { return light / 255.0f; }
            set { light = (byte)(value * 255); }
        }


        public Color GroundColor
        {
            get { return new Color(r / 255.0f, g / 255.0f, b / 255.0f); }
            set
            {
                r = (byte)(value.r * 255);
                g = (byte)(value.g * 255);
                b = (byte)(value.b * 255);
            }
        }
        public byte light;
        public byte r;
        public byte g;
        public byte b;


    }
}
