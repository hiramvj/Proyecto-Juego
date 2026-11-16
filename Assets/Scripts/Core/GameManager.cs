using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// En qué momento está el juego.
public enum EstadoJuego { EnMenu, Jugando, Victoria, Derrota }

public class GameManager : MonoBehaviour
{
    static GameManager instancia;

    public static event Action OnVictoria;
    public static event Action OnDerrota;

    public static EstadoJuego Estado { get; private set; }

    public static int NivelActual { get; private set; } = -1;


    public static void ReiniciarNivel()
    {
        SceneLoader.ReiniciarNivel();
    }

    public static void SiguienteNivel()
    {
        SceneLoader.SiguienteNivel();   
    }

    public static void IrAlMenu()
    {
        SceneLoader.CargarMenu();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Arrancar()
    {
        if (instancia == null) new GameObject("GameManager").AddComponent<GameManager>();
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

        Meta.OnMetaAlcanzada += Ganar;
        PlayerController.OnMuerte += Perder;
        SceneManager.sceneLoaded += AlCargarEscena;

        RevisarEscena();  
    }

    void OnDestroy()
    {
        if (instancia != this) return;

        Meta.OnMetaAlcanzada -= Ganar;
        PlayerController.OnMuerte -= Perder;
        SceneManager.sceneLoaded -= AlCargarEscena;
        instancia = null;
    }

    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        RevisarEscena();
    }

    // Cada vez que se abre una escena: anota el nivel y vuelve a empezar la partida.
    void RevisarEscena()
    {
        NivelActual = SceneLoader.NivelActual;
        Estado = NivelActual >= 0 ? EstadoJuego.Jugando : EstadoJuego.EnMenu;
    }

    void Ganar()
    {
        if (Estado != EstadoJuego.Jugando) return;   // ya ganaste o perdiste: no cuenta dos veces
        Estado = EstadoJuego.Victoria;
        if (OnVictoria != null) OnVictoria();
    }

    void Perder()
    {
        if (Estado != EstadoJuego.Jugando) return;
        Estado = EstadoJuego.Derrota;
        if (OnDerrota != null) OnDerrota();
    }
}
