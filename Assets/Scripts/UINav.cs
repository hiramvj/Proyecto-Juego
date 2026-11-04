using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Navegación simple para los botones de la UI. Siempre restablece el tiempo
/// (los menús lo ponen en 0 al pausar).
/// Si la Persona 1 termina el SceneLoader, se puede cambiar el contenido de estos métodos.
/// </summary>
public static class UINav
{
    public const string MainMenuScene = "MainMenu";

    public static void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuScene);
    }

    public static void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public static void Quit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
