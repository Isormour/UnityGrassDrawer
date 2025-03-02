using System;
using UnityEngine;

public class GrassObjectData : ScriptableObject
{
    public GrassBladeData[] GrassBlades;
    public Bounds ObjectBounds;
    public Action OnRefreshRenderer;
#if UNITY_EDITOR
    public static GrassObjectData CreateGrassObjectData(Renderer rend)
    {
        GrassObjectData obj = CreateInstance<GrassObjectData>();
        obj.GrassBlades = new GrassBladeData[0];
        Bounds bounds = rend.bounds;
        bounds.extents = ClampVector(bounds.extents, 1, 1000);
        obj.ObjectBounds = rend.bounds;
        return obj;
    }

    public static Vector3 ClampVector(Vector3 v, float min, float max)
    {
        Vector3 temp = new Vector3(0, 0, 0);
        temp.x = Mathf.Clamp(v.x, min, max);
        temp.y = Mathf.Clamp(v.y, min, max);
        temp.z = Mathf.Clamp(v.z, min, max);
        return temp;
    }
#endif
    public void RefreshRenderer()
    {
#if UNITY_EDITOR
        if (OnRefreshRenderer == null)
        {
            GrassRenderer grassRenderer = GameObject.FindObjectOfType<GrassRenderer>();
            grassRenderer?.AddGrassObject(this);
        }
#endif
        OnRefreshRenderer?.Invoke();
    }
    [System.Serializable]
    public struct GrassBladeData
    {
        public Vector3 Position;
        public float Light;
        public Color GroundColor;
    }
}
