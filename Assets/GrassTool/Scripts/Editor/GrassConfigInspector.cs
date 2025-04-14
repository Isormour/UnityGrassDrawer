using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrassConfig))]
public class GrassConfigInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GrassConfig data = (GrassConfig)target;
        if (GUILayout.Button("Find grass types"))
        {
            string[] paths = System.IO.Directory.GetFiles("Assets/GrassTool/GrassTypes/");
            List<string> correctPaths = new List<string>();
            foreach (var item in paths)
            {
                if (item.Contains(".meta"))
                    continue;
                correctPaths.Add(item);
            }
            List<GrassType> grassTypes = new List<GrassType>();
            for (int i = 0; i < correctPaths.Count; i++)
            {
                GrassType type = AssetDatabase.LoadAssetAtPath<GrassType>(correctPaths[i]);
                grassTypes.Add(type);
            }

            Debug.Log("GrassTypes found = " + grassTypes.Count);

            EditorUtility.SetDirty(data);
            data.GrassType = grassTypes.ToArray();
            AssetDatabase.SaveAssets();

        }
    }
}
