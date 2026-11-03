using System.Collections;
using UnityEngine;

/// <summary>
/// Punto único para mostrar las pantallas de Gane y Pérdida.
/// La Persona 1 puede llamar desde el GameManager: UIManager.Instance.ShowWin() / ShowLose().
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Pantallas")]
    [SerializeField] private EndScreenPanel winPanel;
    [SerializeField] private EndScreenPanel losePanel;
    [Tooltip("Segundos antes de mostrar Pérdida, para que se vea la animación de muerte.")]
    [SerializeField] private float loseDelay = 1.2f;

    [Header("Pérdida automática")]
    [Tooltip("Muestra Pérdida al morir el jugador. Desactívalo cuando el GameManager llame a ShowLose().")]
    [SerializeField] private bool showLoseOnPlayerDeath = true;

    [Header("Debug (Editor y Development Build)")]
    [SerializeField] private bool debugKeys = true; // F1 = Gane, F2 = Pérdida

    private PlayerEnergy energy;
    private bool lossPending;

    public bool EndScreenVisible =>
        lossPending ||
        (winPanel != null && winPanel.gameObject.activeSelf) ||
        (losePanel != null && losePanel.gameObject.activeSelf);

    private void Awake()
    {
        Instance = this;
        if (winPanel != null) winPanel.gameObject.SetActive(false);
        if (losePanel != null) losePanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        energy = PlayerEnergy.Instance;
        if (energy != null) energy.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDestroy()
    {
        if (energy != null) energy.OnPlayerDied -= HandlePlayerDied;
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!debugKeys) return;
        if (Input.GetKeyDown(KeyCode.F1)) ShowWin();
        if (Input.GetKeyDown(KeyCode.F2)) ShowLose();
#endif
    }

    private void HandlePlayerDied()
    {
        if (showLoseOnPlayerDeath) ShowLose();
    }

    // Públicos: también se pueden enganchar al OnClick de botones de debug.
    public void ShowWin()
    {
        if (EndScreenVisible || winPanel == null) return;
        winPanel.Show();
    }

    public void ShowLose()
    {
        if (EndScreenVisible || losePanel == null) return;
        StartCoroutine(ShowLoseAfterDelay());
    }

    private IEnumerator ShowLoseAfterDelay()
    {
        lossPending = true;
        if (loseDelay > 0f) yield return new WaitForSeconds(loseDelay);
        lossPending = false;
        losePanel.Show();
    }
}
