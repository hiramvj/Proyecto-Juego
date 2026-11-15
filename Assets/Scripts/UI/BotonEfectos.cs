using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Le da vida a un botón: sonido al pasar el mouse y al hacer clic,
// y un saltito de tamaño (crece un poco con el mouse encima, se encoge al presionar).
//
// AudioManager se lo agrega solo a todos los botones de cada escena.
// Solo hay que ponerlo a mano cuando un botón necesita un sonido propio.
public class BotonEfectos : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Tooltip("Opcional: sonido propio al hacer clic. Vacío = el clic normal.")]
    [SerializeField] AudioClip sonidoPropio;
    [SerializeField] float escalaEncima = 1.05f;
    [SerializeField] float escalaPresionado = 0.96f;

    Button boton;
    Vector3 escalaBase;
    bool encima;
    bool presionado;

    void Awake()
    {
        boton = GetComponent<Button>();
        escalaBase = transform.localScale;
    }

    void OnDisable()
    {
        encima = false;
        presionado = false;
        transform.localScale = escalaBase;
    }

    bool Activo
    {
        get { return boton == null || boton.IsInteractable(); }
    }

    public void OnPointerEnter(PointerEventData datos)
    {
        encima = true;
        if (Activo) AudioManager.SonarHover();
    }

    public void OnPointerExit(PointerEventData datos)
    {
        encima = false;
        presionado = false;
    }

    public void OnPointerDown(PointerEventData datos)
    {
        presionado = true;
    }

    public void OnPointerUp(PointerEventData datos)
    {
        presionado = false;
    }

    public void OnPointerClick(PointerEventData datos)
    {
        if (datos.button != PointerEventData.InputButton.Left || !Activo) return;

        if (sonidoPropio != null) AudioManager.Sonar(sonidoPropio);
        else AudioManager.SonarClic();
    }

    void Update()
    {
        float objetivo = 1f;
        if (Activo)
        {
            if (presionado) objetivo = escalaPresionado;
            else if (encima) objetivo = escalaEncima;
        }

        // Se acerca suavemente al tamaño objetivo (funciona aunque el juego esté en pausa).
        float paso = 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, escalaBase * objetivo, paso);
    }
}
