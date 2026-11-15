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

    [Header("Animación de entrada")]
    [Tooltip("Segundos que tarda el panel en aparecer.")]
    [SerializeField] float duracionEntrada = 0.3f;

    bool mostrando;

    void Awake()
    {
        if (panelVictoria != null) panelVictoria.SetActive(false);

        if (botonSiguiente != null) botonSiguiente.onClick.AddListener(SiguienteNivel);
        if (botonReintentar != null) botonReintentar.onClick.AddListener(Reintentar);
        if (botonMenu != null) botonMenu.onClick.AddListener(IrAlMenu);
    }

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

    void SiguienteNivel() { SceneLoader.SiguienteNivel(); }

    void Reintentar() { SceneLoader.ReiniciarNivel(); }

    void IrAlMenu() { SceneLoader.CargarMenu(); }
}