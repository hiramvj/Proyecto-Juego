using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float smooth = 5f;
    public Vector2 offset = new Vector2(0f, 1f);
    Transform target;

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p == null) return;
            target = p.transform;
        }
        Vector3 desired = new Vector3(target.position.x + offset.x,
                                      target.position.y + offset.y,
                                      -10f);
        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
    }
}