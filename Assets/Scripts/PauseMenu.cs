using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menú de pausa con Esc. Pon este script en un objeto SIEMPRE ACTIVO del Canvas
/// y arrastra a 'panel' un hijo con los botones (el panel empieza oculto).
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button quitButton;

    private bool paused;

    private void Start()
    {
        if (panel != null) panel.SetActive(false);

        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (restartButton != null) restartButton.onClick.AddListener(UINav.RestartLevel);
        if (menuButton != null) menuButton.onClick.AddListener(UINav.LoadMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(UINav.Quit);
    }

    private void OnDestroy()
    {
        if (paused) Time.timeScale = 1f;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (paused) Resume();
        else if (UIManager.Instance == null || !UIManager.Instance.EndScreenVisible) Pause();
    }

    public void Pause()
    {
        paused = true;
        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        paused = false;
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
    }
}
