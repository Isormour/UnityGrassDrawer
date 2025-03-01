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
            paramsBuffer = new ComputeBuffer(instances, GetStructSize(typeof(BasicInstancedParams)));
    }
    int GetStructSize(Type paramType)
    {
        return System.Runtime.InteropServices.Marshal.SizeOf(paramType);
    }
    public struct BasicInstancedParams
    {
        public Matrix4x4 transformMatrix;
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
        RenderParams rp = new RenderParams(drawMat);

        rp.worldBounds = data.ObjectBounds;
        rp.matProps = new MaterialPropertyBlock();

        for (int i = 0; i < instances; i++)
        {
            instanceParams[i] = new BasicInstancedParams();

            Quaternion rotation = Quaternion.identity;
            instanceParams[i].transformMatrix = Matrix4x4.TRS(data.GrassBlades[i].Position, rotation, new Vector3(1, 1, 1));
            instanceParams[i].light = data.GrassBlades[i].Light;
            instanceParams[i].textureColor = data.GrassBlades[i].GroundColor.linear;
        }
        paramsBuffer.SetData(instanceParams);

        rp.matProps.SetBuffer("_ParamsBuffer", paramsBuffer);

        commandData[0].indexCountPerInstance = drawMesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)instances;

        commandBuf.SetData(commandData);
        Graphics.RenderMeshIndirect(rp, drawMesh, commandBuf, 1);
    }
}
