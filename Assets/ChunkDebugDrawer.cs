using UnityEngine;

[ExecuteAlways]
public class ChunkDebugDrawer : MonoBehaviour
{
    MeshRenderer renderer;
    Bounds bounds;
    Bounds[,] chunks;
    public Vector2 ChunkSize = new Vector2(10, 10);

    private void OnEnable()
    {
        renderer = GetComponent<MeshRenderer>();
        bounds = renderer.bounds;
        bounds.size = bounds.size + new Vector3(-1f, 0.1f, 0);
        GenerateChunks();

    }

    private void GenerateChunks()
    {
        int columns = Mathf.RoundToInt(bounds.size.x / ChunkSize.x);
        int rows = Mathf.RoundToInt(bounds.size.z / ChunkSize.y);
        columns += 1;
        rows += 1;

        float sizeX = bounds.size.x / columns;
        float sizeY = bounds.size.z / rows;

        chunks = new Bounds[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 centerPoint = new Vector3(0, 0, 0);

                centerPoint.x = bounds.min.x + sizeX / 2;
                centerPoint.x += i * sizeX;

                centerPoint.y = bounds.center.y;

                centerPoint.z = bounds.min.z + sizeY / 2;
                centerPoint.z += j * sizeY;


                chunks[i, j] = new Bounds(centerPoint, new Vector3(sizeX, 1, sizeY));
            }
        }
    }

    private void OnDrawGizmos()
    {
        Color col = Color.cyan;
        col.a = 0.5f;
        // Gizmos.color = col;
        Gizmos.DrawCube(bounds.center, bounds.size);

        col = Color.yellow;
        col.a = 0.5f;
        Gizmos.color = col;

        foreach (var chunk in chunks)
        {
            Gizmos.DrawCube(chunk.center, chunk.size);
        }

    }
}
