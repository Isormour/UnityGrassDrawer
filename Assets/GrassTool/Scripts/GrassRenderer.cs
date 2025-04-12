using UnityEngine;

[ExecuteInEditMode]
public class GrassRenderer : MonoBehaviour
{
    [SerializeField] GrassObjectData[] grassObjectData;
    [SerializeField] Material grassMat;
    [SerializeField] Mesh grassMesh;
    ObjectGrassRenderer[][,] renderers;

#if UNITY_EDITOR
    public bool drawGizmos = false;
#endif
    private void OnEnable()
    {
        if (grassObjectData == null)
        {
            return;
        }


#if UNITY_EDITOR
        for (int i = 0; i < grassObjectData.Length; i++)
        {
            if (grassObjectData[i] != null)
                grassObjectData[i].OnRefreshRenderer = OnEnable;
        }
#endif
        renderers = new ObjectGrassRenderer[grassObjectData.Length][,];
        if (grassObjectData.Length > 0 && grassMat && grassMesh)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i] = new ObjectGrassRenderer[grassObjectData[i].chunks.Size.x, grassObjectData[i].chunks.Size.y];
                for (int j = 0; j < grassObjectData[i].chunks.Size.x; j++)
                {
                    for (int k = 0; k < grassObjectData[i].chunks.Size.y; k++)
                    {
                        renderers[i][j, k] = new ObjectGrassRenderer(grassObjectData[i].chunks.Get(j, k), grassMat, grassMesh);
                    }
                }
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
        if (renderers == null || renderers.Length < 1)
            return;

        foreach (var item in renderers)
        {
            if (item != null)
                foreach (var chunk in item)
                {
                    chunk?.Update();
                }
        }

    }
    private void OnDisable()
    {
        if (renderers != null && renderers.Length > 0)
        {
            foreach (var item in renderers)
            {
                if (item != null)
                    foreach (var chunk in item)
                    {
                        chunk?.Deinitialize();
                    }
            }
        }
        renderers = null;
    }
    public void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!drawGizmos) return;
#endif

        if (renderers == null || renderers.Length < 1)
        {
            return;
        }
        foreach (var item in renderers)
        {
            if (item == null)

                return;

            int xLen = item.GetLength(0);

            for (int i = 0; i < xLen; i++)
            {
                int yLen = item.GetLength(1);
                for (int j = 0; j < yLen; j++)
                {
                    item[i, j].DrawGizmos();
                }
            }
        }

    }
}
