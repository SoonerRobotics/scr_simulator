using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    private VisualElement ui;

    public DropdownField mapDropdown;
    public Button startButton;

    void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;
    }

    void OnEnable()
    {
        mapDropdown = ui.Q<DropdownField>("MapDropdown");
        startButton = ui.Q<Button>("StartButton");

        startButton.clicked += StartGame;
    }

    void OnDisable()
    {
        startButton.clicked -= StartGame;
    }

    private void StartGame()
    {
        string selectedMap = mapDropdown.value;
        Debug.Log("Starting game with map: " + selectedMap);

        switch (selectedMap)
        {
            case "[IGVC 2026] AutoNav":
                UnityEngine.SceneManagement.SceneManager.LoadScene("IGVC_2026_AutoNav");
                break;
            case "[IGVC 2026] Self Drive":
                UnityEngine.SceneManagement.SceneManager.LoadScene("IGVC_2026_SelfDrive");
                break;
            default:
                Debug.LogError("Unknown map selected: " + selectedMap);
                break;
        }
    }

    void Update()
    {
        
    }
}
