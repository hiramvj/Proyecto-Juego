using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SelectorNiveles : MonoBehaviour
{
    [Header("Panel")]
    [Tooltip("PanelNiveles: el fondo oscuro. Su primer hijo debe ser la nota de papel.")]
    [SerializeField] GameObject panel;
    [SerializeField] Button botonVolver;

    [Header("Niveles")]
    [Tooltip("En orden: Element 0 carga Nivel0, Element 1 carga Nivel1, y así.")]
    [SerializeField] Button[] botonesNivel;

    [Header("Animación")]
    [SerializeField] float duracion = 0.22f;

    public bool Abierto { get; private set; }

    CanvasGroup grupo;
    Transform nota;
    Coroutine animacion;

    void Awake()
    {
        if (panel != null)
        {
            grupo = panel.GetComponent<CanvasGroup>();
            if (grupo == null) grupo = panel.AddComponent<CanvasGroup>();
            grupo.alpha = 0f;

            if (panel.transform.childCount > 0) nota = panel.transform.GetChild(0);
            panel.SetActive(false);  
        }

        if (botonVolver != null) botonVolver.onClick.AddListener(Cerrar);

        for (int i = 0; botonesNivel != null && i < botonesNivel.Length; i++)
        {
            Button boton = botonesNivel[i];
            if (boton == null) continue;

            int numero = i; 

            boton.interactable = Application.CanStreamedLevelBeLoaded(SceneLoader.PrefijoNivel + numero);
            boton.onClick.AddListener(() => Elegir(numero));
        }
    }

    void Update()
    {
        if (Abierto && Input.GetKeyDown(KeyCode.Escape)) Cerrar();
    }

    public void Abrir()
    {
        if (panel == null || Abierto || SceneLoader.Cargando) return;
        Abierto = true;
        Animar(true);
    }

    public void Cerrar()
    {
        if (panel == null || !Abierto) return;
        Abierto = false;
        Animar(false);
    }

    void Elegir(int numero)
    {
        AudioManager.DetenerMusica();  
        SceneLoader.CargarNivel(numero);
    }

    void Animar(bool abrir)
    {
        if (animacion != null) StopCoroutine(animacion);
        animacion = StartCoroutine(Rutina(abrir));
    }


    IEnumerator Rutina(bool abrir)
    {
        panel.SetActive(true);
        grupo.blocksRaycasts = abrir;    

        float inicio = grupo.alpha;
        float fin = abrir ? 1f : 0f;
        float d = Mathf.Max(0.01f, duracion);

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / d)
        {
            float k = 1f - Mathf.Pow(1f - t, 3f);   
            grupo.alpha = Mathf.Lerp(inicio, fin, k);
            if (nota != null) nota.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, grupo.alpha);
            yield return null;
        }

        grupo.alpha = fin;
        if (nota != null) nota.localScale = Vector3.one;
        if (!abrir) panel.SetActive(false);
        animacion = null;
    }
}
