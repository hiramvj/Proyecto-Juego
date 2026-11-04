using TMPro;
using UnityEngine;

/// <summary>
/// Cuadro de texto único del HUD donde aparecen los prompts ("E - Empujar").
/// Pon este script en un objeto SIEMPRE ACTIVO del Canvas y arrastra a 'panel'
/// un hijo con el fondo y el TMP_Text.
/// </summary>
public class PromptDisplay : MonoBehaviour
{
    public static PromptDisplay Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text label;

    private object owner;

    private void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(object requester, string text)
    {
        owner = requester;
        if (label != null) label.text = text;
        if (panel != null) panel.SetActive(true);
    }

    // Solo oculta si quien pide ocultar es quien lo mostró (evita pisarse entre zonas).
    public void Hide(object requester)
    {
        if (owner != requester) return;
        owner = null;
        if (panel != null) panel.SetActive(false);
    }
}
