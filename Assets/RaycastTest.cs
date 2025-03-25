using UnityEngine;

[ExecuteAlways]
public class RaycastTest : MonoBehaviour
{
    [SerializeField] private bool _drawRay = false;
    [SerializeField] private float _maxDistance = 3;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool hit = false;
        Ray ray = new Ray(this.transform.position, Vector3.up);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, _maxDistance))
        {
            Debug.Log("uv coord " + hitInfo.textureCoord);
            hit = true;
        }
        Color hitColor = hit ? Color.green : Color.red;
        Debug.DrawRay(ray.origin, ray.direction * _maxDistance, hitColor, 0.1f);
    }
}
