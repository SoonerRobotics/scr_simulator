using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class LevelSelect : MonoBehaviour
{
    MenuControllerIGVC menuControllerIGVC;

    private int selectedLevelIndex = 0;

    public GameObject LevelSelectPanel;
    public GameObject IGVCMenuPanel;

    void Start()
    {
        menuControllerIGVC = GetComponent<MenuControllerIGVC>();

        LevelSelectPanel.SetActive(true);
        IGVCMenuPanel.SetActive(false);
    }

    public void RecieveLevelIndex(int levelIndex)
    {
        selectedLevelIndex = levelIndex;
    }

    public void ConfirmLevelSelection()
    {
        if (selectedLevelIndex == 1)
            IGVCMenuPanel.SetActive(true);
        else if (selectedLevelIndex == 2)
            LevelSelectPanel.SetActive(true);  //Load the other panel... or just load the sim if options arent required
        else
            LevelSelectPanel.SetActive(false);

        MenuValues._instance.level_id = selectedLevelIndex.ToString();
    }
}
