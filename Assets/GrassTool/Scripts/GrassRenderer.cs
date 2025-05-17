using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class GrassRenderer : MonoBehaviour
{
    public GrassType grassType;
    public GrassObject[] grassObjects;

    public ObjectGrassRendererGrid[] objectGrid { private set; get; }

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
        CreateGridsForObjects();
    }

    private void CreateGridsForObjects()
    {
        objectGrid = new ObjectGrassRendererGrid[grassObjects.Length];
        if (grassObjects.Length > 0 && grassType.mat && grassType.mesh)
        {
            for (int i = 0; i < objectGrid.Length; i++)
            {
                var chunks = grassObjects[i].data.chunks;
                objectGrid[i] = new ObjectGrassRendererGrid(chunks, grassType.mat, grassType.mesh);
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
        if (objectGrid == null || objectGrid.Length < 1)
            return;

        foreach (var item in objectGrid)
        {
            if (item != null)
                item.Update();
        }

    }
    private void OnDisable()
    {
        if (objectGrid != null && objectGrid.Length > 0)
        {
            foreach (var item in objectGrid)
            {
                if (item != null)
                    item.Deinitialize();
            }
        }
        objectGrid = null;
    }
#if UNITY_EDITOR
    public void OnDrawGizmos()
    {

        if (!drawGizmos) return;

        if (objectGrid == null || objectGrid.Length < 1)
        {
            return;
        }
        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;

        foreach (var item in objectGrid)
        {
            if (item == null)

                return;

            int xLen = item.objectGrassRenderers.GetLength(0);
            min = item.objectGrassRenderers[0, 0].ChunkData.ObjectBounds.center;
            min.y = item.objectGrassRenderers[0, 0].ChunkData.ObjectBounds.center.z;

            for (int i = 0; i < xLen; i++)
            {
                int yLen = item.objectGrassRenderers.GetLength(1);
                max = item.objectGrassRenderers[xLen - 1, yLen - 1].ChunkData.ObjectBounds.center;
                max.y = item.objectGrassRenderers[xLen - 1, yLen - 1].ChunkData.ObjectBounds.center.z;

                for (int j = 0; j < yLen; j++)
                {
                    item.objectGrassRenderers[i, j].DrawGizmo();
                }
            }
        }
    }
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
