using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Meta : MonoBehaviour
{
    public static event Action OnMetaAlcanzada;

    bool alcanzada;

    void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (alcanzada) return;                   
        if (!other.CompareTag("Player")) return; 

        PlayerController micro = other.GetComponentInParent<PlayerController>();
        if (micro != null && micro.EstaMuerto) return;

        alcanzada = true;

        if (micro != null) micro.SetControlActivo(false);


        OnMetaAlcanzada?.Invoke();
    }
}
