using UnityEditor;

[CustomEditor(typeof(GrassObjectData))]
public class GrassObjectDataInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GrassObjectData data = (GrassObjectData)target;
        if (data.chunks.Length > 0)
        {

            int blades = 0;
            int chunks = 0;
            foreach (var item in data.chunks.Matrix)
            {
                chunks++;
            }
            EditorGUILayout.LabelField("Grass chunks " + chunks);
            foreach (var item in data.chunks.Matrix)
            {
                blades += item.GrassBlades.Length;
            }
            EditorGUILayout.LabelField("Grass blades " + blades);
        }

    }
}
