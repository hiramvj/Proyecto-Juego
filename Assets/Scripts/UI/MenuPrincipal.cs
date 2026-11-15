using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Va en el Canvas "UI_Menu" de la escena MainMenu.
// Conecta los botones, pone la música y anima la entrada del logo y de los botones.
public class MenuPrincipal : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] Button botonJugar;
    [SerializeField] Button botonNiveles;
    [SerializeField] Button botonOpciones;
    [SerializeField] Button botonSalir;

    [Header("Animación de entrada")]
    [SerializeField] RectTransform logo;
    [Tooltip("El objeto vacío que contiene los cuatro botones.")]
    [SerializeField] RectTransform grupoBotones;
    [SerializeField] float duracionEntrada = 0.6f;

    [Header("Audio")]
    [SerializeField] AudioClip musica;

    Vector2 posicionLogo;
    float inicioFlotar;
    bool logoListo;

    void Awake()
    {
        if (botonJugar != null) botonJugar.onClick.AddListener(Jugar);
        if (botonNiveles != null) botonNiveles.onClick.AddListener(Niveles);
        if (botonOpciones != null) botonOpciones.onClick.AddListener(Opciones);
        if (botonSalir != null) botonSalir.onClick.AddListener(Salir);
    }

    void Start()
    {
        AudioManager.PonerMusica(musica);

        // Micro está en el menú de adorno: no debe responder al teclado.
        PlayerController micro = FindObjectOfType<PlayerController>();
        if (micro != null) micro.SetControlActivo(false);

        StartCoroutine(Entrada());
    }

    void Update()
    {
        // El logo flota despacito arriba y abajo.
        if (logoListo && logo != null)
        {
            float y = Mathf.Sin((Time.unscaledTime - inicioFlotar) * 1.5f) * 6f;
            logo.anchoredPosition = posicionLogo + new Vector2(0f, y);
        }
    }

    // ---------- Botones ----------
    void Jugar()
    {
        AudioManager.DetenerMusica();     // la música se va apagando durante la carga
        SceneLoader.CargarNivel(0);
    }

    void Niveles()
    {
        Debug.Log("Niveles: el selector llega en la siguiente parte.");
    }

    void Opciones()
    {
        Debug.Log("Opciones: llega más adelante.");
    }

    void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // en el editor: detiene Play
#else
        Application.Quit();                                // en el juego compilado: lo cierra
#endif
    }

    // ---------- Animación de entrada ----------
    IEnumerator Entrada()
    {
        CanvasGroup grupoLogo = Grupo(logo);
        CanvasGroup grupoBot = Grupo(grupoBotones);
        Vector2 posicionBotones = grupoBotones != null ? grupoBotones.anchoredPosition : Vector2.zero;
        if (logo != null) posicionLogo = logo.anchoredPosition;

        if (grupoLogo != null) grupoLogo.alpha = 0f;
        if (grupoBot != null)
        {
            grupoBot.alpha = 0f;
            grupoBot.blocksRaycasts = false;   // sin clics hasta que termine de entrar
        }

        yield return new WaitForSecondsRealtime(0.25f);
        float duracion = Mathf.Max(0.01f, duracionEntrada);

        // 1) El logo cae desde arriba y rebota.
        if (grupoLogo != null)
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duracion)
            {
                grupoLogo.alpha = Mathf.Clamp01(t * 2.5f);
                logo.anchoredPosition = posicionLogo + new Vector2(0f, 80f * (1f - Rebote(t)));
                yield return null;
            }
            grupoLogo.alpha = 1f;
            logo.anchoredPosition = posicionLogo;
            inicioFlotar = Time.unscaledTime;
            logoListo = true;
        }

        // 2) Los botones entran desde la izquierda.
        if (grupoBot != null)
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / (duracion * 0.8f))
            {
                float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);   // frena al llegar
                grupoBot.alpha = k;
                grupoBotones.anchoredPosition = posicionBotones + new Vector2(-90f * (1f - k), 0f);
                yield return null;
            }
            grupoBot.alpha = 1f;
            grupoBotones.anchoredPosition = posicionBotones;
            grupoBot.blocksRaycasts = true;
        }
    }

    // CanvasGroup controla la transparencia de un objeto y de todos sus hijos.
    static CanvasGroup Grupo(RectTransform objeto)
    {
        if (objeto == null) return null;
        CanvasGroup grupo = objeto.GetComponent<CanvasGroup>();
        return grupo != null ? grupo : objeto.gameObject.AddComponent<CanvasGroup>();
    }

    // Va de 0 a 1 pasándose un poquito antes de asentarse.
    static float Rebote(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        t = Mathf.Clamp01(t);
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
