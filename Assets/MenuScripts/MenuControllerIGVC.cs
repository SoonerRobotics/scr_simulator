using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

public class MenuControllerIGVC : MonoBehaviour
{
    public GameObject togglePrefab;

    [Header("Levels")]
    public LevelScriptableObject[] levels;
    public ToggleGroup togglesGroup;
    public RectTransform togglesParent;
    private LevelScriptableObject activeLevel;

    [Header("Robots")]
    public RobotScriptableObject[] robots;
    public ToggleGroup robotsGroup;
    public RectTransform robotsParent;
    private RobotScriptableObject activeRobot;
    private Dictionary<string, TMP_InputField> keyToOptionField = new Dictionary<string, TMP_InputField>();

    [Header("Options")]
    public GameObject optionPrefab;
    public RectTransform optionEntryParent;
    private List<GameObject> options = new List<GameObject>();

    public void Start()
    {
        // Load up Robot Options
        RobotOptions.LoadSaved(robots);

        bool first = true;

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

            if (first)
            {
                toggle.isOn = true;
                first = false;
            }

            TextMeshProUGUI text = levelToggle.GetComponentInChildren<TextMeshProUGUI>();
            text.text = level.levelName;
        }

        first = true;

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

            if (first)
            {
                toggle.isOn = true;
                first = false;
            }


            TextMeshProUGUI text = robotToggle.GetComponentInChildren<TextMeshProUGUI>();
            text.text = robot.robotName;
        }
    }

    public void SelectLevel(LevelScriptableObject activeLevel)
    {
        this.activeLevel = activeLevel;
    }

    public void SelectRobot(RobotScriptableObject activeRobot)
    {
        this.activeRobot = activeRobot;

        // Delete existing options
        foreach(GameObject option in options)
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
            input_field.text = RobotOptions.GetValue(activeRobot.name + option.name);
            input_field.onEndEdit.AddListener(delegate
            {
                RobotOptions.SetValue(activeRobot.name + option.name, input_field.text);
            });
            keyToOptionField[activeRobot.name + option.name] = input_field;
        }
    }

    public void SaveOptions()
    {
        RobotOptions.Save();
    }

    public void LoadOptions()
    {
        RobotOptions.LoadSaved(robots);

        foreach (string key in keyToOptionField.Keys)
        {
            keyToOptionField[key].text = RobotOptions.GetValue(key);
        }
    }

    public void DefaultOptions()
    {
        RobotOptions.LoadDefaults(robots);

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
