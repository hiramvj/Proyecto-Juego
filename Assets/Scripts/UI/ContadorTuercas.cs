using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ContadorTuercas : MonoBehaviour
{
    [Tooltip("Opcional: si se deja vacío, usa el NumeroUI que encuentre en sus hijos.")]
    [SerializeField] NumeroUI numero;

    CanvasGroup grupo;
    int ultimas = -1;
    Coroutine rutinaRebote;

    void Awake()
    {
        grupo = GetComponent<CanvasGroup>();
        if (numero == null) numero = GetComponentInChildren<NumeroUI>();
    }

    void OnEnable()
    {
        GameManager.OnPuntajeCambiado += Actualizar;
        Actualizar();
    }

    void OnDisable()
    {
        GameManager.OnPuntajeCambiado -= Actualizar;
    }

    void Start()
    {
        Actualizar();
    }

    void Actualizar()
    {
        int recogidas = GameManager.TuercasRecogidas;
        int totales = GameManager.TuercasTotales;

        grupo.alpha = totales > 0 ? 1f : 0f;
        if (numero != null) numero.Mostrar(recogidas + "/" + totales);

        if (ultimas >= 0 && recogidas > ultimas && isActiveAndEnabled)
        {
            if (rutinaRebote != null) StopCoroutine(rutinaRebote);
            rutinaRebote = StartCoroutine(Rebote());
        }
        ultimas = recogidas;
    }

    IEnumerator Rebote()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.25f;
            float k = 1f - Mathf.Clamp01(t);
            transform.localScale = Vector3.one * (1f + 0.18f * k * k);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}