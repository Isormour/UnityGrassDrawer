using System;
using UnityEngine;

public class GrassObjectData : ScriptableObject
{
    public GrassBladeData[] GrassBlades;
    public Bounds ObjectBounds;
    public Action OnRefreshRenderer;
    public static GrassObjectData CreateGrassObjectData(Renderer rend)
    {
        GrassObjectData obj = CreateInstance<GrassObjectData>();
        obj.GrassBlades = new GrassBladeData[0];
        obj.ObjectBounds = rend.bounds;
        return obj;
    }
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
