using System.IO;
using UnityEditor;
using UnityEngine;

public class PictureDrawerController
{
    public RenderTexture rt { private set; get; }
    EditorRaycaster raycaster;
    public bool update = false;
    private int tempHotControl;
    private ComputeShader compute;
    int size = 1024;
    int kernelID;
    Texture2D materialTexture;
    Material visualizeMaterial;
    public PictureDrawerController(Material brushMat, Material visualizeMaterial, Mesh mesh, ComputeShader compute)
    {
        rt = new RenderTexture(size, size, 16);
        rt.enableRandomWrite = true;
        SceneView.duringSceneGui += OnSceneGUI;
        tempHotControl = GUIUtility.hotControl;
        raycaster = new EditorRaycaster(brushMat, mesh);
        materialTexture = (Texture2D)visualizeMaterial.mainTexture;
        this.visualizeMaterial = visualizeMaterial;

        this.compute = compute;
        kernelID = compute.FindKernel("CSMain");
    }
    public void Dispose()
    {
        update = true;
        ToggleEnabled();
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView view)
    {
        if (!update) return;
        raycaster.Update();
        if (!raycaster.lastHit.collider) return;

        if (CheckLMB())
        {
            DrawOnTexture(raycaster.lastHit.collider.gameObject, raycaster.lastHit, 1);
        }
        if (CheckRMB())
        {
            DrawOnTexture(raycaster.lastHit.collider.gameObject, raycaster.lastHit, -0.5f);
        }
        Event e = Event.current;

        if (CheckLMB() || CheckRMB()) e.Use();
    }

    private bool CheckLMB()
    {
        Event e = Event.current;
        bool isLeftClicked = false;
        bool isLeftHoldWithShift = false;
        isLeftClicked = e.button == 0 && e.type == EventType.MouseDown;
        isLeftHoldWithShift = e.button == 0 && e.type == EventType.MouseDrag && e.shift;
        return isLeftClicked || isLeftHoldWithShift;
    }
    private bool CheckRMB()
    {
        Event e = Event.current;
        bool isLeftClicked = false;
        bool isLeftHoldWithShift = false;
        isLeftClicked = e.button == 1 && e.type == EventType.MouseDown;
        isLeftHoldWithShift = e.button == 1 && e.type == EventType.MouseDrag && e.shift;
        return isLeftClicked || isLeftHoldWithShift;
    }

    private void DrawOnTexture(GameObject gameObject, RaycastHit hit, float multiply)
    {
        Vector2 coords = CalculatePoint(gameObject, hit);
        // Ustawiamy parametry w compute shaderze.
        compute.SetTexture(kernelID, "_Source", rt);
        compute.SetTexture(kernelID, "_Result", rt);
        compute.SetFloat("_Mult", multiply);
        compute.SetInts("_TextureSize", size, size);

        // Koordynaty centrum i promień.
        Renderer rend = gameObject.GetComponent<Renderer>();
        float U = (rend.bounds.max.x - rend.bounds.min.x);

        float brushSize = raycaster.BrushScale / U;

        compute.SetFloat("_Radius", brushSize);
        compute.SetFloats("_Coord", coords.x, coords.y);

        // Liczymy, ile grup wątków musimy uruchomić. 
        // Zgodnie z [numthreads(8, 8, 1)], robimy odpowiednie zaokrąglenie.
        int groupsX = Mathf.CeilToInt(size / 8.0f);
        int groupsY = Mathf.CeilToInt(size / 8.0f);

        // Uruchamiamy kernel.
        compute.Dispatch(kernelID, groupsX, groupsY, 1);
    }

    private Vector2 CalculatePoint(GameObject gameObject, RaycastHit hit)
    {
        Renderer rend = gameObject.GetComponent<Renderer>();
        float U = (hit.point.x - rend.bounds.min.x) / (rend.bounds.max.x - rend.bounds.min.x);
        float V = (hit.point.z - rend.bounds.min.z) / (rend.bounds.max.z - rend.bounds.min.z);
        return new Vector2(U, V);
    }

    internal void ToggleEnabled()
    {
        update = !update;
        Tools.hidden = update;
        GUIUtility.hotControl = 0;
        if (update)
        {
            SceneVisibilityManager.instance.DisableAllPicking();
            this.visualizeMaterial.mainTexture = rt;
        }
        else
        {
            this.visualizeMaterial.mainTexture = this.materialTexture;
            SceneVisibilityManager.instance.EnableAllPicking();
            string path = Application.dataPath + "/Test.png";
            Debug.Log("TextureSaved at " + path);
            SaveRenderTextureToPNG(rt, path);
        }
    }
    public static void SaveRenderTextureToPNG(RenderTexture renderTexture, string path)
    {
        if (renderTexture == null)
        {
            Debug.LogError("RenderTexture jest null – nie można zapisać!");
            return;
        }

        // Zapisujemy oryginalny aktywny kontekst renderowania:
        RenderTexture currentRT = RenderTexture.active;

        // Ustawiamy nasz RenderTexture jako aktywny:
        RenderTexture.active = renderTexture;

        // Tworzymy nową teksturę, w której przechowamy dane z RT:
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        // Czytamy piksele z aktywnego RT do tekstury 2D:
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        // Przywracamy poprzedni aktywny RT:
        RenderTexture.active = currentRT;

        // Konwertujemy teksturę do formatu PNG:
        byte[] bytes = tex.EncodeToPNG();

        // Usuwamy obiekt Texture2D, jeśli nie jest już potrzebny, by zwolnić pamięć:
        Object.DestroyImmediate(tex);

        // Zapis do pliku:
        File.WriteAllBytes(path, bytes);

        Debug.Log($"RenderTexture zapisany jako PNG: {path}");
    }
}