using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrassObjectData : ScriptableObject
{
    public TArray<GrassObjectChunk> chunks;
    public Bounds ObjectBounds;
    public Action OnRefreshRenderer;
    public Vector2 ChunkSize;

    public static GrassObjectData CreateGrassObjectData(Renderer rend, Vector2 ChunkSize)
    {
        GrassObjectData grassObjectData = CreateInstance<GrassObjectData>();
        grassObjectData.ObjectBounds = rend.bounds;
        grassObjectData.ChunkSize = ChunkSize;
        grassObjectData.GenerateChunks();
        return grassObjectData;

    }

    private void GenerateChunks()
    {
        Bounds[,] bounds = GenerateChunkBounds();
        chunks = new GrassObjectChunk[bounds.GetLength(0), bounds.GetLength(1)];
        for (int i = 0; i < bounds.GetLength(0); i++)
        {

            for (int j = 0; j < bounds.GetLength(1); j++)
            {
                chunks.Set(i, j, new GrassObjectChunk(bounds[i, j]));
            }
        }
    }

    public void AddGrassBlades(List<GrassObjectChunk.GrassBladeData> tempGrassBlades)
    {
        Dictionary<Tuple<int, int>, List<GrassObjectChunk.GrassBladeData>> chunksToGrassblades =
            new Dictionary<Tuple<int, int>, List<GrassObjectChunk.GrassBladeData>>();

        foreach (var item in tempGrassBlades)
        {
            Vector3 point = item.Position;
            Vector2 sizeDelta = new Vector2((ObjectBounds.size.x / ChunkSize.x), (ObjectBounds.size.z / ChunkSize.y));
            float distX = point.x - ObjectBounds.min.x;
            float distZ = point.z - ObjectBounds.min.z;
            int chunkX = Mathf.RoundToInt(distX / ChunkSize.x);
            int chunkY = Mathf.RoundToInt(distZ / ChunkSize.y);

            Tuple<int, int> chunkXY = new Tuple<int, int>(chunkX, chunkY);
            if (!chunksToGrassblades.ContainsKey(chunkXY))
            {
                chunksToGrassblades.Add(chunkXY, new List<GrassObjectChunk.GrassBladeData>());
            }
            chunksToGrassblades[chunkXY].Add(item);
        }

        List<Tuple<int, int>> keys = chunksToGrassblades.Keys.ToList();
        foreach (var key in keys)
        {
            chunks.Get(key.Item1, key.Item2).AddGrassBlades(chunksToGrassblades[key]);
        }
        OnRefreshRenderer?.Invoke();
    }

    public GrassObjectChunk GetChunkByPosition(Vector3 point)
    {
        Vector2 sizeDelta = new Vector2((ObjectBounds.size.x / ChunkSize.x), (ObjectBounds.size.z / ChunkSize.y));
        float distX = Mathf.Abs(ObjectBounds.min.x) + Mathf.Abs(point.x);
        float distZ = Mathf.Abs(ObjectBounds.min.z) + Mathf.Abs(point.z);
        int chunkX = Mathf.RoundToInt(distX / ChunkSize.x);
        int chunkY = Mathf.RoundToInt(distZ / ChunkSize.y);
        return chunks.Get(chunkX, chunkY);
    }
    public void RemoveGrassBlades(Vector3 point, float distance)
    {
        GrassObjectChunk selectedChunk = GetChunkByPosition(point);
        List<GrassObjectChunk.GrassBladeData> tempGrassBlades = selectedChunk.GrassBlades.ToList();
        for (int i = 0; i < tempGrassBlades.Count; i++)
        {
            if (Vector3.Distance(tempGrassBlades[i].Position, point) < 1 * distance)
            {
                tempGrassBlades.RemoveAt(i);
                i--;
            }
        }
        selectedChunk.GrassBlades = tempGrassBlades.ToArray();
        OnRefreshRenderer?.Invoke();
    }
    private Bounds[,] GenerateChunkBounds()
    {
        int columns = Mathf.RoundToInt(ObjectBounds.size.x / ChunkSize.x);
        int rows = Mathf.RoundToInt(ObjectBounds.size.z / ChunkSize.y);
        columns += 1;
        rows += 1;

        float sizeX = ObjectBounds.size.x / columns;
        float sizeY = ObjectBounds.size.z / rows;

        Bounds[,] chunks = new Bounds[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 centerPoint = new Vector3(0, 0, 0);

                centerPoint.x = ObjectBounds.min.x + sizeX / 2;
                centerPoint.x += i * sizeX;

                centerPoint.y = ObjectBounds.center.y;

                centerPoint.z = ObjectBounds.min.z + sizeY / 2;
                centerPoint.z += j * sizeY;


                chunks[i, j] = new Bounds(centerPoint, new Vector3(sizeX, 1, sizeY));
            }
        }
        return chunks;
    }
    [System.Serializable]
    public class ChunkArray
    {
        public List<GrassObjectChunk> array;
        public ChunkArray()
        {
            array = new List<GrassObjectChunk>();
        }
    }
}