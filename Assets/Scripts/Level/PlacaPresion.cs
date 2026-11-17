using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class PlacaPresion : MonoBehaviour
{
    [Tooltip("Masa mínima que tiene que haber encima. La canica pesa 2; Micro pequeño, 0.5.")]
    [SerializeField] float masaNecesaria = 1.5f;
    [Tooltip("Ancho de la placa, en unidades. Cuenta lo que tenga el centro sobre ella.")]
    [SerializeField] float ancho = 2f;
    [Tooltip("Cuánto baja al activarse, en unidades.")]
    [SerializeField] float hundimiento = 0.08f;

    [Header("Al activarse")]
    [Tooltip("Dibujo de la placa activada (n1_placa_activa).")]
    [SerializeField] Sprite spriteActiva;
    [SerializeField] AudioClip sonido;
    [Tooltip("El puente que baja: arrastra aquí el objeto Puente_Bisagra.")]
    [SerializeField] PuenteLevadizo puente;

    SpriteRenderer pintor;
    Vector3 posInicial;
    float superficie;     
    bool activada;
    bool hayAlgo;
    float hundido;      
    readonly List<Rigidbody2D> cuerpos = new List<Rigidbody2D>();

    public bool Activada { get { return activada; } }

    void Awake()
    {
        pintor = GetComponent<SpriteRenderer>();
        posInicial = transform.position;
        superficie = pintor.bounds.max.y;
    }

    void FixedUpdate()
    {
        float masa = MasaEncima();
        hayAlgo = masa > 0f;
        if (!activada && masa >= masaNecesaria) Activar();
    }

    void Update()
    {
        float objetivo = activada ? 1f : (hayAlgo ? 0.3f : 0f);
        hundido = Mathf.MoveTowards(hundido, objetivo, Time.deltaTime * 6f);

        float temblor = (!activada && hayAlgo) ? Mathf.Sin(Time.time * 40f) * 0.008f : 0f;
        transform.position = posInicial + Vector3.down * (hundimiento * hundido + temblor);

        if (!activada)
        {
            float luz = 0.82f + 0.18f * (0.5f + 0.5f * Mathf.Sin(Time.time * 4f));
            pintor.color = new Color(luz, luz, luz, 1f);
        }
    }

    void Activar()
    {
        activada = true;
        pintor.color = Color.white;
        if (spriteActiva != null) pintor.sprite = spriteActiva;
        AudioManager.Sonar(sonido);

        if (puente != null) puente.Bajar();
        else Debug.LogWarning("PlacaPresion: falta arrastrar Puente_Bisagra al campo 'Puente'.", this);
    }

    float MasaEncima()
    {
        Vector2 centro = new Vector2(posInicial.x, superficie + 0.1f);
        Vector2 tam = new Vector2(ancho + 1.2f, 0.35f);

        cuerpos.Clear();
        float total = 0f;
        foreach (Collider2D c in Physics2D.OverlapBoxAll(centro, tam, 0f))
        {
            if (c.isTrigger) continue;
            Rigidbody2D cuerpo = c.attachedRigidbody;
            if (cuerpo == null || cuerpo.bodyType != RigidbodyType2D.Dynamic) continue;
            if (cuerpos.Contains(cuerpo)) continue;
            if (Mathf.Abs(cuerpo.worldCenterOfMass.x - posInicial.x) > ancho * 0.5f) continue;

            cuerpos.Add(cuerpo);
            total += cuerpo.mass;
        }
        return total;
    }

    void OnDrawGizmosSelected()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;
        float arriba = Application.isPlaying ? superficie : sr.bounds.max.y;
        float x = Application.isPlaying ? posInicial.x : transform.position.x;
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.8f);
        Gizmos.DrawWireCube(new Vector3(x, arriba + 0.1f, 0f), new Vector3(ancho, 0.35f, 0f));
    }
}