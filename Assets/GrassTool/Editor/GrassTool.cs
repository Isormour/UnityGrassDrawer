using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GrassTool
{
    public int Density;
    Material brushMat;
    Mesh brushMesh;
    float brushScale = 1;
    public bool DrawGizmos = true;
    public GrassType grassType;
    public GrassToolViewPortCollider viewportCollider { private set; get; }
    GrassRenderer grassRenderer;

    public GrassTool(Material brushMat, Mesh brushMesh, GrassType grassType)
    {
        this.brushMat = brushMat;
        this.brushMesh = brushMesh;
        this.Density = 1;
        this.grassType = grassType;
        grassRenderer = FindRenderer();
    }

    private void AddSelectedObject(GameObject obj, GrassObjectData tempData)
    {
        GrassRenderer.GrassObject[] old = grassRenderer.grassObjects;
        grassRenderer.grassObjects = new GrassRenderer.GrassObject[grassRenderer.grassObjects.Length + 1];
        for (int i = 0; i < old.Length; i++)
        {
            grassRenderer.grassObjects[i] = old[i];
        }
        grassRenderer.grassObjects[grassRenderer.grassObjects.Length - 1] = new GrassRenderer.GrassObject(obj, tempData);

        Renderer rend = obj.GetComponent<Renderer>();
        Texture2D texture = (Texture2D)rend.sharedMaterial.mainTexture;
        if (rend.lightmapIndex > -1 && rend.lightmapIndex < LightmapSettings.lightmaps.Length)
        {
            Texture2D lightmap = LightmapSettings.lightmaps[rend.lightmapIndex].lightmapColor;
            SetTextureReadable(lightmap, true);
        }
        SetTextureReadable(texture, true);
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
        if (grassRenderer.grassObjects.Length < 1) return;
        Event currentEvent = Event.current;
        currentEvent.Use();
        if (hit.collider && hit.collider.GetComponent<Renderer>())
        {
            GrassObjectData currentData = FindGameObjectData(hit.collider.gameObject);
            currentData.RemoveGrassBlades(hit.point, brushScale);
        }
    }

    private GrassObjectData FindGameObjectData(GameObject gameObject)
    {
        for (int i = 0; i < grassRenderer.grassObjects.Length; i++)
        {
            if (grassRenderer.grassObjects[i].gameObject == gameObject)
            {
                return grassRenderer.grassObjects[i].data;
            }
        }
        GrassObjectData tempData = GrassObjectData.CreateGrassObjectData(gameObject.GetComponent<Renderer>(), new Vector2(100, 100));
        AddSelectedObject(gameObject, tempData);
        return tempData;
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
                GrassObjectData currentData = FindGameObjectData(toAddKeys[i]);

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
        if (renderer)
        {
            int lightmapIndex = renderer.lightmapIndex;
            Color lightmapColor = Color.white;

            lightmapColor.a = 0.3f;
            brushMat.SetColor("_Color", lightmapColor);
        }
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
                data.Light = Mathf.Clamp01(SampleLightmap(hitInfo).grayscale);
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

    public void SaveData(GameObject selectedObj)
    {
        string filePath = GetFilePath(selectedObj);
        if (AssetDatabase.LoadAssetAtPath<GrassObjectData>(filePath) == null)
        {
            AssetDatabase.CreateAsset(FindGameObjectData(selectedObj), filePath);
        }
        else
        {

            EditorUtility.SetDirty(FindGameObjectData(selectedObj));
            SerializedObject so = new SerializedObject(FindGameObjectData(selectedObj));
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
        grassRenderer.gameObject.SetActive(false);
        grassRenderer.gameObject.SetActive(true);

    }
    Color SampleTexture(RaycastHit hit, Texture2D currentTexture)
    {
        Vector2 uv = hit.textureCoord;
        int x = Mathf.FloorToInt(uv.x * currentTexture.width);
        int y = Mathf.FloorToInt(uv.y * currentTexture.height);
        if (!currentTexture.isReadable) SetTextureReadable(currentTexture, true);
        Color color = currentTexture.GetPixel(x, y);
        return color.linear;
    }

    private void SetTextureReadable(Texture2D texture, bool v)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer != null)
        {
            importer.isReadable = v;
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

        string filePath = $"{scenePath}/{grassType.name}_{sceneName}_{selectedObj.GetInstanceID()}.asset";
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
    }
    public void EndDraw()
    {
        GrassRenderer.GrassObject[] currentObjects = grassRenderer.grassObjects;
        foreach (var item in currentObjects)
        {
            SaveData(item.gameObject);
        }
        foreach (var item in currentObjects)
        {
            Renderer rend = item.gameObject.GetComponent<Renderer>();
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

    private GrassRenderer FindRenderer()
    {
        GrassRenderer[] rends = GameObject.FindObjectsOfType<GrassRenderer>();
        if (rends == null)
        {
            return CreateGrassRendererObject();
        }
        foreach (var rend in rends)
        {
            if (rend.grassType == grassType)
            {
                return rend;
            }
        }
        return CreateGrassRendererObject();
    }

    private GrassRenderer CreateGrassRendererObject()
    {
        GameObject grassRendererOB = new GameObject("Grass Renderer " + grassType.name);
        GrassRenderer grassRenderer = grassRendererOB.AddComponent<GrassRenderer>();
        grassRenderer.grassType = grassType;
        return grassRenderer;
    }
}
