using UnityEngine;

public class ObjectGrassRendererGrid
{
    public ObjectGrassRenderer[,] objectGrassRenderers { private set; get; }
    public ObjectGrassRendererGrid(Extensions.TArray<GrassObjectChunk> chunks, Material mat, Mesh mesh)
    {
        objectGrassRenderers = new ObjectGrassRenderer[chunks.Size.x, chunks.Size.y];
        for (int i = 0; i < chunks.Size.x; i++)
        {
            for (int j = 0; j < chunks.Size.y; j++)
            {
                objectGrassRenderers[i, j] = new ObjectGrassRenderer(chunks.Get(i, j), mat, mesh);
            }
        }
    }

    internal void Update()
    {
        foreach (var chunkRenderer in objectGrassRenderers)
        {
            if (chunkRenderer.enabled)
                chunkRenderer.Update();
        }
    }

    internal void Deinitialize()
    {
        foreach (var chunkRenderer in objectGrassRenderers)
        {
            if (chunkRenderer.enabled)
                chunkRenderer.Deinitialize();
        }
    }
}