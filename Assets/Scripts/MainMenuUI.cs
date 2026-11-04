using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menú principal: Iniciar y Salir. Va en el Canvas de la escena MainMenu.
/// Las escenas deben estar en Build Settings (lo hace la Persona 1).
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private string firstLevelScene = "Nivel0";

    private void Start()
    {
        Time.timeScale = 1f;
        if (playButton != null) playButton.onClick.AddListener(() => UINav.LoadScene(firstLevelScene));
        if (quitButton != null) quitButton.onClick.AddListener(UINav.Quit);
    }
}
