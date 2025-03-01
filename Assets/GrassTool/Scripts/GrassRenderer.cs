using UnityEngine;

[ExecuteInEditMode]
public class GrassRenderer : MonoBehaviour
{
    [SerializeField] GrassObjectData[] grassObjectData;
    public GrassObjectData[] CurrentGrassObjects => grassObjectData;
    [SerializeField] Material grassMat;
    [SerializeField] Mesh grassMesh;
    ObjectGrassRenderer[] renderers;
    private void OnEnable()
    {
        if (grassObjectData == null)
        {
            return;
        }

#if UNITY_EDITOR

        for (int i = 0; i < grassObjectData.Length; i++)
        {
            grassObjectData[i].OnRefreshRenderer = OnEnable;
        }

#endif

        if (grassObjectData.Length > 0 && grassMat && grassMesh)
        {
            renderers = new ObjectGrassRenderer[grassObjectData.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i] = new ObjectGrassRenderer(grassObjectData[i], grassMat, grassMesh);
            }

        }
    }
    public void AddGrassObject(GrassObjectData grassObject)
    {
        GrassObjectData[] old = grassObjectData;
        grassObjectData = new GrassObjectData[grassObjectData.Length + 1];
        for (int i = 0; i < old.Length; i++)
        {
            grassObjectData[i] = old[i];
        }
        grassObjectData[grassObjectData.Length - 1] = grassObject;
        OnEnable();
    }

    private void Update()
    {
        if (renderers != null && renderers.Length > 0)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].Update();
            }
        }
    }
    private void OnDisable()
    {
        if (renderers != null && renderers.Length > 0)
        {
            foreach (ObjectGrassRenderer rend in renderers)
            {
                rend.Deinitialize();
            }
        }
    }
}
