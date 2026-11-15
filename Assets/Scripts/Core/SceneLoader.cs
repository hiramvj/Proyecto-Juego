using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Qué mensaje muestra la pantalla de carga.
public enum TipoCarga { Cargando, SiguienteNivel, Reintentar, Menu }

// Cambia de escena mostrando la pantalla de carga.
//
// Vive en el prefab Assets/Resources/PantallaDeCarga. NO se pone en ninguna escena:
// se crea solo la primera vez que alguien pide cambiar de escena y ya no se destruye.
//
// Se usa desde cualquier script, sin referencias en el Inspector:
//   SceneLoader.SiguienteNivel();
//   SceneLoader.ReiniciarNivel();
//   SceneLoader.CargarMenu();
//   SceneLoader.CargarNivel(1);
[RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
public class SceneLoader : MonoBehaviour
{
    // ---------- Convención de nombres de escena ----------
    public const string EscenaMenu = "MainMenu";
    public const string PrefijoNivel = "Nivel";      // Nivel0, Nivel1, Nivel2...
    const string NombrePrefab = "PantallaDeCarga";   // dentro de Assets/Resources

    static SceneLoader instancia;

    // true mientras hay un cambio de escena en curso.
    public static bool Cargando { get; private set; }

    [Header("Piezas de la pantalla")]
    [SerializeField] Image mensaje;
    [SerializeField] Image linea;
    [SerializeField] RectTransform lapiz;

    [Header("Mensajes")]
    [SerializeField] Sprite msgCargando;
    [SerializeField] Sprite msgSiguiente;
    [SerializeField] Sprite msgReintentar;
    [SerializeField] Sprite msgMenu;

    [Header("Tiempos (segundos)")]
    [SerializeField] float duracionFundido = 0.35f;
    [Tooltip("Tiempo mínimo que se ve la pantalla, para que dé tiempo de leerla.")]
    [SerializeField] float tiempoMinimo = 1.4f;
    [Tooltip("Tiempo mínimo al reintentar: más corto para no hacer esperar.")]
    [SerializeField] float tiempoReintentar = 0.7f;

    Canvas canvas;
    CanvasGroup grupo;

    // =====================================================================
    //  Lo que usan los demás scripts
    // =====================================================================

    // Número del nivel abierto (0 para "Nivel0"). Devuelve -1 si la escena no es un nivel.
    public static int NivelActual
    {
        get
        {
            string nombre = SceneManager.GetActiveScene().name;
            int numero;
            if (nombre.StartsWith(PrefijoNivel) &&
                int.TryParse(nombre.Substring(PrefijoNivel.Length), out numero))
                return numero;
            return -1;
        }
    }

    // true si existe la escena del nivel que sigue y está en Build Settings.
    public static bool HaySiguienteNivel
    {
        get
        {
            return NivelActual >= 0 &&
                   Application.CanStreamedLevelBeLoaded(PrefijoNivel + (NivelActual + 1));
        }
    }

    public static void SiguienteNivel()
    {
        if (NivelActual < 0)
            Debug.LogWarning("SceneLoader: la escena '" + SceneManager.GetActiveScene().name +
                             "' no se llama Nivel0, Nivel1... y no sé cuál sigue.");

        if (HaySiguienteNivel)
        {
            CargarEscena(PrefijoNivel + (NivelActual + 1), TipoCarga.SiguienteNivel);
        }
        else
        {
            Debug.Log("SceneLoader: no hay más niveles, vuelvo al menú.");
            CargarMenu();
        }
    }

    public static void ReiniciarNivel()
    {
        CargarEscena(SceneManager.GetActiveScene().name, TipoCarga.Reintentar);
    }

    public static void CargarMenu()
    {
        CargarEscena(EscenaMenu, TipoCarga.Menu);
    }

    public static void CargarNivel(int numero)
    {
        CargarEscena(PrefijoNivel + numero, TipoCarga.Cargando);
    }

    public static void CargarEscena(string nombre, TipoCarga tipo = TipoCarga.Cargando)
    {
        if (Cargando) return;   // ya hay un cambio en curso: se ignoran los clics repetidos

        if (!Application.CanStreamedLevelBeLoaded(nombre))
        {
            Debug.LogError("SceneLoader: la escena '" + nombre + "' no está en Build Settings. " +
                           "Agrégala en File > Build Settings.");
            return;
        }

        SceneLoader cargador = ObtenerInstancia();
        if (cargador == null)
        {
            // Sin prefab de pantalla de carga: se cambia de escena directo.
            Time.timeScale = 1f;
            SceneManager.LoadScene(nombre);
            return;
        }

        cargador.StartCoroutine(cargador.Rutina(nombre, tipo));
    }

    // =====================================================================
    //  Funcionamiento interno
    // =====================================================================

    static SceneLoader ObtenerInstancia()
    {
        if (instancia != null) return instancia;

        GameObject prefab = Resources.Load<GameObject>(NombrePrefab);
        if (prefab == null)
        {
            Debug.LogWarning("SceneLoader: no encontré el prefab Assets/Resources/" + NombrePrefab +
                             ". Cambio de escena sin pantalla de carga.");
            return null;
        }

        GameObject objeto = Instantiate(prefab);   // su Awake lo registra como instancia
        objeto.name = NombrePrefab;
        if (instancia == null)
            Debug.LogError("SceneLoader: el prefab " + NombrePrefab + " no tiene el script SceneLoader en su raíz.");
        return instancia;
    }

    void Awake()
    {
        // Solo puede existir uno.
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);   // sobrevive a los cambios de escena

        canvas = GetComponent<Canvas>();
        grupo = GetComponent<CanvasGroup>();
        Ocultar();
    }

    void Ocultar()
    {
        grupo.alpha = 0f;
        grupo.blocksRaycasts = false;
        canvas.enabled = false;
    }

    IEnumerator Rutina(string escena, TipoCarga tipo)
    {
        Cargando = true;

        // 1) Aparece la pantalla de carga encima de todo.
        if (mensaje != null)
        {
            Sprite s = SpriteDe(tipo);
            if (s != null) mensaje.sprite = s;
        }
        MoverLapiz(0f);
        canvas.enabled = true;
        grupo.blocksRaycasts = true;     // nadie puede hacer clic en lo de atrás
        yield return Fundido(0f, 1f);

        // 2) Con la pantalla ya tapada, se carga la escena nueva.
        Time.timeScale = 1f;             // por si el juego estaba en pausa
        AsyncOperation carga = SceneManager.LoadSceneAsync(escena);
        carga.allowSceneActivation = false;   // todavía no la muestres

        float minimo = Mathf.Max(0.05f, tipo == TipoCarga.Reintentar ? tiempoReintentar : tiempoMinimo);
        float t = 0f;
        // Unity deja 'progress' en 0.9 cuando la escena está lista para activarse.
        while (t < minimo || carga.progress < 0.9f)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / minimo);
            if (carga.progress < 0.9f) p = Mathf.Min(p, 0.9f);
            MoverLapiz(p);
            yield return null;
        }
        MoverLapiz(1f);

        // 3) Se activa la escena nueva y desaparece la pantalla.
        carga.allowSceneActivation = true;
        while (!carga.isDone) yield return null;
        yield return null;
        yield return null;               // dos cuadros para que la escena termine de arrancar

        yield return Fundido(1f, 0f);
        Ocultar();
        Cargando = false;
    }

    Sprite SpriteDe(TipoCarga tipo)
    {
        switch (tipo)
        {
            case TipoCarga.SiguienteNivel: return msgSiguiente;
            case TipoCarga.Reintentar: return msgReintentar;
            case TipoCarga.Menu: return msgMenu;
            default: return msgCargando;
        }
    }

    // p va de 0 a 1: la línea se dibuja y el lápiz la sigue por la punta.
    void MoverLapiz(float p)
    {
        if (linea == null) return;
        linea.fillAmount = p;

        if (lapiz == null) return;
        RectTransform r = linea.rectTransform;
        float inicio = r.anchoredPosition.x - r.rect.width * r.pivot.x;
        float temblor = Mathf.Sin(Time.unscaledTime * 24f) * 2f;
        lapiz.anchoredPosition = new Vector2(inicio + r.rect.width * p, r.anchoredPosition.y + temblor);
        lapiz.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.unscaledTime * 12f) * 3f);
    }

    IEnumerator Fundido(float desde, float hasta)
    {
        float duracion = Mathf.Max(0.01f, duracionFundido);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duracion;
            grupo.alpha = Mathf.Lerp(desde, hasta, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        grupo.alpha = hasta;
    }
}
