using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Canica : MonoBehaviour
{
    [Header("Zona donde se puede usar (X del mundo)")]
    [Tooltip("Si el centro de la canica queda a la izquierda de este valor, reaparece.")]
    [SerializeField] float zonaMinX = 28.2f;
    [Tooltip("Si queda a la derecha de este valor, reaparece.")]
    [SerializeField] float zonaMaxX = 34.6f;
    [Tooltip("Segundos fuera de la zona antes de reaparecer.")]
    [SerializeField] float esperaReaparecer = 0.6f;

    [Header("Movimiento")]
    [Tooltip("Velocidad máxima al rodar, en unidades por segundo.")]
    [SerializeField] float velocidadMaxima = 6f;

    [Header("Dibujo y sonido")]
    [Tooltip("Hijo con el reflejo de luz. Si se deja vacío, usa el primer hijo.")]
    [SerializeField] Transform brillo;
    [SerializeField] AudioClip sonidoReaparecer;

    Rigidbody2D rb;
    CircleCollider2D col;
    Vector3 posInicial;
    Vector3 escalaInicial;
    float fueraDesde = -1f;
    bool reapareciendo;
    bool frenadaPorMicro;

    void Reset()
    {
        Rigidbody2D r = GetComponent<Rigidbody2D>();
        r.bodyType = RigidbodyType2D.Dynamic;
        r.mass = 2f;
        r.drag = 0.2f;
        r.angularDrag = 1f;
        r.gravityScale = 3f;
        r.interpolation = RigidbodyInterpolation2D.Interpolate;
        r.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        GetComponent<CircleCollider2D>().radius = 0.5f;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        posInicial = transform.position;
        escalaInicial = transform.localScale;

        if (col.sharedMaterial == null)
        {
            PhysicsMaterial2D material = new PhysicsMaterial2D("Canica");
            material.friction = 0.6f;
            material.bounciness = 0f;
            col.sharedMaterial = material;
        }

        if (brillo == null && transform.childCount > 0) brillo = transform.GetChild(0);
    }

    void FixedUpdate()
    {
        if (reapareciendo) return;

        Vector2 v = rb.velocity;
        if (Mathf.Abs(v.x) > velocidadMaxima)
            rb.velocity = new Vector2(Mathf.Sign(v.x) * velocidadMaxima, v.y);

        Vector2 p = rb.position;
        bool fuera = p.x < zonaMinX || p.x > zonaMaxX || p.y < posInicial.y - 3f;
        if (!fuera)
        {
            fueraDesde = -1f;
            return;
        }

        if (fueraDesde < 0f) fueraDesde = Time.time;
        else if (Time.time - fueraDesde >= esperaReaparecer) StartCoroutine(Reaparecer());
    }

    void LateUpdate()
    {
        if (brillo != null) brillo.rotation = Quaternion.identity; 
    }


    void OnCollisionEnter2D(Collision2D choque) { RevisarContacto(choque); }
    void OnCollisionStay2D(Collision2D choque) { RevisarContacto(choque); }

    void OnCollisionExit2D(Collision2D choque)
    {
        if (choque.collider.GetComponentInParent<PlayerController>() != null) Soltar();
    }

    void RevisarContacto(Collision2D choque)
    {
        PlayerController micro = choque.collider.GetComponentInParent<PlayerController>();
        if (micro == null) return;

        if (micro.CanPush) Soltar();  
        else Frenar();               
    }

    void Frenar()
    {
        if (frenadaPorMicro) return;
        frenadaPorMicro = true;
        rb.velocity = new Vector2(0f, rb.velocity.y);
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    void Soltar()
    {
        if (!frenadaPorMicro) return;
        frenadaPorMicro = false;
        rb.constraints = RigidbodyConstraints2D.None;
    }


    IEnumerator Reaparecer()
    {
        reapareciendo = true;
        Soltar();
        rb.simulated = false;  

        yield return StartCoroutine(Escalar(1f, 0f, 0.25f, false));

        transform.SetPositionAndRotation(posInicial, Quaternion.identity);
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        while (SitioOcupado()) yield return new WaitForSeconds(0.2f);

        AudioManager.Sonar(sonidoReaparecer);
        yield return StartCoroutine(Escalar(0f, 1f, 0.3f, true));

        rb.simulated = true;
        fueraDesde = -1f;
        reapareciendo = false;
    }

    bool SitioOcupado()
    {
        float radio = col.radius * 1.1f;
        foreach (Collider2D otro in Physics2D.OverlapCircleAll(posInicial, radio))
        {
            if (otro == col || otro.isTrigger) continue;
            Rigidbody2D cuerpo = otro.attachedRigidbody;
            if (cuerpo == null || cuerpo.bodyType == RigidbodyType2D.Static) continue;   
            return true;
        }
        return false;
    }

    IEnumerator Escalar(float desde, float hasta, float duracion, bool conRebote)
    {
        for (float t = 0f; t < 1f; t += Time.deltaTime / duracion)
        {
            float k = conRebote ? SalidaConRebote(t) : Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = escalaInicial * Mathf.LerpUnclamped(desde, hasta, k);
            yield return null;
        }
        transform.localScale = escalaInicial * hasta;
    }

    static float SalidaConRebote(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float u = t - 1f;
        return 1f + c3 * u * u * u + c1 * u * u;
    }
}