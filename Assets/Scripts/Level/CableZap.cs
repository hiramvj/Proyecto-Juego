using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CableZap : MonoBehaviour
{
    [Header("Daño")]
    [SerializeField] int dano = 20;
    [Tooltip("Segundos sin recibir daño después de un chispazo.")]
    [SerializeField] float esperaEntreGolpes = 1f;
    [Tooltip("Velocidad hacia arriba del saltito que da Micro al tocarlo.")]
    [SerializeField] float rebote = 9f;

    [Header("Chispas")]
    [SerializeField] SpriteRenderer chispa;
    [Tooltip("Dibujos que se van turnando: n1_chispa_a y n1_chispa_b.")]
    [SerializeField] Sprite[] dibujosChispa;
    [SerializeField] SpriteRenderer brillo;

    [Header("Sonido")]
    [SerializeField] AudioClip sonidoZap;
    [SerializeField] AudioClip sonidoChispa;
    [Tooltip("Se oye el chisporroteo solo si Micro está a menos de esta distancia.")]
    [SerializeField] float distanciaSonido = 7f;

    Transform micro;
    float proximoGolpe;
    float proximoCambio;
    float proximoChisporroteo;
    float estallido;                 
    Vector3 escalaChispa = Vector3.one;
    Vector3 escalaBrillo = Vector3.one;
    Color colorBrillo = Color.white;

    void Reset()
    {
        BoxCollider2D c = GetComponent<BoxCollider2D>();
        c.isTrigger = true;
        c.offset = Vector2.zero;
        c.size = new Vector2(0.5f, 0.25f);
    }

    void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        if (chispa != null) escalaChispa = chispa.transform.localScale;
        if (brillo != null)
        {
            escalaBrillo = brillo.transform.localScale;
            colorBrillo = brillo.color;
        }
    }

    void Start()
    {
        PlayerController jugador = FindFirstObjectByType<PlayerController>();
        if (jugador != null) micro = jugador.transform;
        proximoChisporroteo = Time.time + Random.Range(0.5f, 1.5f);
    }

    void Update()
    {
        estallido = Mathf.MoveTowards(estallido, 0f, Time.deltaTime / 0.35f);
        AnimarChispa();
        AnimarBrillo();
        Chisporrotear();
    }

    void OnTriggerEnter2D(Collider2D otro) { Tocar(otro); }
    void OnTriggerStay2D(Collider2D otro) { Tocar(otro); }

    void Tocar(Collider2D otro)
    {
        if (Time.time < proximoGolpe) return;
        if (!otro.CompareTag("Player")) return;                    
        if (GameManager.Estado != EstadoJuego.Jugando) return;

        PlayerController jugador = otro.GetComponentInParent<PlayerController>();
        if (jugador == null || jugador.EstaMuerto) return;

        proximoGolpe = Time.time + esperaEntreGolpes;
        estallido = 1f;
        AudioManager.Sonar(sonidoZap);

        PlayerEnergy energia = PlayerEnergy.Instance;
        if (energia != null) energia.TakeDamage(dano);

        Rigidbody2D cuerpo = otro.attachedRigidbody;
        if (cuerpo != null && !jugador.EstaMuerto)
            cuerpo.velocity = new Vector2(cuerpo.velocity.x, Mathf.Max(cuerpo.velocity.y, rebote));
    }

    void AnimarChispa()
    {
        if (chispa == null || Time.time < proximoCambio) return;
        proximoCambio = Time.time + Random.Range(0.04f, 0.11f);

        bool visible = estallido > 0f || Random.value < 0.7f;
        chispa.enabled = visible;
        if (!visible) return;

        if (dibujosChispa != null && dibujosChispa.Length > 0)
            chispa.sprite = dibujosChispa[Random.Range(0, dibujosChispa.Length)];
        chispa.flipX = Random.value < 0.5f;
        chispa.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));
        float tam = Random.Range(0.7f, 1.1f) * (1f + 0.9f * estallido);
        chispa.transform.localScale = escalaChispa * tam;
    }

    void AnimarBrillo()
    {
        if (brillo == null) return;
        float luz = Mathf.Lerp(0.35f, 1f, Mathf.PerlinNoise(Time.time * 9f, 0.3f));
        luz = Mathf.Max(luz, estallido);
        Color c = colorBrillo;
        c.a = colorBrillo.a * luz;
        brillo.color = c;
        brillo.transform.localScale = escalaBrillo * (1f + 0.6f * estallido);
    }

    void Chisporrotear()
    {
        if (Time.time < proximoChisporroteo) return;
        proximoChisporroteo = Time.time + Random.Range(0.9f, 2.2f);
        if (micro == null || sonidoChispa == null) return;

        float distancia = Mathf.Abs(micro.position.x - transform.position.x);
        float volumen = Mathf.Clamp01(1f - distancia / distanciaSonido) * 0.6f;
        if (volumen > 0.05f) AudioManager.Sonar(sonidoChispa, volumen);
    }
}