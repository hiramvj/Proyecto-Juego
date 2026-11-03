using UnityEngine;

/// <summary>
/// Zona genérica de prompt. La Persona 3 la agrega a sus prefabs:
/// un hijo con Collider2D (Is Trigger) + este script, y escribe el texto en el Inspector.
/// Ejemplos: "E - Empujar" (SoloGrande), "Q - Cambiar a Modo Micro" (SoloGrande).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class InteractionZone : MonoBehaviour
{
    public enum RequiredMode { Cualquiera, SoloMicro, SoloGrande }

    [SerializeField] private string promptText = "E - Interactuar";
    [Tooltip("En qué modo del jugador se muestra el prompt.")]
    [SerializeField] private RequiredMode requiredMode = RequiredMode.Cualquiera;

    private PlayerController player;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnEnable()
    {
        PlayerController.OnSizeModeChanged += HandleModeChanged;
    }

    private void OnDisable()
    {
        PlayerController.OnSizeModeChanged -= HandleModeChanged;
        Hide();
        player = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController p = other.GetComponentInParent<PlayerController>();
        if (p == null) return;

        player = p;
        Refresh();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController p = other.GetComponentInParent<PlayerController>();
        if (p == null || p != player) return;

        player = null;
        Hide();
    }

    private void HandleModeChanged(SizeMode mode)
    {
        if (player != null) Refresh();
    }

    private void Refresh()
    {
        bool visible = true;
        if (requiredMode == RequiredMode.SoloMicro) visible = player.CurrentMode == SizeMode.Micro;
        else if (requiredMode == RequiredMode.SoloGrande) visible = player.CurrentMode == SizeMode.Grande;

        if (visible) Show();
        else Hide();
    }

    private void Show()
    {
        if (PromptDisplay.Instance != null) PromptDisplay.Instance.Show(this, promptText);
    }

    private void Hide()
    {
        if (PromptDisplay.Instance != null) PromptDisplay.Instance.Hide(this);
    }
}
