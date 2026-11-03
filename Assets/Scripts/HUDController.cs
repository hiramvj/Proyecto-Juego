using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD: barra de batería (esquina superior izquierda) e indicador de modo (superior derecha).
/// Va en un objeto activo del Canvas del HUD.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Batería")]
    [Tooltip("Image con Image Type = Filled, Fill Method = Horizontal.")]
    [SerializeField] private Image batteryFill;
    [SerializeField] private TMP_Text batteryText;
    [SerializeField] private Color fullColor = new Color(0.35f, 0.9f, 0.4f);
    [SerializeField] private Color lowColor = new Color(0.95f, 0.25f, 0.2f);
    [SerializeField, Range(0f, 1f)] private float lowThreshold = 0.3f;

    [Header("Indicador de modo")]
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private Image modeIcon;       // opcional
    [SerializeField] private Sprite microIcon;     // opcional
    [SerializeField] private Sprite grandeIcon;    // opcional

    private PlayerEnergy energy;

    private void OnEnable()
    {
        // PlayerController.OnSizeModeChanged es un evento estático.
        PlayerController.OnSizeModeChanged += HandleModeChanged;
    }

    private void OnDisable()
    {
        PlayerController.OnSizeModeChanged -= HandleModeChanged;
    }

    private void Start()
    {
        energy = PlayerEnergy.Instance;
        if (energy != null)
        {
            energy.OnEnergyChanged += HandleEnergyChanged;
            HandleEnergyChanged(energy.CurrentEnergy, energy.MaxEnergy);
        }
    }

    private void OnDestroy()
    {
        if (energy != null) energy.OnEnergyChanged -= HandleEnergyChanged;
    }

    private void HandleEnergyChanged(int current, int max)
    {
        float t = max > 0 ? (float)current / max : 0f;

        if (batteryFill != null)
        {
            batteryFill.fillAmount = t;
            batteryFill.color = t <= lowThreshold ? lowColor : fullColor;
        }
        if (batteryText != null) batteryText.text = Mathf.RoundToInt(t * 100f) + "%";
    }

    private void HandleModeChanged(SizeMode mode)
    {
        bool micro = mode == SizeMode.Micro;

        if (modeText != null) modeText.text = micro ? "MODO MICRO" : "MODO GRANDE";
        if (modeIcon != null)
        {
            Sprite s = micro ? microIcon : grandeIcon;
            if (s != null) modeIcon.sprite = s;
        }
    }
}
