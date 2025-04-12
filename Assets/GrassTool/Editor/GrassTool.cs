using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GrassTool
{
    public Dictionary<GameObject, GrassObjectData> selectedObjects { private set; get; }
    public int Density;

    Material brushMat;
    Mesh brushMesh;

    public delegate void OnPointsChanged();
    OnPointsChanged onPointsChanged;
    float brushScale = 1;
    public bool DrawGizmos = true;
    Vector2 chunkSize = new Vector2(100, 100);
    public GrassToolViewPortCollider viewportCollider { private set; get; }
    public GrassTool(Material brushMat, Mesh brushMesh, OnPointsChanged onPointsChanged)
    {
        this.brushMat = brushMat;
        this.brushMesh = brushMesh;
        this.onPointsChanged = onPointsChanged;
        this.Density = 1;
        selectedObjects = new Dictionary<GameObject, GrassObjectData>();
    }

    private bool GetDataObject(GameObject obj)
    {
        string filePath = GetFilePath(obj);
        GrassObjectData tempData = AssetDatabase.LoadAssetAtPath<GrassObjectData>(filePath);
        if (!selectedObjects.ContainsKey(obj))
        {
            AddSelectedObject(obj, tempData);
        }
        return tempData != null;
    }

    private void AddSelectedObject(GameObject obj, GrassObjectData tempData)
    {
        selectedObjects.Add(obj, tempData);
        Renderer rend = obj.GetComponent<Renderer>();
        Texture2D texture = (Texture2D)rend.sharedMaterial.mainTexture;
        if (rend.lightmapIndex > -1 && rend.lightmapIndex < LightmapSettings.lightmaps.Length)
        {
            Texture2D lightmap = LightmapSettings.lightmaps[rend.lightmapIndex].lightmapColor;
            SetTextureReadable(lightmap, true);
        }
        SetTextureReadable(texture, true);
    }

    private void LoadData(GameObject selectedObj)
    {
        if (selectedObj == null) return;
        GrassObjectData tempData = null;
        if (GetDataObject(selectedObj))
        {
            tempData = ApplyData(selectedObj);
        }
        else
        {
            tempData = GrassObjectData.CreateGrassObjectData(selectedObj.GetComponent<Renderer>(), chunkSize);
        }

        if (!selectedObjects.ContainsKey(selectedObj))
            selectedObjects.Add(selectedObj, null);
        selectedObjects[selectedObj] = tempData;
    }

    public void OnSceneGUI(SceneView view)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            DrawPaintMarker(hit);
        }

        // input
        Event e = Event.current;
        bool isScrollWithShift = e.type == EventType.ScrollWheel && e.shift;
        if (isScrollWithShift)
        {
            brushScale += e.delta.x;
            brushScale = Mathf.Clamp(brushScale, 0.5f, 100);
        }

        bool isLeftClicked = e.button == 0 && e.type == EventType.MouseDown;
        bool isLeftHoldWithShift = e.button == 0 && e.type == EventType.MouseDrag && e.shift;
        if (isLeftClicked || isLeftHoldWithShift)
        {
            AddGrassPoints(hit);
        }
        bool isRightclicked = e.button == 1 && e.type == EventType.MouseDown;
        bool isRightHoldWithShift = e.button == 1 && e.type == EventType.MouseDrag && e.shift;
        if (isRightclicked || isRightHoldWithShift)
        {
            RemoveGrassPoints(hit);
        }
    }

    private void RemoveGrassPoints(RaycastHit hit)
    {
        if (selectedObjects.Keys.Count < 1) return;
        Event currentEvent = Event.current;
        currentEvent.Use();
        if (hit.collider && hit.collider.GetComponent<Renderer>())
        {
            GrassObjectData currentData = selectedObjects[hit.collider.gameObject];
            currentData.RemoveGrassBlades(hit.point, brushScale);
        }
    }
    private void AddGrassPoints(RaycastHit hit)
    {
        Event currentEvent = Event.current;
        currentEvent.Use();
        if (hit.collider && hit.collider.GetComponent<Renderer>())
        {

            Dictionary<GameObject, List<GrassObjectChunk.GrassBladeData>> toAdd = FindPointsAtPosition(10 * Density * brushScale, hit.point);
            GameObject[] toAddKeys = toAdd.Keys.ToArray();

            for (int i = 0; i < toAddKeys.Length; i++)
            {
                List<GrassObjectChunk.GrassBladeData> pointsToAdd = toAdd[toAddKeys[i]];
                GrassObjectData currentData = selectedObjects[toAddKeys[i]];

                List<GrassObjectChunk.GrassBladeData> tempGrassBlades = new List<GrassObjectChunk.GrassBladeData>();
                for (int j = 0; j < pointsToAdd.Count; j++)
                {
                    GrassObjectChunk.GrassBladeData temp = new GrassObjectChunk.GrassBladeData();
                    temp.Position = pointsToAdd[j].Position;
                    temp.Light = pointsToAdd[j].Light;
                    temp.GroundColor = pointsToAdd[j].GroundColor;
                    tempGrassBlades.Add(temp);
                }
                currentData.AddGrassBlades(tempGrassBlades);

            }
        }
    }

    private void DrawPaintMarker(RaycastHit hit)
    {
        Color originalColor = Handles.color;
        Color tempColor = Color.green;
        tempColor.a = 0.5f;
        Handles.color = tempColor;
        Matrix4x4 matrix = Matrix4x4.TRS(hit.point, Quaternion.identity, new Vector3(1, 1, 1) * brushScale);

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
    private Dictionary<GameObject, List<GrassObjectChunk.GrassBladeData>> FindPointsAtPosition(float density, Vector3 Position)
    {
        Dictionary<GameObject, List<GrassObjectChunk.GrassBladeData>> pointsOnGameobject = new Dictionary<GameObject, List<GrassObjectChunk.GrassBladeData>>();
        int amount = (int)(density);
        for (int i = 0; i < amount; i++)
        {
            float randomRadious = Random.Range(0.0f, 1.0f) * brushScale;
            float randomAngle = Random.Range(0.0f, 360.0f);
            Vector3 randomPoint = GetPointOnCircle(Position, randomRadious, randomAngle);
            randomPoint += new Vector3(0, 3, 0);
            RaycastHit hitInfo;
            if (Physics.Raycast(randomPoint, -Vector3.up, out hitInfo))
            {
                Renderer rend = hitInfo.collider.GetComponent<Renderer>();
                if (!rend)
                {
                    continue;
                }
                Vector3 pos = hitInfo.point;
                GrassObjectChunk.GrassBladeData data = new GrassObjectChunk.GrassBladeData();
                data.Position = pos;
                data.Light = SampleLightmap(hitInfo).grayscale;
                data.GroundColor = SampleTexture(hitInfo, (Texture2D)rend.sharedMaterial.mainTexture);
                GameObject grassParent = hitInfo.collider.gameObject;

                if (!pointsOnGameobject.ContainsKey(grassParent))
                {
                    pointsOnGameobject.Add(grassParent, new List<GrassObjectChunk.GrassBladeData>());
                }
                pointsOnGameobject[grassParent].Add(data);
            }
            if (DrawGizmos)
            {
                Debug.DrawRay(randomPoint, -Vector3.up * 3, Color.cyan, 0.1f);
            }
        }
        return pointsOnGameobject;
    }
    private Vector3 GetPointOnCircle(Vector3 center, float radious, float angle)
    {
        float X = radious * Mathf.Cos(angle);
        float Y = radious * Mathf.Sin(angle);
        center.x += X;
        center.z += Y;
        return center;
    }

    GrassObjectData ApplyData(GameObject obj)
    {
        string filePath = GetFilePath(obj);
        GrassObjectData currentObjectData = AssetDatabase.LoadAssetAtPath<GrassObjectData>(filePath);
        return currentObjectData;
    }

    public void SaveData(GameObject selectedObj)
    {
        string filePath = GetFilePath(selectedObj);
        if (AssetDatabase.LoadAssetAtPath<GrassObjectData>(filePath) == null)
        {
            AssetDatabase.CreateAsset(selectedObjects[selectedObj], filePath);
        }
        else
        {

            EditorUtility.SetDirty(selectedObjects[selectedObj]);
            SerializedObject so = new SerializedObject(selectedObjects[selectedObj]);
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
    Color SampleTexture(RaycastHit hit, Texture2D currentTexture)
    {
        Vector2 uv = hit.textureCoord;
        int x = Mathf.FloorToInt(uv.x * currentTexture.width);
        int y = Mathf.FloorToInt(uv.y * currentTexture.height);
        if (!currentTexture.isReadable) SetTextureReadable(currentTexture, true);
        Color color = currentTexture.GetPixel(x, y);
        color = color.linear;
        return color;
    }

    private void SetTextureReadable(Texture2D texture, bool v)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer != null)
        {
            // Ustawienie w³aœciwoœci Read/Write Enabled
            importer.isReadable = v;

            // Zapisanie zmian
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"Set isReadable = true for {texture.name}");
        }
        else
        {
            Debug.LogError("Could not find TextureImporter for the selected texture.");
        }
    }

    Color SampleLightmap(RaycastHit hit)
    {
        Color col = Color.white;
        Vector2 lightmapUV = hit.lightmapCoord;
        if (lightmapUV.x < 0 || lightmapUV.x > 1 || lightmapUV.y < 0 || lightmapUV.y > 1)
        {
            Debug.LogWarning("UV coordinates are out of range: " + lightmapUV);
            return Color.white;
        }

        int lightmapIndex = hit.collider.gameObject.GetComponent<Renderer>().lightmapIndex;
        if (lightmapIndex > -1)
        {
            LightmapData lightmapData = LightmapSettings.lightmaps[lightmapIndex];
            Texture2D lightmapTexture = lightmapData.lightmapColor;
            if (!lightmapTexture.isReadable)
            {
                Debug.LogError("texture should be readable, impossible state");
                SetTextureReadable(lightmapTexture, true);
                return col;
            }
            if (lightmapTexture != null)
            {
                int x = Mathf.FloorToInt(lightmapUV.x * lightmapTexture.width);
                int y = Mathf.FloorToInt(lightmapUV.y * lightmapTexture.height);

                if (x >= 0 && x < lightmapTexture.width && y >= 0 && y < lightmapTexture.height)
                {
                    col = lightmapTexture.GetPixel(x, y);
                }
            }
        }
        return col;
    }
    string GetFilePath(GameObject selectedObj)
    {
        string scenePath = SceneManager.GetActiveScene().path;
        scenePath = scenePath.Replace(".unity", "");
        string sceneName = SceneManager.GetActiveScene().name;
        sceneName = sceneName.Replace(".unity", "");
        CheckDirectory(scenePath, sceneName);
        string filePath = scenePath + "/" + sceneName + "_Grass" + "_" + selectedObj.GetInstanceID() + ".asset";
        return filePath;
    }

    private static void CheckDirectory(string scenePath, string sceneName)
    {
        if (!System.IO.Directory.Exists(scenePath + "/" + sceneName))
            System.IO.Directory.CreateDirectory(scenePath + "/" + sceneName);
    }

    public void StartDraw()
    {
        viewportCollider = new GrassToolViewPortCollider();
        viewportCollider.OnGameObjectEnter = LoadData;
    }
    public void EndDraw()
    {
        List<GameObject> currentObjects = selectedObjects.Keys.ToList();
        foreach (var item in currentObjects)
        {
            SaveData(item);
        }
        foreach (var item in currentObjects)
        {
            Renderer rend = item.GetComponent<Renderer>();
            Texture2D texture = (Texture2D)rend.sharedMaterial.mainTexture;
            SetTextureReadable(texture, false);
            if (rend.lightmapIndex > -1)
            {
                Texture2D lightmap = LightmapSettings.lightmaps[rend.lightmapIndex].lightmapColor;
                SetTextureReadable(lightmap, false);
            }
        }
        viewportCollider.Dispose();
        viewportCollider.OnGameObjectEnter = null;
    }
}
