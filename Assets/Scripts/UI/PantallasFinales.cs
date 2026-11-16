using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PantallasFinales : MonoBehaviour
{
    [Header("Victoria")]
    [SerializeField] GameObject panelVictoria;
    [SerializeField] Button botonSiguiente;
    [SerializeField] Button botonReintentar;
    [SerializeField] Button botonMenu;
    [Tooltip("Segundos entre tocar la meta y mostrar la pantalla.")]
    [SerializeField] float retrasoVictoria = 0.6f;
    [SerializeField] AudioClip sonidoVictoria;

    [Header("Derrota")]
    [SerializeField] GameObject panelDerrota;
    [SerializeField] Button botonReintentarDerrota;
    [SerializeField] Button botonMenuDerrota;
    [Tooltip("Segundos entre la muerte de Micro y la pantalla, para que se vea la animación.")]
    [SerializeField] float retrasoDerrota = 1.2f;
    [SerializeField] AudioClip sonidoDerrota;

    [Header("Animación de entrada")]
    [Tooltip("Segundos que tarda el panel en aparecer.")]
    [SerializeField] float duracionEntrada = 0.3f;

    bool mostrando;

    void Awake()
    {

        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        if (botonSiguiente != null) botonSiguiente.onClick.AddListener(GameManager.SiguienteNivel);
        if (botonReintentar != null) botonReintentar.onClick.AddListener(GameManager.ReiniciarNivel);
        if (botonMenu != null) botonMenu.onClick.AddListener(GameManager.IrAlMenu);
        if (botonReintentarDerrota != null) botonReintentarDerrota.onClick.AddListener(GameManager.ReiniciarNivel);
        if (botonMenuDerrota != null) botonMenuDerrota.onClick.AddListener(GameManager.IrAlMenu);
    }

    void OnEnable()
    {
        GameManager.OnVictoria += AlGanar;
        GameManager.OnDerrota += AlPerder;
    }

    void OnDisable()
    {
        GameManager.OnVictoria -= AlGanar;
        GameManager.OnDerrota -= AlPerder;
    }

    void AlGanar()
    {
        if (mostrando) return;
        mostrando = true;
        AudioManager.Sonar(sonidoVictoria);        
        StartCoroutine(MostrarPanel(panelVictoria, retrasoVictoria));
    }

    void AlPerder()
    {
        if (mostrando) return;
        mostrando = true;
        AudioManager.Sonar(sonidoDerrota);       
        StartCoroutine(MostrarPanel(panelDerrota, retrasoDerrota));
    }

    IEnumerator MostrarPanel(GameObject panel, float retraso)
    {
        if (panel == null)
        {
            Debug.LogWarning("PantallasFinales: falta asignar el panel en el Inspector.");
            yield break;
        }

        if (retraso > 0f) yield return new WaitForSecondsRealtime(retraso);

        CanvasGroup grupo = panel.GetComponent<CanvasGroup>();
        if (grupo == null) grupo = panel.AddComponent<CanvasGroup>();

        Transform nota = panel.transform.childCount > 0 ? panel.transform.GetChild(0) : null;

        grupo.alpha = 0f;
        grupo.blocksRaycasts = false;  
        panel.SetActive(true);

        float duracion = Mathf.Max(0.01f, duracionEntrada);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duracion;
            float k = Mathf.Clamp01(t);
            grupo.alpha = k;
            if (nota != null) nota.localScale = Vector3.one * Rebote(k);
            yield return null;
        }

        grupo.alpha = 1f;
        if (nota != null) nota.localScale = Vector3.one;
        grupo.blocksRaycasts = true;
    }

    static float Rebote(float k)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float e = 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
        return Mathf.LerpUnclamped(0.85f, 1f, e);
    }
}