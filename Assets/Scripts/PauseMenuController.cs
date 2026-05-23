using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public Button mainMenuButton;
    public Button restartButton;
    
    public Button keyboardButton;
    public TMP_Text keyboardButtonText;

    void Start()
    {
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        restartButton.onClick.AddListener(OnRestartClicked);
        keyboardButton.onClick.AddListener(OnKeyboardClicked);

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePauseMenu();
        }
    }

    void TogglePauseMenu()
    {
        bool isActive = pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(!isActive);
        Time.timeScale = isActive ? 1f : 0f;
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void OnKeyboardClicked()
    {
        // TODO: Toggle
    }
}
