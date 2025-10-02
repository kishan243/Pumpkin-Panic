using UnityEngine;
public class randomMove : MonoBehaviour
{
    public float speed = 2f;
    public float rangeX = 5f, rangeZ = 5f;

    Vector3 dir;
    float t;

    void Start()
    {
        PickDirection();
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t > 2f) { PickDirection(); t = 0; }

        transform.position += dir * speed * Time.deltaTime;

        var pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -rangeX, rangeX);
        pos.y = Mathf.Clamp(pos.y, 2, 5);
        pos.z = Mathf.Clamp(pos.z, -rangeZ, rangeZ);
        transform.position = pos;
    }
    void PickDirection()
    {
        dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f,
                              1f)).normalized;
    }
}

