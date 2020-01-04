using UnityEngine;
using System;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public struct LevelTogglePair
{
    public Toggle toggle;
    public int levelId;
}

public class LevelSelect : MonoBehaviour
{
    public GameObject LevelSelectPanel;
    public GameObject ConfigPanel;

    public LevelTogglePair[] levels;
    
    void Start()
    {
        LevelSelectPanel.SetActive(true);
        ConfigPanel.SetActive(false);
    }


    public void ConfirmLevelSelection()
    {
        foreach (var level in levels)
        {
            if (level.toggle.isOn)
            {
                MenuValues._instance.level_id = level.levelId.ToString();
                LevelSelectPanel.SetActive(false);
                ConfigPanel.SetActive(true);
                break;
            }
        }
    }
}
