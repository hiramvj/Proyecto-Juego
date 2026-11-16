using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public const string EscenaMenu = "MainMenu";
    public const string PrefijoNivel = "Nivel";   
    const string NombrePrefab = "PantallaDeCarga";  

    static SceneLoader instancia;

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
        if (Cargando) return;

        if (!Application.CanStreamedLevelBeLoaded(nombre))
        {
            Debug.LogError("SceneLoader: la escena '" + nombre + "' no está en Build Settings. " +
                           "Agrégala en File > Build Settings.");
            return;
        }

        SceneLoader cargador = ObtenerInstancia();
        if (cargador == null)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nombre);
            return;
        }

        cargador.StartCoroutine(cargador.Rutina(nombre, tipo));
    }


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

        GameObject objeto = Instantiate(prefab); 
        objeto.name = NombrePrefab;
        if (instancia == null)
            Debug.LogError("SceneLoader: el prefab " + NombrePrefab + " no tiene el script SceneLoader en su raíz.");
        return instancia;
    }

    void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);  

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


        if (mensaje != null)
        {
            Sprite s = SpriteDe(tipo);
            if (s != null) mensaje.sprite = s;
        }
        MoverLapiz(0f);
        canvas.enabled = true;
        grupo.blocksRaycasts = true;   
        yield return Fundido(0f, 1f);


        Time.timeScale = 1f;      
        AsyncOperation carga = SceneManager.LoadSceneAsync(escena);
        carga.allowSceneActivation = false; 

        float minimo = Mathf.Max(0.05f, tipo == TipoCarga.Reintentar ? tiempoReintentar : tiempoMinimo);
        float t = 0f;

        while (t < minimo || carga.progress < 0.9f)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / minimo);
            if (carga.progress < 0.9f) p = Mathf.Min(p, 0.9f);
            MoverLapiz(p);
            yield return null;
        }
        MoverLapiz(1f);


        carga.allowSceneActivation = true;
        while (!carga.isDone) yield return null;
        yield return null;
        yield return null;         

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
