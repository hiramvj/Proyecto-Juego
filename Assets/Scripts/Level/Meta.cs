using System;
using UnityEngine;

// Va en el objeto raíz del prefab Meta (el banderín).
// Necesita un Box Collider 2D con Is Trigger activado en el mismo objeto.
[RequireComponent(typeof(BoxCollider2D))]
public class Meta : MonoBehaviour
{
    // Aviso global: "Micro llegó a la meta".
    // Lo escucha la pantalla de victoria (Parte 1B) y, desde la Parte 2, el GameManager.
    public static event Action OnMetaAlcanzada;

    bool alcanzada;

    // Unity llama a Reset() al añadir el script en el Inspector:
    // deja el collider como trigger automáticamente.
    void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (alcanzada) return;                    // solo cuenta la primera vez
        if (!other.CompareTag("Player")) return;  // solo Micro activa la meta

        PlayerController micro = other.GetComponentInParent<PlayerController>();
        if (micro != null && micro.EstaMuerto) return;

        alcanzada = true;

        // Micro deja de responder al teclado y se queda quieto en la bandera.
        if (micro != null) micro.SetControlActivo(false);

        Debug.Log("¡Meta alcanzada!");
        OnMetaAlcanzada?.Invoke();
    }
}
