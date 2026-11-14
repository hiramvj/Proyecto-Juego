using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class ParallaxLayer : MonoBehaviour
{
    public Transform cam;                        
    [Range(0f, 1f)] public float factor = 0.5f;  
    public bool followY = true;              

    float startX, camStartX, offsetY;

    void Start()
    {
        if (cam == null) cam = Camera.main.transform;
        startX = transform.position.x;
        camStartX = cam.position.x;
        offsetY = transform.position.y - cam.position.y;
    }

    void LateUpdate()
    {
        Vector3 p = transform.position;
        p.x = startX + (cam.position.x - camStartX) * factor;
        if (followY) p.y = cam.position.y + offsetY;
        transform.position = p;
    }
}