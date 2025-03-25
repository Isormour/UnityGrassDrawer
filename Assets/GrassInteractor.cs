using UnityEngine;

[ExecuteAlways]
public class GrassInteractor : MonoBehaviour
{
    Vector3 startpos;
    float currentTime = 0;
    [SerializeField] bool _animate;
    [SerializeField] float _speed;
    [SerializeField] float _distance;
    // Start is called before the first frame update
    void Start()
    {
        startpos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_animate)
        {
            startpos = this.transform.position;
            return;
        }


        currentTime += Time.deltaTime * _speed;
        this.transform.position = startpos + new Vector3(Mathf.Sin(currentTime) * _distance, 0, Mathf.Cos(currentTime) * _distance);
        Shader.SetGlobalVector("_InteractPosition", this.transform.position);
    }
}
