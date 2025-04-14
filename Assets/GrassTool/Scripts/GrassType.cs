using UnityEngine;

[CreateAssetMenu(fileName = "TempGrass", menuName = "GrassTool/GrassType", order = 1)]
public class GrassType : ScriptableObject
{
    public Material mat;
    public Mesh mesh;
}
