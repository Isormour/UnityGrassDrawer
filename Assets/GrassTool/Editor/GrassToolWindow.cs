using UnityEditor;
using UnityEngine;

public class GrassToolWindow : EditorWindow
{
    const string BRUSH_MESH_PATH_ID = "GRASS_TOOL_WINDOW_BRUSH_MESH";
    const string BRUSH_MATERIAL_PATH_ID = "GRASS_TOOL_WINDOW_BRUSH_MATERIAL";


    bool startPaint = false;
    int tempHotControl = 0;
    static Mesh brushMesh;
    static Material brushMat;
    string fieldName;
    GrassTool tool;
    GrassType grassType;
    [MenuItem("Tools/GrassDrawer")]

    public static void CreateWindow()
    {
        GrassToolWindow window = GetWindow<GrassToolWindow>("GrassDrawer");
        window.InitWindow();
    }
    void InitWindow()
    {
        LoadToolAssets();
        if (!brushMat || !brushMesh || !grassType)
            return;
        tool = new GrassTool(brushMat, brushMesh, grassType);
    }

    private static void LoadToolAssets()
    {
        string meshPath = PlayerPrefs.GetString(BRUSH_MESH_PATH_ID);
        brushMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        string materialPath = PlayerPrefs.GetString(BRUSH_MATERIAL_PATH_ID);
        brushMat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= tool.OnSceneGUI;
        if (startPaint)
        {
            EndDraw();
        }
    }

    private void OnGUI()
    {
        if (startPaint)
        {
            Color backgroundColor = Color.green;
            backgroundColor.a = 0.5f;
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), backgroundColor);
        }
        DrawUI();
    }
    void DrawUI()
    {
        if (!brushMesh || !brushMat || !GrassRendererExsist() || !grassType)
        {
            DrawInitPanel();
        }
        else
        {
            DrawPaintPanel();
        }
    }
    bool GrassRendererExsist()
    {
        return GameObject.FindObjectOfType<GrassRenderer>();
    }
    private void DrawInitPanel()
    {
        EditorGUILayout.LabelField("Initialization");
        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Set brush values");

        brushMesh = EditorGUILayout.ObjectField("Brush mesh", brushMesh, typeof(Mesh), true) as Mesh;
        if (brushMesh != null)
        {
            string path = AssetDatabase.GetAssetPath(brushMesh);
            PlayerPrefs.SetString(BRUSH_MESH_PATH_ID, AssetDatabase.GetAssetPath(brushMesh));
            PlayerPrefs.Save();
        }
        brushMat = EditorGUILayout.ObjectField("Brush material", brushMat, typeof(Material), true) as Material;
        if (brushMat != null)
        {
            string path = AssetDatabase.GetAssetPath(brushMat);
            PlayerPrefs.SetString(BRUSH_MATERIAL_PATH_ID, AssetDatabase.GetAssetPath(brushMat));
            PlayerPrefs.Save();
        }
        if (!GrassRendererExsist())
        {
            GUIStyle redLabelStyle = new GUIStyle(EditorStyles.label);
            redLabelStyle.normal.textColor = Color.red;
            EditorGUILayout.LabelField("No grass renderer on scene", redLabelStyle);
        }
        if (!grassType)
        {
            GUIStyle redLabelStyle = new GUIStyle(EditorStyles.label);
            redLabelStyle.normal.textColor = Color.red;
            EditorGUILayout.LabelField("Set grass type", redLabelStyle);
        }
        grassType = EditorGUILayout.ObjectField("grass type", grassType, typeof(GrassType), false) as GrassType;

        if (brushMat && brushMesh && grassType)
            this.tool = new GrassTool(brushMat, brushMesh, grassType);
    }
    private void DrawPaintPanel()
    {
        DrawControls();
        DrawData();
        DrawSettings();
        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Controls:");
        DrawLegend();
    }

    private static void DrawLegend()
    {
        EditorGUILayout.LabelField("LMB add grass");
        EditorGUILayout.LabelField("RMB remove grass");
        EditorGUILayout.LabelField("Hold shift to continuous paint");
        EditorGUILayout.LabelField("Hold shift + scroll to change brush size");
    }

    private void DrawSettings()
    {
        tool.DrawGizmos = EditorGUILayout.Toggle("Draw Gizmo ", tool.DrawGizmos);
        tool.Density = EditorGUILayout.IntField("Density ", tool.Density);
    }

    private void DrawData()
    {
        EditorGUILayout.LabelField("");
        if (startPaint)
        {
            EditorGUILayout.LabelField("Objects in viewport count = " + tool.viewportCollider.objectsInView.Count);
        }
        EditorGUILayout.LabelField("");
    }

    void DrawControls()
    {
        if (!startPaint && GUILayout.Button("Start Paint"))
        {
            StartDraw();
        }
        if (startPaint && GUILayout.Button("End Paint"))
        {
            EndDraw();
        }
    }
    void StartDraw()
    {
        if (startPaint) return;
        startPaint = true;
        SceneView.duringSceneGui += RepaintWindow;
        SceneVisibilityManager.instance.DisableAllPicking();
        tempHotControl = GUIUtility.hotControl;
        GUIUtility.hotControl = 0;

        tool.StartDraw();
        SceneView.duringSceneGui += tool.OnSceneGUI;
        Tools.hidden = true;
    }

    private void RepaintWindow(SceneView view)
    {
        Repaint();
    }

    void EndDraw()
    {
        if (!startPaint) return;
        SceneView.duringSceneGui -= RepaintWindow;
        startPaint = false;
        SceneVisibilityManager.instance.EnableAllPicking();
        SceneView.duringSceneGui -= tool.OnSceneGUI;
        GUIUtility.hotControl = tempHotControl;
        tool.EndDraw();
        Tools.hidden = false;
        grassType = null;
    }

}
