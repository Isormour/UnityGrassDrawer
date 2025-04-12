using UnityEditor;
using UnityEngine;

internal class EditorRaycaster
{
    public float BrushScale = 1;
    Material brushMat;
    Mesh brushMesh;
    public RaycastHit lastHit { private set; get; }
    public EditorRaycaster(Material mat, Mesh mesh)
    {
        brushMat = mat;
        brushMesh = mesh;
    }
    public void Update()
    {
        ChangeBrushSize();
        Raycast();
    }

    private void Raycast()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            DrawPaintMarker(hit);
            lastHit = hit;
        }

    }

    private void ChangeBrushSize()
    {
        // input
        Event e = Event.current;
        bool isScrollWithShift = e.type == EventType.ScrollWheel && e.shift;
        if (isScrollWithShift)
        {
            BrushScale += e.delta.x;
            BrushScale = Mathf.Clamp(BrushScale, 0.5f, 100);
        }
    }

    private void DrawPaintMarker(RaycastHit hit)
    {
        Color originalColor = Handles.color;
        Color tempColor = Color.green;
        tempColor.a = 0.5f;
        Handles.color = tempColor;
        Matrix4x4 matrix = Matrix4x4.TRS(hit.point, Quaternion.identity, new Vector3(1, 1, 1) * BrushScale);

        SceneView.RepaintAll();
        Renderer renderer = hit.collider.GetComponent<Renderer>();
        int lightmapIndex = renderer.lightmapIndex;
        Color lightmapColor = Color.white;
        if (lightmapIndex != -1)
        {
            //lightmapColor = SampleLightmap(hit);
        }
        lightmapColor.a = 0.3f;
        brushMat.SetColor("_Color", lightmapColor);
        brushMat.SetPass(0);
        Graphics.DrawMeshNow(brushMesh, matrix, 0);

        SceneView.RepaintAll();
        Handles.color = originalColor;
    }
}