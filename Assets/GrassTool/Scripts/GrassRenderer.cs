using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class GrassRenderer : MonoBehaviour
{
    public GrassType grassType;
    public GrassObject[] grassObjects;

    public ObjectGrassRenderer[][,] renderers { private set; get; }

#if UNITY_EDITOR
    public bool drawGizmos = false;
#endif
    private void OnEnable()
    {
        if (grassObjects == null)
        {
            return;
        }


#if UNITY_EDITOR
        for (int i = 0; i < grassObjects.Length; i++)
        {
            if (grassObjects[i].data != null)
                grassObjects[i].data.OnRefreshRenderer = OnEnable;
        }
#endif
        renderers = new ObjectGrassRenderer[grassObjects.Length][,];
        if (grassObjects.Length > 0 && grassType.mat && grassType.mesh)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                var chunks = grassObjects[i].data.chunks;
                renderers[i] = new ObjectGrassRenderer[chunks.Size.x, chunks.Size.y];
                for (int j = 0; j < chunks.Size.x; j++)
                {
                    for (int k = 0; k < chunks.Size.y; k++)
                    {
                        renderers[i][j, k] = new ObjectGrassRenderer(chunks.Get(j, k), grassType.mat, grassType.mesh);
                    }
                }
            }
        }
    }
    public void AddGrassObject(GrassObject grassObject)
    {
        GrassObject[] old = grassObjects;
        grassObjects = new GrassObject[old.Length + 1];
        for (int i = 0; i < old.Length; i++)
        {
            grassObjects[i] = old[i];
        }
        grassObjects[grassObjects.Length - 1] = grassObject;
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
                    item[i, j].DrawGizmo();
                }
            }
        }
#endif
    }
#if UNITY_EDITOR
    private static void DrawGizmo(Vector2 min, Vector2 max, ObjectGrassRenderer render)
    {
        Vector3 center = render.bounds.center;
        center += new Vector3(0, 0, 0);
        Color col = Color.black;
        col.a = 0.5f;
        Vector2 point = render.bounds.center;
        point.y = render.bounds.center.z;
        Vector2 normPos = NormalizePosition(min, max, point);
        col.r = normPos.x;
        col.g = normPos.y;

        var bounds = render.bounds;
        Camera cam = SceneView.currentDrawingSceneView.camera;
        UnityEngine.Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        bool inBounds = GeometryUtility.TestPlanesAABB(planes, bounds);

        if (inBounds)
        {
            Gizmos.color = col;
            Gizmos.DrawCube(center, render.bounds.size + new Vector3(-0.5f, 0, -0.5f));
        }
    }
#endif
    static public Vector2 NormalizePosition(Vector2 min, Vector2 max, Vector2 point)
    {
        Vector2 norm = Vector2.zero;
        norm.x = max.x - min.x;
        norm.x = point.x / norm.x;
        norm.y = max.y - min.y;
        norm.y = point.y / norm.y;
        return norm;
    }
    [System.Serializable]
    public class GrassObject
    {
        public GameObject gameObject;
        public GrassObjectData data;
        public GrassObject(GameObject gameObject, GrassObjectData data)
        {
            this.gameObject = gameObject;
            this.data = data;
        }
    }
}
