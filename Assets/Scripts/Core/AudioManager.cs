using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Música y sonidos de todo el juego.
//
// Vive en el prefab Assets/Resources/AudioManager. NO se pone en ninguna escena:
// se crea solo al arrancar el juego y ya no se destruye.
//
// - Botones: a todos los botones de cada escena les agrega BotonEfectos
//   (sonido al pasar el mouse y al hacer clic). No hay que configurar nada en cada botón.
// - Música: la escena que quiera música llama a AudioManager.PonerMusica(clip).
//   Si una escena no pide música, la que venía sonando se apaga sola.
public class AudioManager : MonoBehaviour
{
    const string NombrePrefab = "AudioManager";      // dentro de Assets/Resources
    const string ClaveMusica = "VolumenMusica";
    const string ClaveEfectos = "VolumenEfectos";

    static AudioManager instancia;

    [Header("Sonidos de botones")]
    [SerializeField] AudioClip sonidoClic;
    [SerializeField] AudioClip sonidoHover;

    [Header("Volumen (0 a 1)")]
    [Range(0f, 1f)] [SerializeField] float volumenMusica = 0.6f;
    [Range(0f, 1f)] [SerializeField] float volumenEfectos = 0.9f;

    [Header("Música")]
    [Tooltip("Segundos que tarda la música en aparecer o apagarse.")]
    [SerializeField] float duracionFundido = 0.8f;

    AudioSource fuenteMusica;
    AudioSource fuenteEfectos;
    AudioClip musicaPedida;        // la última música que se pidió (null = silencio)
    bool pedidaEnEstaEscena;
    Coroutine cambio;

    // =====================================================================
    //  Lo que usan los demás scripts
    // =====================================================================

    public static void PonerMusica(AudioClip clip)
    {
        AudioManager a = ObtenerInstancia();
        a.pedidaEnEstaEscena = true;
        if (clip == a.musicaPedida) return;   // ya está sonando esa
        a.CambiarA(clip);
    }

    public static void DetenerMusica()
    {
        AudioManager a = ObtenerInstancia();
        if (a.musicaPedida == null) return;
        a.CambiarA(null);
    }

    // Suena un efecto una vez. 'volumen' multiplica al volumen general de efectos.
    public static void Sonar(AudioClip clip, float volumen = 1f)
    {
        if (clip == null) return;
        AudioManager a = ObtenerInstancia();
        a.fuenteEfectos.PlayOneShot(clip, a.volumenEfectos * volumen);
    }

    public static void SonarClic()
    {
        Sonar(ObtenerInstancia().sonidoClic);
    }

    public static void SonarHover()
    {
        Sonar(ObtenerInstancia().sonidoHover, 0.7f);
    }

    // Para el menú de Opciones (más adelante). Se guardan entre partidas.
    public static float VolumenMusica
    {
        get { return ObtenerInstancia().volumenMusica; }
        set
        {
            AudioManager a = ObtenerInstancia();
            a.volumenMusica = Mathf.Clamp01(value);
            if (a.cambio == null) a.fuenteMusica.volume = a.volumenMusica;
            PlayerPrefs.SetFloat(ClaveMusica, a.volumenMusica);
        }
    }

    public static float VolumenEfectos
    {
        get { return ObtenerInstancia().volumenEfectos; }
        set
        {
            AudioManager a = ObtenerInstancia();
            a.volumenEfectos = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(ClaveEfectos, a.volumenEfectos);
        }
    }

    // =====================================================================
    //  Funcionamiento interno
    // =====================================================================

    // Unity llama a esto solo, una vez, cuando termina de cargar la primera escena.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Arrancar()
    {
        ObtenerInstancia();
    }

    static AudioManager ObtenerInstancia()
    {
        if (instancia != null) return instancia;

        GameObject prefab = Resources.Load<GameObject>(NombrePrefab);
        if (prefab != null)
        {
            GameObject objeto = Instantiate(prefab);   // su Awake lo registra como instancia
            objeto.name = NombrePrefab;
        }

        if (instancia == null)
        {
            Debug.LogWarning("AudioManager: no encontré el prefab Assets/Resources/" + NombrePrefab +
                             " (o no tiene el script). Los botones no van a sonar.");
            new GameObject(NombrePrefab).AddComponent<AudioManager>();   // uno vacío, para que nada falle
        }
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
        DontDestroyOnLoad(gameObject);

        volumenMusica = PlayerPrefs.GetFloat(ClaveMusica, volumenMusica);
        volumenEfectos = PlayerPrefs.GetFloat(ClaveEfectos, volumenEfectos);

        // Dos "parlantes": uno para la música (en bucle) y otro para los efectos.
        fuenteMusica = gameObject.AddComponent<AudioSource>();
        fuenteMusica.loop = true;
        fuenteMusica.playOnAwake = false;
        fuenteMusica.volume = 0f;

        fuenteEfectos = gameObject.AddComponent<AudioSource>();
        fuenteEfectos.playOnAwake = false;

        SceneManager.sceneLoaded += AlCargarEscena;
        PrepararBotones();
    }

    void OnDestroy()
    {
        if (instancia != this) return;
        SceneManager.sceneLoaded -= AlCargarEscena;
        instancia = null;
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        PrepararBotones();
        StartCoroutine(RevisarMusica());
    }

    // Da dos cuadros para que la escena nueva pida su música; si nadie pide, silencio.
    IEnumerator RevisarMusica()
    {
        pedidaEnEstaEscena = false;
        yield return null;
        yield return null;
        if (!pedidaEnEstaEscena) DetenerMusica();
    }

    void PrepararBotones()
    {
        // true = incluye los botones que están desactivados (paneles ocultos)
        foreach (Button boton in FindObjectsOfType<Button>(true))
        {
            if (boton.GetComponent<BotonEfectos>() == null)
                boton.gameObject.AddComponent<BotonEfectos>();
        }
    }

    void CambiarA(AudioClip clip)
    {
        musicaPedida = clip;
        if (cambio != null) StopCoroutine(cambio);
        cambio = StartCoroutine(Fundir(clip));
    }

    // Baja la música que suena y, si hay una nueva, la sube poco a poco.
    IEnumerator Fundir(AudioClip nueva)
    {
        float duracion = Mathf.Max(0.01f, duracionFundido);

        if (fuenteMusica.isPlaying)
        {
            float desde = fuenteMusica.volume;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duracion)
            {
                fuenteMusica.volume = Mathf.Lerp(desde, 0f, t);
                yield return null;
            }
            fuenteMusica.Stop();
        }

        if (nueva != null)
        {
            fuenteMusica.clip = nueva;
            fuenteMusica.volume = 0f;
            fuenteMusica.Play();
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duracion)
            {
                fuenteMusica.volume = Mathf.Lerp(0f, volumenMusica, t);
                yield return null;
            }
            fuenteMusica.volume = volumenMusica;
        }

        cambio = null;
    }
}
