using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class HUDJuego : MonoBehaviour
{
    [Header("Batería")]
    [Tooltip("El objeto 'Bateria' completo. Es lo que se sacude al recibir daño.")]
    [SerializeField] RectTransform bateria;
    [Tooltip("La imagen 'Relleno'. El script la pone en Image Type = Filled.")]
    [SerializeField] Image relleno;
    [SerializeField] Color colorAlta = new Color32(86, 208, 182, 255);
    [SerializeField] Color colorMedia = new Color32(246, 196, 84, 255);
    [SerializeField] Color colorBaja = new Color32(236, 96, 108, 255);
    [Tooltip("Con esta proporción o menos, la barra se pone amarilla.")]
    [Range(0f, 1f)] [SerializeField] float umbralMedia = 0.5f;
    [Tooltip("Con esta proporción o menos, la barra se pone roja y parpadea.")]
    [Range(0f, 1f)] [SerializeField] float umbralBaja = 0.25f;
    [Tooltip("Qué tan rápido baja o sube la barra (1 = toda la barra en un segundo).")]
    [SerializeField] float velocidadBarra = 1.6f;

    [Header("Indicador de modo")]
    [SerializeField] Image modo;
    [SerializeField] Sprite spriteMicro;
    [SerializeField] Sprite spriteGrande;

    [Header("Daño")]
    [Tooltip("Imagen roja a pantalla completa que destella al recibir daño (opcional).")]
    [SerializeField] Image destelloDano;
    [SerializeField] AudioClip sonidoDano;
    [Tooltip("Cuántos píxeles se mueve la batería al sacudirse.")]
    [SerializeField] float fuerzaSacudida = 10f;

    [Header("Aparecer y desaparecer")]
    [SerializeField] float duracionFundido = 0.35f;

    CanvasGroup grupo;
    PlayerEnergy energia;
    Vector2 posBateria;
    float proporcion = 1f;     
    float objetivo = 1f;     
    int ultimaEnergia = -1;
    bool listo;                

    Coroutine rutinaFundido, rutinaSacudida, rutinaDestello, rutinaReboteBateria, rutinaReboteModo;

    void Awake()
    {
        grupo = GetComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.interactable = false;
        grupo.blocksRaycasts = false;   

        if (relleno != null)
        {
            relleno.type = Image.Type.Filled;
            relleno.fillMethod = Image.FillMethod.Horizontal;
            relleno.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        if (bateria != null) posBateria = bateria.anchoredPosition;

        if (destelloDano != null)
        {
            destelloDano.raycastTarget = false;
            PonerAlfa(destelloDano, 0f);
        }
    }

    void OnEnable()
    {
        PlayerController.OnSizeModeChanged += AlCambiarModo;
        GameManager.OnVictoria += AlGanar;
        GameManager.OnDerrota += AlPerder;
    }

    void OnDisable()
    {
        PlayerController.OnSizeModeChanged -= AlCambiarModo;
        GameManager.OnVictoria -= AlGanar;
        GameManager.OnDerrota -= AlPerder;
    }

    IEnumerator Start()
    {
        energia = PlayerEnergy.Instance;
        if (energia != null)
        {
            energia.OnEnergyChanged += AlCambiarEnergia;
            ultimaEnergia = energia.CurrentEnergy;
            objetivo = proporcion = Proporcion(energia.CurrentEnergy, energia.MaxEnergy);

            PlayerController micro = energia.GetComponent<PlayerController>();
            if (micro != null) MostrarModo(micro.CurrentMode, false);
        }
        else
        {
            Debug.LogWarning("HUDJuego: no encontré PlayerEnergy en la escena. ¿Micro_Prefab tiene el componente Player Energy?");
        }

        PintarBateria();
        IniciarFundido(1f, 0f);

        yield return null; 
        listo = true;
    }

    void OnDestroy()
    {
        if (energia != null) energia.OnEnergyChanged -= AlCambiarEnergia;
    }

    void Update()
    {
        proporcion = Mathf.MoveTowards(proporcion, objetivo, velocidadBarra * Time.unscaledDeltaTime);
        PintarBateria();
    }



    void AlCambiarEnergia(int actual, int maximo)
    {
        objetivo = Proporcion(actual, maximo);

        if (!listo || ultimaEnergia < 0)
        {
            proporcion = objetivo;                       // valor inicial: sin animación
        }
        else if (actual < ultimaEnergia)                 // daño
        {
            if (bateria != null) Reiniciar(ref rutinaSacudida, Sacudir());
            if (destelloDano != null) Reiniciar(ref rutinaDestello, Destello());
            if (actual > 0) AudioManager.Sonar(sonidoDano);   // en 0 ya suena la derrota
        }
        else if (actual > ultimaEnergia)                 // recarga
        {
            if (bateria != null) Reiniciar(ref rutinaReboteBateria, Rebote(bateria));
        }

        ultimaEnergia = actual;
    }

    void PintarBateria()
    {
        if (relleno == null) return;

        relleno.fillAmount = proporcion;

        Color c = colorAlta;
        if (objetivo <= umbralBaja) c = colorBaja;
        else if (objetivo <= umbralMedia) c = colorMedia;

        // Batería baja: parpadea para llamar la atención.
        if (objetivo <= umbralBaja && objetivo > 0f)
        {
            float pulso = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 9f);
            c = Color.Lerp(c, Color.white, 0.4f * pulso);
        }

        relleno.color = c;
    }

    static float Proporcion(int actual, int maximo)
    {
        return maximo > 0 ? Mathf.Clamp01((float)actual / maximo) : 0f;
    }

    IEnumerator Sacudir()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.35f;
            float fuerza = fuerzaSacudida * (1f - Mathf.Clamp01(t));
            bateria.anchoredPosition = posBateria + Random.insideUnitCircle * fuerza;
            yield return null;
        }
        bateria.anchoredPosition = posBateria;
    }

    IEnumerator Destello()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.5f;
            float k = 1f - Mathf.Clamp01(t);
            PonerAlfa(destelloDano, 0.85f * k * k);
            yield return null;
        }
        PonerAlfa(destelloDano, 0f);
    }

    void AlCambiarModo(SizeMode nuevo)
    {
        MostrarModo(nuevo, listo);
    }

    void MostrarModo(SizeMode cual, bool animar)
    {
        if (modo == null) return;

        Sprite s = cual == SizeMode.Micro ? spriteMicro : spriteGrande;
        if (s != null) modo.sprite = s;

        if (animar) Reiniciar(ref rutinaReboteModo, Rebote(modo.rectTransform));
    }

    IEnumerator Rebote(Transform objeto)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.25f;
            float k = 1f - Mathf.Clamp01(t);
            objeto.localScale = Vector3.one * (1f + 0.16f * k * k);
            yield return null;
        }
        objeto.localScale = Vector3.one;
    }


    void AlGanar()
    {
        IniciarFundido(0f, 0f);
    }

    void AlPerder()
    {
        IniciarFundido(0f, 0.7f);    
    }

    void IniciarFundido(float destino, float retraso)
    {
        Reiniciar(ref rutinaFundido, Fundido(destino, retraso));
    }

    IEnumerator Fundido(float destino, float retraso)
    {
        if (retraso > 0f) yield return new WaitForSecondsRealtime(retraso);

        float inicio = grupo.alpha;
        float duracion = Mathf.Max(0.01f, duracionFundido);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duracion;
            grupo.alpha = Mathf.Lerp(inicio, destino, Mathf.Clamp01(t));
            yield return null;
        }
        grupo.alpha = destino;
    }


    void Reiniciar(ref Coroutine rutina, IEnumerator nueva)
    {
        if (rutina != null) StopCoroutine(rutina);
        rutina = StartCoroutine(nueva);
    }

    static void PonerAlfa(Image imagen, float alfa)
    {
        Color c = imagen.color;
        c.a = alfa;
        imagen.color = c;
    }
}
