using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PuenteLevadizo : MonoBehaviour
{
    [Header("Ángulos en Z (grados)")]
    [SerializeField] float anguloArriba = 90f;
    [SerializeField] float anguloAbajo = 0f;

    [Header("Caída")]
    [SerializeField] float duracionTemblor = 0.35f;
    [SerializeField] float duracionCaida = 0.7f;
    [Tooltip("Cuánto rebota al golpear, en grados.")]
    [SerializeField] float rebote = 6f;
    [SerializeField] AudioClip sonidoCrujido;
    [SerializeField] AudioClip sonidoGolpe;

    [Header("Cámara")]
    [SerializeField] bool mostrarConCamara = true;
    [Tooltip("Adónde va la cámara para mostrar el puente (X, Y).")]
    [SerializeField] Vector2 puntoCamara = new Vector2(51f, 2.2f);
    [SerializeField] float duracionViaje = 0.8f;
    [Tooltip("Segundos que la cámara se queda mirando el puente ya bajado.")]
    [SerializeField] float pausaFinal = 0.5f;

    public bool Bajado { get; private set; }

    Rigidbody2D rb;
    bool enMarcha;

    void Reset()
    {
        Rigidbody2D r = GetComponent<Rigidbody2D>();
        r.bodyType = RigidbodyType2D.Kinematic;
        r.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;  
        Girar(anguloArriba, true);             
    }

    public void Bajar()
    {
        if (Bajado || enMarcha) return;
        StartCoroutine(RutinaBajar());
    }

    IEnumerator RutinaBajar()
    {
        enMarcha = true;

        Transform cam = null;
        CameraFollow seguidor = null;
        PlayerController micro = null;
        if (mostrarConCamara && Camera.main != null)
        {
            cam = Camera.main.transform;
            seguidor = cam.GetComponent<CameraFollow>();
            micro = FindFirstObjectByType<PlayerController>();
            if (micro != null) micro.SetControlActivo(false);
            if (seguidor != null) seguidor.enabled = false;

            yield return new WaitForSeconds(0.3f);
            Vector3 destino = new Vector3(puntoCamara.x, puntoCamara.y, cam.position.z);
            yield return StartCoroutine(MoverCamara(cam, destino, duracionViaje));
        }

        AudioManager.Sonar(sonidoCrujido);
        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime / duracionTemblor)
        {
            Girar(anguloArriba + Mathf.Sin(t * Mathf.PI * 7f) * 2f, false);
            yield return new WaitForFixedUpdate();
        }

        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime / duracionCaida)
        {
            Girar(Mathf.Lerp(anguloArriba, anguloAbajo, t * t), false);
            yield return new WaitForFixedUpdate();
        }
        Girar(anguloAbajo, false);
        AudioManager.Sonar(sonidoGolpe);

        yield return StartCoroutine(Rebotar(rebote, 0.2f));
        yield return StartCoroutine(Rebotar(rebote * 0.35f, 0.12f));
        Girar(anguloAbajo, false);
        Bajado = true;

        if (cam != null)
        {
            yield return new WaitForSeconds(pausaFinal);
            if (seguidor != null) seguidor.enabled = true;
            yield return new WaitForSeconds(0.4f);
            if (micro != null && GameManager.Estado == EstadoJuego.Jugando) micro.SetControlActivo(true);
        }

        enMarcha = false;
    }

    IEnumerator Rebotar(float grados, float duracion)
    {
        float haciaArriba = Mathf.Sign(anguloArriba - anguloAbajo);
        for (float t = 0f; t < 1f; t += Time.fixedDeltaTime / duracion)
        {
            Girar(anguloAbajo + haciaArriba * grados * Mathf.Sin(t * Mathf.PI), false);
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator MoverCamara(Transform cam, Vector3 destino, float duracion)
    {
        Vector3 desde = cam.position;
        for (float t = 0f; t < 1f; t += Time.deltaTime / duracion)
        {
            cam.position = Vector3.Lerp(desde, destino, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        cam.position = destino;
    }

    void Girar(float angulo, bool alInstante)
    {
        if (alInstante)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, angulo);
            rb.rotation = angulo;
        }
        else
        {
            rb.MoveRotation(angulo);
        }
    }
}