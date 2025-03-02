using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GrassTool
{
    GameObject selectedObj;
    public GrassObjectData currentObjectData { private set; get; }
    Material brushMat;
    Mesh brushMesh;
    Texture2D currentTexture;
    bool SetReadeableToFalse = false;
    public delegate void OnPointsChanged();
    OnPointsChanged onPointsChanged;
    int lightmapIndex = -1;
    float brushScale = 1;

    public GrassTool(GameObject selectedObj, Material brushMat, Mesh brushMesh, OnPointsChanged onPointsChanged)
    {
        this.selectedObj = selectedObj;
        this.brushMat = brushMat;
        this.brushMesh = brushMesh;
        this.onPointsChanged = onPointsChanged;
        LoadData();
        if (selectedObj != null)
        {
            MeshCollider meshColl = selectedObj.AddComponent<MeshCollider>();
            meshColl.sharedMesh = selectedObj.GetComponent<MeshFilter>().sharedMesh;
        }
    }
    private bool GetDataObject()
    {
        string filePath = GetFilePath();
        currentObjectData = AssetDatabase.LoadAssetAtPath<GrassObjectData>(filePath);
        return currentObjectData != null;
    }

    public void ChangeSelectedObj(GameObject newObj)
    {
        if (selectedObj != null)
        {
            EndDraw();
            if (SetReadeableToFalse)
            {
                SetTextureReadable(currentTexture, false);
            }
        }
        if (lightmapIndex > -1)
        {
            LightmapData lightmapData = LightmapSettings.lightmaps[lightmapIndex];
            Texture2D lightmapTexture = lightmapData.lightmapColor;
            if (!LightmapSettings.lightmaps[lightmapIndex].lightmapColor.isReadable)
            {
                SetTextureReadable(lightmapTexture, false);
            }
        }
        selectedObj = newObj;
        LoadData();
        CheckRendererGrassObjectData();

        Renderer renderer = newObj.GetComponent<Renderer>();
        currentTexture = renderer.sharedMaterial.mainTexture as Texture2D;


        if (!currentTexture.isReadable)
        {
            SetReadeableToFalse = true;
            SetTextureReadable(currentTexture, true);
        }

        lightmapIndex = renderer.lightmapIndex;
        if (lightmapIndex > -1)
        {
            LightmapData lightmapData = LightmapSettings.lightmaps[lightmapIndex];
            Texture2D lightmapTexture = lightmapData.lightmapColor;
            if (!LightmapSettings.lightmaps[lightmapIndex].lightmapColor.isReadable)
            {
                SetTextureReadable(lightmapTexture, true);
            }
        }


    }

    private void CheckRendererGrassObjectData()
    {
        GrassRenderer rend = GameObject.FindObjectOfType<GrassRenderer>();
        bool rendererContainsData = false;
        for (int i = 0; i < rend.CurrentGrassObjects.Length; i++)
        {
            if (rend.CurrentGrassObjects[i] == currentObjectData)
            {
                rendererContainsData = true;
                break;
            }
        }
        if (!rendererContainsData)
        {
            rend.AddGrassObject(currentObjectData);
        }
    }

    private void LoadData()
    {
        if (selectedObj == null) return;
        if (GetDataObject())
        {
            currentObjectData = ApplyData();
        }
        else
        {
            Renderer rend = selectedObj.GetComponent<Renderer>();
            if (rend)
                currentObjectData = GrassObjectData.CreateGrassObjectData(rend);
        }
    }

    public void OnSceneGUI(SceneView view)
    {

        DrawCurrentPositions();

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
    private void DrawCurrentPositions()
    {
        if (currentObjectData.GrassBlades.Length > 15000) return;

        foreach (var grassBlade in currentObjectData.GrassBlades)
        {
            Debug.DrawRay(grassBlade.Position, Vector3.up * 0.2f, Color.green);
        }
    }
    private void RemoveGrassPoints(RaycastHit hit)
    {
        Event currentEvent = Event.current;
        currentEvent.Use();
        if (hit.collider)
        {

            List<GrassObjectData.GrassBladeData> tempGrassBlades = currentObjectData.GrassBlades.ToList();
            for (int i = 0; i < tempGrassBlades.Count; i++)
            {
                if (Vector3.Distance(tempGrassBlades[i].Position, hit.point) < 1 * brushScale)
                {
                    tempGrassBlades.RemoveAt(i);
                    i--;
                }
            }
            currentObjectData.GrassBlades = tempGrassBlades.ToArray();
        }
        onPointsChanged?.Invoke();
        currentObjectData.RefreshRenderer();
    }
    private void AddGrassPoints(RaycastHit hit)
    {
        Event currentEvent = Event.current;
        currentEvent.Use();
        if (hit.collider)
        {
            List<GrassObjectData.GrassBladeData> toAdd = FindPointsOnObject(selectedObj, 10 * brushScale, hit.point);
            List<GrassObjectData.GrassBladeData> tempGrassBlades = currentObjectData.GrassBlades.ToList();

            for (int i = 0; i < toAdd.Count; i++)
            {
                GrassObjectData.GrassBladeData temp = new GrassObjectData.GrassBladeData();
                temp.Position = toAdd[i].Position;
                temp.Light = toAdd[i].Light;
                temp.GroundColor = toAdd[i].GroundColor;
                tempGrassBlades.Add(temp);

            }
            currentObjectData.GrassBlades = tempGrassBlades.ToArray();
        }
        onPointsChanged?.Invoke();
        currentObjectData.RefreshRenderer();
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
            lightmapColor = SampleLightmap(hit);
        }
        lightmapColor.a = 0.3f;
        brushMat.SetColor("_Color", lightmapColor);
        brushMat.SetPass(0);
        Graphics.DrawMeshNow(brushMesh, matrix, 0);

        SceneView.RepaintAll();
        Handles.color = originalColor;
    }
    private List<GrassObjectData.GrassBladeData> FindPointsOnObject(GameObject obj, float density, Vector3 Position)
    {
        List<GrassObjectData.GrassBladeData> points = new List<GrassObjectData.GrassBladeData>();
        int amount = (int)(density);
        for (int i = 0; i < amount; i++)
        {
            float randomRadious = Random.Range(0.0f, 1.0f) * brushScale;
            float randomAngle = Random.Range(0, 360);
            Vector3 randomPoint = GetPointOnCircle(Position, randomRadious, randomAngle);
            randomPoint += new Vector3(0, 3, 0);
            RaycastHit hitInfo;
            if (Physics.Raycast(randomPoint, -Vector3.up, out hitInfo))
            {
                if (hitInfo.collider.gameObject == obj)
                {
                    Vector3 pos = hitInfo.point;
                    GrassObjectData.GrassBladeData data = new GrassObjectData.GrassBladeData();
                    data.Position = pos;
                    data.Light = SampleLightmap(hitInfo).grayscale;
                    data.GroundColor = SampleTexture(hitInfo);
                    points.Add(data);
                }
            }
            Debug.DrawRay(randomPoint, -Vector3.up * 3, Color.cyan, 0.1f);
        }
        return points;
    }
    private Vector3 GetPointOnCircle(Vector3 center, float radious, float angle)
    {
        float X = radious * Mathf.Cos(angle);
        float Y = radious * Mathf.Sin(angle);
        center.x += X;
        center.z += Y;
        return center;
    }

    GrassObjectData ApplyData()
    {
        string filePath = GetFilePath();
        GrassObjectData currentObjectData = AssetDatabase.LoadAssetAtPath<GrassObjectData>(filePath);
        return currentObjectData;
    }
    public void EndDraw()
    {
        //remove mesh collider
        if (selectedObj)
        {
            Object.DestroyImmediate(selectedObj.GetComponent<MeshCollider>());
        }
        SaveData();

    }
    public void SaveData()
    {
        string filePath = GetFilePath();
        if (AssetDatabase.LoadAssetAtPath<GrassObjectData>(filePath) == null)
        {
            AssetDatabase.CreateAsset(currentObjectData, filePath);
        }
        else
        {
            EditorUtility.SetDirty(currentObjectData);
            AssetDatabase.SaveAssets();
        }
    }
    Color SampleTexture(RaycastHit hit)
    {

        Vector2 uv = hit.textureCoord;

        int x = Mathf.FloorToInt(uv.x * currentTexture.width);
        int y = Mathf.FloorToInt(uv.y * currentTexture.height);
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

        if (lightmapIndex > -1)
        {
            LightmapData lightmapData = LightmapSettings.lightmaps[lightmapIndex];
            Texture2D lightmapTexture = lightmapData.lightmapColor;
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
    string GetFilePath()
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
}
