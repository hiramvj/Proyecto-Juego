using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float smooth = 5f;
    public Vector2 offset = new Vector2(0f, 1f);

    [Header("Límites del nivel (opcional)")]
    public bool useLimits = false;
    public Vector2 minPos = new Vector2(-1000f, -1000f);
    public Vector2 maxPos = new Vector2(1000f, 1000f);

    Transform target;

    void LateUpdate()
    {
        bool justFound = false;
        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p == null) return;
            target = p.transform;
            justFound = true;
        }

        Vector3 desired = new Vector3(target.position.x + offset.x,
                                      target.position.y + offset.y,
                                      -10f);

        if (useLimits)
        {
            desired.x = Mathf.Clamp(desired.x, minPos.x, maxPos.x);
            desired.y = Mathf.Clamp(desired.y, minPos.y, maxPos.y);
        }

        if (justFound)
            transform.position = desired; 
        else
            transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
    }
}