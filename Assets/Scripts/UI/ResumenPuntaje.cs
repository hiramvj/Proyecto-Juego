using System.Collections;
using UnityEngine;

public class ResumenPuntaje : MonoBehaviour
{
    [Tooltip("Puntos ganados con tuercas.")]
    [SerializeField] NumeroUI numTuercas;
    [Tooltip("Tuercas recogidas y totales, por ejemplo 3/5.")]
    [SerializeField] NumeroUI numConteo;
    [Tooltip("Bono por la batería que quedó.")]
    [SerializeField] NumeroUI numBateria;
    [SerializeField] NumeroUI numTotal;

    [Header("Animación")]
    [Tooltip("Segundos que espera antes de empezar a contar.")]
    [SerializeField] float retraso = 0.45f;
    [Tooltip("Segundos que tarda en llegar al valor final.")]
    [SerializeField] float duracion = 0.9f;

    void OnEnable()
    {
        StartCoroutine(Contar());
    }

    IEnumerator Contar()
    {
        int tuercas = GameManager.Puntaje;
        int bono = GameManager.BonoBateria;
        int total = GameManager.PuntajeTotal;

        Poner(0, 0, 0);
        if (numConteo != null) numConteo.Mostrar(GameManager.TuercasRecogidas + "/" + GameManager.TuercasTotales);
        if (numTotal != null) numTotal.transform.localScale = Vector3.one;

        if (retraso > 0f) yield return new WaitForSecondsRealtime(retraso);

        float d = Mathf.Max(0.01f, duracion);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / d;
            float k = Mathf.Clamp01(t);
            k = 1f - (1f - k) * (1f - k);          // arranca rápido y frena al final
            Poner(Mathf.RoundToInt(tuercas * k), Mathf.RoundToInt(bono * k), Mathf.RoundToInt(total * k));
            yield return null;
        }
        Poner(tuercas, bono, total);

        if (numTotal != null)
        {
            t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.25f;
                float k = 1f - Mathf.Clamp01(t);
                numTotal.transform.localScale = Vector3.one * (1f + 0.22f * k * k);
                yield return null;
            }
            numTotal.transform.localScale = Vector3.one;
        }
    }

    void Poner(int tuercas, int bono, int total)
    {
        if (numTuercas != null) numTuercas.Mostrar(tuercas);
        if (numBateria != null) numBateria.Mostrar(bono);
        if (numTotal != null) numTotal.Mostrar(total);
    }
}
