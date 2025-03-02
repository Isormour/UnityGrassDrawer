using System;
using UnityEngine;

public class ObjectGrassRenderer
{
    GraphicsBuffer commandBuf;
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    ComputeBuffer paramsBuffer;
    int instances;

    [SerializeField] GrassObjectData data;
    [SerializeField] Material drawMat;
    [SerializeField] Mesh drawMesh;
    BasicInstancedParams[] instanceParams;
    RenderParams renderParams;
    public ObjectGrassRenderer(GrassObjectData data, Material drawMat, Mesh drawMesh)
    {
        this.data = data;
        this.drawMat = new Material(drawMat);
        this.drawMesh = drawMesh;
        this.instances = data.GrassBlades.Length;
        this.commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        this.commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        instanceParams = new BasicInstancedParams[instances];
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
        if (data && drawMat && drawMesh && data.GrassBlades.Length > 0)
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
        Graphics.RenderMeshIndirect(renderParams, drawMesh, commandBuf, 1);
    }

    private void InitializeRender()
    {
        renderParams = new RenderParams(drawMat);

        renderParams.worldBounds = data.ObjectBounds;
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
}
