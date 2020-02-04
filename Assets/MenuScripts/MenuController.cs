using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

public class MenuController : MonoBehaviour
{
    public GameObject togglePrefab;
    private static bool alreadyInit = false;

    [Header("Levels")]
    public LevelScriptableObject[] levels;
    public ToggleGroup togglesGroup;
    public RectTransform togglesParent;
    public static LevelScriptableObject activeLevel { get; private set; }

    [Header("Robots")]
    public RobotScriptableObject[] robots;
    public ToggleGroup robotsGroup;
    public RectTransform robotsParent;
    public static RobotScriptableObject activeRobot { get; private set; }
    private Dictionary<string, TMP_InputField> keyToOptionField = new Dictionary<string, TMP_InputField>();

    [Header("Options")]
    public GameObject optionPrefab;
    public RectTransform optionEntryParent;
    private List<GameObject> options = new List<GameObject>();

    public void Start()
    {
        // Load up Robot Options
        if (!alreadyInit)
        {
            RobotOptions.LoadSaved(robots);
        }
        else
        {
            FillOptionFields();
        }

        string robotNameToLoad = PlayerPrefs.GetString("lastRobot", robots[0].robotName);
        int levelIdToLoad = PlayerPrefs.GetInt("lastLevel", levels[0].levelId);

        // Generate level toggles
        foreach (LevelScriptableObject level in levels)
        {
            GameObject levelToggle = Instantiate(togglePrefab, togglesParent);

            Toggle toggle = levelToggle.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate
            {
                SelectLevel(level);
            });
            toggle.group = togglesGroup;

            if (level.levelId == levelIdToLoad)
            {
                activeLevel = level;
                toggle.isOn = true;
            }

            TextMeshProUGUI text = levelToggle.GetComponentInChildren<TextMeshProUGUI>();
            text.text = level.levelName;
        }
        // Generate robot toggles
        foreach (RobotScriptableObject robot in robots)
        {
            GameObject robotToggle = Instantiate(togglePrefab, robotsParent);

            Toggle toggle = robotToggle.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(delegate
            {
                SelectRobot(robot);
            });
            toggle.group = robotsGroup;

            if (robot.robotName == robotNameToLoad)
            {
                activeRobot = robot;
                toggle.isOn = true;
            }


            TextMeshProUGUI text = robotToggle.GetComponentInChildren<TextMeshProUGUI>();
            text.text = robot.robotName;
        }

        alreadyInit = true;
    }

    public void SelectLevel(LevelScriptableObject activeLevel)
    {
        MenuController.activeLevel = activeLevel;
        PlayerPrefs.SetInt("lastLevel", activeLevel.levelId);
    }

    public void SelectRobot(RobotScriptableObject activeRobot)
    {
        MenuController.activeRobot = activeRobot;
        PlayerPrefs.SetString("lastRobot", activeRobot.robotName);

        // Delete existing options
        foreach (GameObject option in options)
        {
            Destroy(option);
        }

        // Make new ones for the new robot
        foreach(Option option in activeRobot.options)
        {
            GameObject optionObj = Instantiate(optionPrefab, optionEntryParent);
            options.Add(optionObj);

            TextMeshProUGUI text = optionObj.GetComponentInChildren<TextMeshProUGUI>();
            text.text = option.name;

            TMP_InputField input_field = optionObj.GetComponentInChildren<TMP_InputField>();
            input_field.text = RobotOptions.GetValue(activeRobot.robotName + option.name);
            input_field.onEndEdit.AddListener(delegate
            {
                RobotOptions.SetValue(activeRobot.robotName + option.name, input_field.text);
            });
            keyToOptionField[activeRobot.robotName + option.name] = input_field;
        }
    }

    public void SaveOptions()
    {
        RobotOptions.Save();
    }

    public void LoadOptions()
    {
        RobotOptions.LoadSaved(robots);

        FillOptionFields();
    }

    public void DefaultOptions()
    {
        RobotOptions.LoadDefaults(robots);

        FillOptionFields();
    }

    public void FillOptionFields()
    {
        foreach (string key in keyToOptionField.Keys)
        {
            keyToOptionField[key].text = RobotOptions.GetValue(key);
        }
    }

    public void PlaySim()
    {
        SceneManager.LoadScene(activeLevel.levelId);
    }
}
