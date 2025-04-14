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
        //
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
        previousBlades.AddRange(grassBladeDatas);
        GrassBlades = previousBlades.ToArray();

        if (!ObjectBounds.Contains(grassBladeDatas[0].Position))
        {
            Debug.LogError("To nie powinno mie� miejsca, nani dafuk");
        }
    }

    [Serializable]
    public struct GrassBladeData
    {
        public Vector3 Position;
        public float Light;
        public Color GroundColor;
    }
}
