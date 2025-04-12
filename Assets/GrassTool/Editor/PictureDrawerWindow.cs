using UnityEditor;
using UnityEngine;

public class PictureDrawerWindow : EditorWindow
{

    const string BRUSH_MESH_PATH_ID = "GRASS_TOOL_WINDOW_BRUSH_MESH";
    const string BRUSH_MATERIAL_PATH_ID = "GRASS_TOOL_WINDOW_BRUSH_MATERIAL";
    const string VISUALISE_MATERIAL_PATH_ID = "VISUALISE_WINDOW_BRUSH_MATERIAL";
    const string PAINTER_COMPUTE_PATH_ID = "PAINTER_COMPUTE";
    Material mat;
    Material visMat;
    Mesh mesh;
    PictureDrawerController controller;
    ComputeShader compute;
    bool materialSet = false;


    [MenuItem("Tools/PictureDrawer")]
    public static void CreateWindow()
    {
        PictureDrawerWindow window = GetWindow<PictureDrawerWindow>("pricture Drawer");

        window.InitWindow();
    }
    void InitWindow()
    {
        LoadToolAssets();
        controller = new PictureDrawerController(mat, visMat, mesh, compute);
    }

    private void LoadToolAssets()
    {
        string meshPath = PlayerPrefs.GetString(BRUSH_MESH_PATH_ID);
        mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        string materialPath = PlayerPrefs.GetString(BRUSH_MATERIAL_PATH_ID);
        mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        string visualiseMaterialPath = PlayerPrefs.GetString(VISUALISE_MATERIAL_PATH_ID);
        visMat = AssetDatabase.LoadAssetAtPath<Material>(visualiseMaterialPath);
        string computeShaderPath = PlayerPrefs.GetString(PAINTER_COMPUTE_PATH_ID);
        compute = AssetDatabase.LoadAssetAtPath<ComputeShader>(computeShaderPath);
    }
    private void OnGUI()
    {
        if (!mesh || !mat || !visMat || !compute) DrawBrushProps();
        else DrawControls();
        SetMaterial();
    }

    private void DrawControls()
    {
        EditorGUILayout.LabelField("-- CONTROLS --");
        if (GUILayout.Button("Draw"))
        {
            controller.ToggleEnabled();
            if (controller.update)
            {
                SceneView.duringSceneGui += OnSceneGUI;
            }
            else
            {
                SceneView.duringSceneGui -= OnSceneGUI;
            }
        }
        if (controller.update)
        {
            GUI.DrawTexture(new Rect(0, 60, 256, 256), controller.rt, ScaleMode.ScaleToFit, false);
        }
    }

    private void OnSceneGUI(SceneView view)
    {
        this.Repaint();
    }

    private void DrawBrushProps()
    {
        mesh = EditorGUILayout.ObjectField("Brush mesh", mesh, typeof(Mesh), true) as Mesh;
        if (mesh != null)
        {
            string path = AssetDatabase.GetAssetPath(mesh);
            PlayerPrefs.SetString(BRUSH_MESH_PATH_ID, AssetDatabase.GetAssetPath(mesh));
            PlayerPrefs.Save();
        }
        mat = EditorGUILayout.ObjectField("Brush material", mat, typeof(Material), true) as Material;
        if (mat != null)
        {
            string path = AssetDatabase.GetAssetPath(mat);
            PlayerPrefs.SetString(BRUSH_MATERIAL_PATH_ID, AssetDatabase.GetAssetPath(mat));
            PlayerPrefs.Save();
        }
        visMat = EditorGUILayout.ObjectField("visualise material", visMat, typeof(Material), true) as Material;
        if (visMat != null)
        {
            string path = AssetDatabase.GetAssetPath(visMat);
            PlayerPrefs.SetString(VISUALISE_MATERIAL_PATH_ID, AssetDatabase.GetAssetPath(visMat));
            PlayerPrefs.Save();
        }
        compute = EditorGUILayout.ObjectField("visualise material", compute, typeof(ComputeShader), true) as ComputeShader;
        if (compute != null)
        {
            string path = AssetDatabase.GetAssetPath(compute);
            PlayerPrefs.SetString(PAINTER_COMPUTE_PATH_ID, AssetDatabase.GetAssetPath(compute));
            PlayerPrefs.Save();
        }
    }

    private void SetMaterial()
    {
        if (mat != null && !materialSet && visMat != null && mesh != null)
        {
            materialSet = true;
            mat.mainTexture = controller.rt;
        }
    }

    private void OnDestroy()
    {
        controller?.Dispose();
    }
}
