using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumeroUI : MonoBehaviour
{
    public enum Alineacion { Izquierda, Centro, Derecha }

    [Tooltip("Alto de los números, en píxeles del Canvas.")]
    [SerializeField] float altura = 36f;
    [Tooltip("Hacia dónde crece el número a partir de la posición de este objeto.")]
    [SerializeField] Alineacion alineacion = Alineacion.Centro;
    [SerializeField] Color color = new Color32(46, 35, 28, 255);
    [Tooltip("Píxeles extra entre un carácter y el siguiente (puede ser negativo).")]
    [SerializeField] float espaciado = 0f;
    [Tooltip("Lo que muestra al empezar.")]
    [SerializeField] string textoInicial = "0";

    const string Carpeta = "Numeros/";
    const int Barra = 10, Mas = 11;
    const float AlturaPequena = 26f;     

    static Sprite[] grandes, pequenos;

    readonly List<Image> imagenes = new List<Image>();
    string textoActual;

    void Awake()
    {
        if (textoActual == null) Mostrar(textoInicial);
    }

    public void Mostrar(int numero)
    {
        Mostrar(numero.ToString());
    }

    public void Mostrar(string texto)
    {
        if (texto == null) texto = "";
        if (texto == textoActual) return;
        textoActual = texto;

        CargarSprites();
        Sprite[] juego = altura < AlturaPequena ? pequenos : grandes;
        if (juego[0] == null) return;                   

        float escala = altura / juego[0].rect.height;

        float total = 0f;
        for (int i = 0; i < texto.Length; i++)
        {
            total += Ancho(juego, texto[i], escala);
            if (i > 0) total += espaciado;
        }

        float x = 0f;
        if (alineacion == Alineacion.Centro) x = -total / 2f;
        else if (alineacion == Alineacion.Derecha) x = -total;

        for (int i = 0; i < texto.Length; i++)
        {
            Sprite s = Glifo(juego, texto[i]);
            float ancho = Ancho(juego, texto[i], escala);
            Image imagen = ObtenerImagen(i);

            imagen.gameObject.SetActive(s != null);         
            if (s != null)
            {
                imagen.sprite = s;
                imagen.color = color;
                imagen.rectTransform.sizeDelta = new Vector2(ancho, altura);
                imagen.rectTransform.anchoredPosition = new Vector2(x + ancho / 2f, 0f);
            }

            x += ancho + espaciado;
        }

        for (int i = texto.Length; i < imagenes.Count; i++) imagenes[i].gameObject.SetActive(false);
    }


    Image ObtenerImagen(int indice)
    {
        while (imagenes.Count <= indice)
        {
            GameObject g = new GameObject("Digito", typeof(RectTransform));
            g.layer = gameObject.layer;
            g.transform.SetParent(transform, false);

            Image imagen = g.AddComponent<Image>();
            imagen.raycastTarget = false;                   

            RectTransform rt = imagen.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);

            imagenes.Add(imagen);
        }
        return imagenes[indice];
    }

    static Sprite Glifo(Sprite[] juego, char c)
    {
        if (c >= '0' && c <= '9') return juego[c - '0'];
        if (c == '/') return juego[Barra];
        if (c == '+') return juego[Mas];
        return null;
    }

    static float Ancho(Sprite[] juego, char c, float escala)
    {
        Sprite s = Glifo(juego, c);
        if (s == null) s = juego[0];                      
        return s.rect.width * escala;
    }

    static void CargarSprites()
    {
        if (grandes != null) return;                      

        grandes = CargarJuego("num_");
        pequenos = CargarJuego("num_p_");

        if (grandes[0] == null)
        {
            Debug.LogError("NumeroUI: no encontré los números. Deben estar en Assets/Resources/Numeros (num_0, num_1, ...).");
        }
        if (pequenos[0] == null) pequenos = grandes;      
    }

    static Sprite[] CargarJuego(string prefijo)
    {
        Sprite[] juego = new Sprite[12];
        for (int i = 0; i < 10; i++) juego[i] = Resources.Load<Sprite>(Carpeta + prefijo + i);
        juego[Barra] = Resources.Load<Sprite>(Carpeta + prefijo + "barra");
        juego[Mas] = Resources.Load<Sprite>(Carpeta + prefijo + "mas");
        return juego;
    }
}