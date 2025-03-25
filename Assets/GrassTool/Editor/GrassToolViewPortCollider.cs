using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class GrassToolViewPortCollider
{
    public UnityAction<GameObject> OnGameObjectEnter;
    public List<GameObject> objectsInView { private set; get; }
    public GrassToolViewPortCollider()
    {
        objectsInView = new List<GameObject>();
        SceneView.duringSceneGui += Update;
    }
    public void Dispose()
    {
        SceneView.duringSceneGui -= Update;
    }

    private void Update(SceneView obj)
    {
        Vector3 sceneCameraPos = SceneView.lastActiveSceneView.camera.transform.position;
        List<GameObject> foundObjs = FindObjects(10000, LayerMask.GetMask(new string[] { "Default" }));
        for (int i = 0; i < objectsInView.Count; i++)
        {
            GameObject item = objectsInView[i];
            if (!foundObjs.Contains(item))
            {
                objectsInView.Remove(item);
                i--;
            }
        }
        foreach (var foundObj in foundObjs)
        {
            if (!objectsInView.Contains(foundObj))
            {
                objectsInView.Add(foundObj);
                OnGameObjectEnter?.Invoke(foundObj);
                //enter viewport;
            }
        }

    }
    List<GameObject> FindObjects(float distance, int mask)
    {
        List<GameObject> FoundObjects = new List<GameObject>();
        Camera cam = SceneView.lastActiveSceneView.camera;
        if (!cam) return null;
        float halfFov = cam.fieldOfView * 0.5f;
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        Collider[] hits = Physics.OverlapSphere(cam.transform.position, distance, mask);

        foreach (Collider hit in hits)
        {
            if (GeometryUtility.TestPlanesAABB(planes, hit.bounds))
            {
                if (hit.gameObject.GetComponent<Renderer>())
                    FoundObjects.Add(hit.gameObject);
            }
        }

        return FoundObjects;
    }
}