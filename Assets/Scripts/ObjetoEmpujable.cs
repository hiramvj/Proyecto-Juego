using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PolygonCollider2D))]
public class ObjetoEmpujable : MonoBehaviour
{
    [Header("Configuración")]
    public float fuerzaEmpuje = 5f;
    public float velocidadMaxima = 3f;

    private Rigidbody2D rb;
    private bool puedeSerEmpujado;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        PlayerController.OnSizeModeChanged += CambiarModo;
    }

    private void OnDisable()
    {
        PlayerController.OnSizeModeChanged -= CambiarModo;
    }

    private void Start()
    {
        PlayerController jugador =
            FindFirstObjectByType<PlayerController>();

        if (jugador != null)
        {
            CambiarModo(jugador.CurrentMode);
        }
    }

    private void CambiarModo(SizeMode modo)
    {
        puedeSerEmpujado = modo == SizeMode.Grande;

        if (puedeSerEmpujado)
        {
            // Modo Grande: desbloquear objeto
            rb.bodyType = RigidbodyType2D.Dynamic;

            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            // Modo Micro: detener antes de bloquear
            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // Bloquear completamente
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!puedeSerEmpujado)
            return;

        PlayerController jugador =
            collision.collider.GetComponentInParent<PlayerController>();

        if (jugador == null)
            return;

        float direccion = Mathf.Sign(
            transform.position.x - jugador.transform.position.x
        );

        rb.AddForce(
            Vector2.right * direccion * fuerzaEmpuje,
            ForceMode2D.Force
        );

        rb.velocity = new Vector2(
            Mathf.Clamp(
                rb.velocity.x,
                -velocidadMaxima,
                velocidadMaxima
            ),
            rb.velocity.y
        );
    }
}