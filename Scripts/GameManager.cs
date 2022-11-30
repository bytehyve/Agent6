using UnityEngine;

/// <summary>
/// Manages the scenes, pause and main game components.
/// </summary>
public static class GameManager 
{
    public static int SceneIndex;
    public static bool Paused;

    public static void PauseGame() {
        Paused = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public static void ResumeGame() {
        Paused = false;
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public static void RestartScene() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneIndex);
    }
    public static void RestartGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    public static void NextScene() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneIndex + 1);
    }

    public static void OnSceneLoad(int index) {
        SceneIndex = index;
        MenuManager.instance = GameObject.FindObjectOfType<MenuManager>();

        AISoldier.ClearOnShootNearbyEvent();

        if (SceneIndex == 0) { 
            PauseGame();
        }
        else ResumeGame();
    }
}
