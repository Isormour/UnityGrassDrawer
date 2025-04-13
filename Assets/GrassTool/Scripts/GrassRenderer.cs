using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class GrassRenderer : MonoBehaviour
{
    [SerializeField] GrassObjectData[] grassObjectData;
    [SerializeField] Material grassMat;
    [SerializeField] Mesh grassMesh;
    public ObjectGrassRenderer[][,] renderers { private set; get; }

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
                    if (chunk.enabled)
                        chunk.Update();
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
        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;

        foreach (var item in renderers)
        {
            if (item == null)

                return;

            int xLen = item.GetLength(0);
            min = item[0, 0].ChunkData.ObjectBounds.center;
            min.y = item[0, 0].ChunkData.ObjectBounds.center.z;

            for (int i = 0; i < xLen; i++)
            {
                int yLen = item.GetLength(1);
                max = item[xLen - 1, yLen - 1].ChunkData.ObjectBounds.center;
                max.y = item[xLen - 1, yLen - 1].ChunkData.ObjectBounds.center.z;

                for (int j = 0; j < yLen; j++)
                {
                    DrawGizmo(min, max, item[i, j]);
                }
            }
        }

    }

    private static void DrawGizmo(Vector2 min, Vector2 max, ObjectGrassRenderer render)
    {
        Vector3 center = render.ChunkData.ObjectBounds.center;

        Color col = Color.black;
        col.a = 0.5f;
        Vector2 point = render.ChunkData.ObjectBounds.center;
        point.y = render.ChunkData.ObjectBounds.center.z;
        Vector2 normPos = NormalizePosition(min, max, point);
        col.r = normPos.x;
        col.g = normPos.y;

        var bounds = render.ChunkData.ObjectBounds;
        Camera cam = SceneView.currentDrawingSceneView.camera;
        UnityEngine.Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        bool inBounds = GeometryUtility.TestPlanesAABB(planes, bounds);

        if (inBounds)
        {
            Gizmos.color = col;
            Gizmos.DrawCube(center, render.ChunkData.ObjectBounds.size + new Vector3(-0.5f, 1, -0.5f));
        }
    }
    static public Vector2 NormalizePosition(Vector2 min, Vector2 max, Vector2 point)
    {
        Vector2 norm = Vector2.zero;
        norm.x = max.x - min.x;
        norm.x = point.x / norm.x;
        norm.y = max.y - min.y;
        norm.y = point.y / norm.y;
        return norm;
    }
}
