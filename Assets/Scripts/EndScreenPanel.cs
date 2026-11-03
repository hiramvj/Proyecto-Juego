using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pantalla de Gane o de Pérdida (se usa el mismo script en las dos).
/// El panel puede empezar desactivado en la escena. Show() lo activa y pausa el juego.
/// </summary>
public class EndScreenPanel : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private bool wired;

    private void Wire()
    {
        if (wired) return;
        wired = true;
        if (restartButton != null) restartButton.onClick.AddListener(UINav.RestartLevel);
        if (menuButton != null) menuButton.onClick.AddListener(UINav.LoadMainMenu);
    }

    public void Show()
    {
        Wire();
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
