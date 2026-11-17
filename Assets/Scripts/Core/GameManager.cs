using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum EstadoJuego { EnMenu, Jugando, Victoria, Derrota }

public class GameManager : MonoBehaviour
{
    public const int PuntosPorBateria = 5;

    static GameManager instancia;

    public static event Action OnVictoria;
    public static event Action OnDerrota;

    public static event Action OnPuntajeCambiado;

    public static EstadoJuego Estado { get; private set; }

    public static int NivelActual { get; private set; } = -1;

    public static int Puntaje { get; private set; }          
    public static int TuercasRecogidas { get; private set; }
    public static int TuercasTotales { get; private set; }   
    public static int BonoBateria { get; private set; }       
    public static int PuntajeTotal { get { return Puntaje + BonoBateria; } }


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


    public static bool RecogerTuerca(int puntos)
    {
        if (Estado == EstadoJuego.Victoria || Estado == EstadoJuego.Derrota) return false;

        TuercasRecogidas++;
        Puntaje += puntos;
        if (OnPuntajeCambiado != null) OnPuntajeCambiado();
        return true;
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

    void RevisarEscena()
    {
        NivelActual = SceneLoader.NivelActual;
        Estado = NivelActual >= 0 ? EstadoJuego.Jugando : EstadoJuego.EnMenu;

        Puntaje = 0;
        TuercasRecogidas = 0;
        BonoBateria = 0;
        TuercasTotales = FindObjectsOfType<Tuerca>().Length;
        if (OnPuntajeCambiado != null) OnPuntajeCambiado();
    }

    void Ganar()
    {
        if (Estado != EstadoJuego.Jugando) return;   // ya ganaste o perdiste: no cuenta dos veces
        Estado = EstadoJuego.Victoria;

        PlayerEnergy energia = PlayerEnergy.Instance;
        BonoBateria = energia != null ? energia.CurrentEnergy * PuntosPorBateria : 0;

        if (OnVictoria != null) OnVictoria();
    }

    void Perder()
    {
        if (Estado != EstadoJuego.Jugando) return;
        Estado = EstadoJuego.Derrota;
        if (OnDerrota != null) OnDerrota();
    }
}