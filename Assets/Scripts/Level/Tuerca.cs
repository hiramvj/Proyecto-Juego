using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Tuerca : MonoBehaviour
{
    [SerializeField] int puntos = 100;
    [SerializeField] AudioClip sonido;

    [Header("Animación")]
    [Tooltip("Cuánto sube y baja, en unidades.")]
    [SerializeField] float alturaFlotar = 0.12f;
    [SerializeField] float velocidadFlotar = 2.2f;
    [Tooltip("Velocidad del giro tipo moneda. 0 = no gira.")]
    [SerializeField] float velocidadGiro = 2.4f;

    Transform dibujo;         
    SpriteRenderer pintor;
    float fase;
    bool recogida;

    void Reset()
    {
        CircleCollider2D c = GetComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius = 0.3f;
    }

    void Awake()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;

        SpriteRenderer original = GetComponent<SpriteRenderer>();
        GameObject hijo = new GameObject("Dibujo");
        hijo.transform.SetParent(transform, false);
        pintor = hijo.AddComponent<SpriteRenderer>();
        pintor.sprite = original.sprite;
        pintor.color = original.color;
        pintor.sharedMaterial = original.sharedMaterial;
        pintor.sortingLayerID = original.sortingLayerID;
        pintor.sortingOrder = original.sortingOrder;
        original.enabled = false;
        dibujo = hijo.transform;

        fase = transform.position.x * 1.7f; 
    }

    void Update()
    {
        if (recogida) return;

        float t = Time.time;
        dibujo.localPosition = Vector3.up * (Mathf.Sin(t * velocidadFlotar + fase) * alturaFlotar);

        float ancho = velocidadGiro > 0f ? Mathf.Cos(t * velocidadGiro + fase) : 1f;
        dibujo.localScale = new Vector3(ancho, 1f, 1f);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (recogida) return;
        if (!otro.CompareTag("Player")) return;        

        PlayerController micro = otro.GetComponentInParent<PlayerController>();
        if (micro != null && micro.EstaMuerto) return;

        if (!GameManager.RecogerTuerca(puntos)) return;  // la partida ya terminó

        recogida = true;
        GetComponent<CircleCollider2D>().enabled = false;
        AudioManager.Sonar(sonido);
        StartCoroutine(Desaparecer());
    }

    IEnumerator Desaparecer()
    {
        Vector3 inicio = dibujo.localPosition;
        Color c = pintor.color;
        float alfa = c.a;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.28f;
            float k = Mathf.Clamp01(t);
            dibujo.localPosition = inicio + Vector3.up * (0.7f * k);
            dibujo.localScale = Vector3.one * (1f + 0.6f * k);
            c.a = alfa * (1f - k);
            pintor.color = c;
            yield return null;
        }

        Destroy(gameObject);
    }
}
