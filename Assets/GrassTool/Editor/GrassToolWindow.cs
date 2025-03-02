using System.Linq;
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
    static GameObject selectedObj;
    static Texture2D lightmap;

    bool defaultColorSet = false;
    Color defaultUIColor;

    GrassTool tool;
    [MenuItem("Tools/GrassDrawer")]

    public static void CreateWindow()
    {
        GrassToolWindow window = GetWindow<GrassToolWindow>("GrassDrawer");
        window.InitWindow();
    }
    void InitWindow()
    {
        selectedObj = null;
        Selection.selectionChanged += OnSelectionChanged;

        LoadToolAssets();
        if (!brushMat || !brushMesh)
            return;

        tool = new GrassTool(selectedObj, brushMat, brushMesh, OnGrassPointsChanged);
        OnSelectionChanged();
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
        Selection.selectionChanged -= OnSelectionChanged;
        SceneView.duringSceneGui -= tool.OnSceneGUI;
        if (startPaint)
        {
            EndDraw();
        }
    }


    void OnSelectionChanged()
    {
        if (selectedObj != Selection.activeGameObject && startPaint)
        {
            EndDraw();
        }
        selectedObj = Selection.activeGameObject;
        if (selectedObj != null && selectedObj.GetComponent<Renderer>())
            tool.ChangeSelectedObj(selectedObj);
        else if (selectedObj == null && startPaint)
            EndDraw();
        this.Repaint();
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


        if (!brushMesh || !brushMat || !GrassRendererExsist())
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
        if (brushMat != null && brushMesh != null)
            this.tool = new GrassTool(selectedObj, brushMat, brushMesh, OnGrassPointsChanged);


    }

    void OnGrassPointsChanged()
    {
        this.Repaint();
    }
    private void DrawPaintPanel()
    {
        selectedObj = EditorGUILayout.ObjectField(selectedObj, typeof(GameObject), true) as GameObject;
        if (GetDrawProblems()) return;
        DrawData();
        DrawControls();
    }

    private static bool GetDrawProblems()
    {
        if (selectedObj == null)
        {
            EditorGUILayout.LabelField("Select object to draw on and provide lightning settings");
            return true;
        }
        if (!lightmap)
        {
            EditorGUILayout.LabelField("provide lightmap if needed");
        }
        MeshRenderer Rend = selectedObj.GetComponent<MeshRenderer>();
        if (!Rend)
        {
            EditorGUILayout.LabelField("Select object with mesh renderer");
            return true;
        }
        if (lightmap != null && !lightmap.isReadable)
        {
            EditorGUILayout.LabelField("Ensure lightmaptexture is READABLE");
        }
        return false;
    }

    private void DrawData()
    {
        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Name = " + selectedObj.name);
        EditorGUILayout.LabelField("Grass Points = " + tool.currentObjectData.GrassBlades.Count());
        EditorGUILayout.LabelField("Object Position = " + tool.currentObjectData.ObjectBounds.center);
        EditorGUILayout.LabelField("ID = " + selectedObj.GetInstanceID());
        EditorGUILayout.LabelField("");
    }

    void DrawControls()
    {
        if (GUILayout.Button("Start Paint"))
        {
            StartDraw();
        }
        if (GUILayout.Button("End Paint"))
        {
            EndDraw();
        }
    }
    void StartDraw()
    {
        if (startPaint) return;
        startPaint = true;
        SceneVisibilityManager.instance.DisableAllPicking();
        tempHotControl = GUIUtility.hotControl;
        GUIUtility.hotControl = 0;

        MeshCollider meshColl = selectedObj.AddComponent<MeshCollider>();
        meshColl.sharedMesh = selectedObj.GetComponent<MeshFilter>().sharedMesh;
        SceneView.duringSceneGui += tool.OnSceneGUI;
        Tools.hidden = true;
    }
    void EndDraw()
    {
        if (!startPaint) return;
        startPaint = false;
        SceneVisibilityManager.instance.EnableAllPicking();
        SceneView.duringSceneGui -= tool.OnSceneGUI;
        GUIUtility.hotControl = tempHotControl;
        tool.EndDraw();
        Tools.hidden = false;
    }

}
