using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Va en el Canvas "UI_FinDeNivel".
// Muestra el panel de victoria cuando Micro toca la meta y conecta sus botones.
// El panel de derrota se agrega más adelante, junto con el GameManager.
public class PantallasFinales : MonoBehaviour
{
    [Header("Victoria")]
    [SerializeField] GameObject panelVictoria;
    [SerializeField] Button botonSiguiente;
    [SerializeField] Button botonReintentar;
    [SerializeField] Button botonMenu;
    [Tooltip("Segundos entre tocar la meta y mostrar la pantalla.")]
    [SerializeField] float retrasoVictoria = 0.6f;

    [Header("Animación de entrada")]
    [Tooltip("Segundos que tarda el panel en aparecer.")]
    [SerializeField] float duracionEntrada = 0.3f;

    bool mostrando;

    void Awake()
    {
        // No importa si el panel quedó activado o desactivado en la escena:
        // al empezar el nivel siempre se oculta.
        if (panelVictoria != null) panelVictoria.SetActive(false);

        // Los botones se conectan por código: no hay que tocar su OnClick en el Inspector.
        if (botonSiguiente != null) botonSiguiente.onClick.AddListener(SiguienteNivel);
        if (botonReintentar != null) botonReintentar.onClick.AddListener(Reintentar);
        if (botonMenu != null) botonMenu.onClick.AddListener(IrAlMenu);
    }

    // Escuchar el evento solo mientras este objeto existe y está activo.
    // Si no se quita en OnDisable, al recargar la escena quedaría un oyente "fantasma".
    void OnEnable() { Meta.OnMetaAlcanzada += AlAlcanzarMeta; }
    void OnDisable() { Meta.OnMetaAlcanzada -= AlAlcanzarMeta; }

    void AlAlcanzarMeta()
    {
        if (mostrando) return;
        mostrando = true;
        StartCoroutine(MostrarPanel(panelVictoria, retrasoVictoria));
    }

    // Espera un momento y luego hace aparecer el panel con un pequeño rebote.
    IEnumerator MostrarPanel(GameObject panel, float retraso)
    {
        if (panel == null)
        {
            Debug.LogWarning("PantallasFinales: falta asignar el panel en el Inspector.");
            yield break;
        }

        if (retraso > 0f) yield return new WaitForSecondsRealtime(retraso);

        // CanvasGroup controla la transparencia de todo el panel a la vez.
        CanvasGroup grupo = panel.GetComponent<CanvasGroup>();
        if (grupo == null) grupo = panel.AddComponent<CanvasGroup>();

        // El primer hijo del panel es la nota de papel: es lo que "rebota".
        Transform nota = panel.transform.childCount > 0 ? panel.transform.GetChild(0) : null;

        grupo.alpha = 0f;
        grupo.blocksRaycasts = false;   // sin clics mientras aparece
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

    // Va de 0.85 a 1 pasándose un poquito antes de asentarse.
    static float Rebote(float k)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float e = 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
        return Mathf.LerpUnclamped(0.85f, 1f, e);
    }

    // ---------- Botones ----------
    void Reintentar()
    {
        Scene escena = SceneManager.GetActiveScene();
        if (escena.buildIndex < 0)
        {
            Debug.LogError("La escena '" + escena.name + "' no está en Build Settings. " +
                           "Ábrela y ve a File > Build Settings > Add Open Scenes.");
            return;
        }

        Time.timeScale = 1f;   // por si algo dejó el juego en pausa
        SceneManager.LoadScene(escena.buildIndex);
    }

    void SiguienteNivel()
    {
        Debug.Log("Siguiente nivel: se conecta en la parte del cambio de escenas.");
    }

    void IrAlMenu()
    {
        Debug.Log("Menú: se conecta en la parte del cambio de escenas.");
    }
}
