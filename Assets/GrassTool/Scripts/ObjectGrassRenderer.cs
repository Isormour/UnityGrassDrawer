using System;
using UnityEngine;

public class ObjectGrassRenderer
{
    GraphicsBuffer commandBuf;
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    ComputeBuffer paramsBuffer;
    int instances;

    [SerializeField] GrassObjectChunk data;
    [SerializeField] Material drawMat;
    [SerializeField] Mesh drawMesh;
    BasicInstancedParams[] instanceParams;
    RenderParams renderParams;
    public bool enabled { private set; get; }

    public GrassObjectChunk ChunkData => data;
    public Bounds bounds;

    public ObjectGrassRenderer(GrassObjectChunk data, Material drawMat, Mesh drawMesh)
    {
        this.data = data;
        this.drawMat = new Material(drawMat);
        this.drawMesh = drawMesh;
        this.instances = data.GrassBlades.Length;
        this.commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        this.commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        instanceParams = new BasicInstancedParams[instances];
        enabled = instances > 0;

        bounds = new Bounds();
        bounds.center = data.ObjectBounds.center + new Vector3(0, 1, 0);
        bounds.extents = data.ObjectBounds.extents;

        if (instances > 0)
        {
            paramsBuffer = new ComputeBuffer(instances, GetStructSize(typeof(BasicInstancedParams)));
            InitializeRender();
        }

    }
    int GetStructSize(Type paramType)
    {
        return System.Runtime.InteropServices.Marshal.SizeOf(paramType);
    }
    public struct BasicInstancedParams
    {
        public Vector3 position;
        public float light;
        public Color textureColor;
    }

    // Update is called once per frame
    public void Update()
    {
        Draw();
    }
    public void Deinitialize()
    {
        commandBuf?.Release();
        commandBuf = null;
        paramsBuffer?.Release();
        paramsBuffer = null;
#if UNITY_EDITOR
        Material.DestroyImmediate(drawMat);
#else
        Material.Destroy(drawMat);
#endif

    }

    public void Draw()
    {
        //if (IsVisible())
        {
            Graphics.RenderMeshIndirect(renderParams, drawMesh, commandBuf, 1);
        }
    }

    private bool IsVisible()
    {
        Bounds renderBound = new Bounds();
        renderBound.center = bounds.center;
        renderBound.extents = bounds.extents;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        bool inBounds = GeometryUtility.TestPlanesAABB(planes, renderBound);
        return inBounds;
    }

    private void InitializeRender()
    {
        renderParams = new RenderParams(drawMat);
        renderParams.worldBounds = bounds;
        renderParams.matProps = new MaterialPropertyBlock();
        for (int i = 0; i < instances; i++)
        {
            instanceParams[i] = new BasicInstancedParams();
            instanceParams[i].position = data.GrassBlades[i].Position;
            instanceParams[i].light = data.GrassBlades[i].Light;
            instanceParams[i].textureColor = data.GrassBlades[i].GroundColor;
        }
        paramsBuffer.SetData(instanceParams);

        renderParams.matProps.SetBuffer("_ParamsBuffer", paramsBuffer);

        commandData[0].indexCountPerInstance = drawMesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)instances;

        commandBuf.SetData(commandData);
    }

    internal void DrawGizmo()
    {
        Color col = Color.gray;
        col.a = 0.5f;
        Gizmos.color = col;
        if (IsVisible())
        {
            Gizmos.DrawCube(bounds.center, bounds.extents * 2);
        }
    }
}
