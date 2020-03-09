using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GUI;

public class Mapper : MonoBehaviour
{
    #region Variables
    // Primary References
    private MapEditor Editor;
    private FreeFlyCameraScript FreeFly;

    // GUI Containers
    public GameObject CreateMapMenu;
    public GameObject PrefabListingMenu;
    public GameObject EscapeMenu;
    public GameObject LoadMapMenu; // Should also be a prefab
    public GameObject SettingsMenu; // Should probably just be a prefab

    // GUI References
    public GameObject ScrollbarContentObject;
    public GameObject PrefabButtonObject;

    // Object References
    private GameObject SelectedObject;

    // General References
    private bool DrawUI = false; // Whether or not we should actually draw the UI (debug ui)
    private bool Disabled = false; // Whether or not our input is disabled
    private bool ClickedMouseLeft = false; 
    private bool HoldingMouseLeft = false;

    private int TagStorage = 0;

    private float timeout = 0;
    #endregion

    #region Unity Functions
    public void OnGUI()
    {
        if (!DrawUI)
            return;

        var debugText = $"{(Editor.activeMap != null ? $"Map: {Editor.activeMap.mapName} by {Editor.activeMap.mapAuthor}" : "Map: No Selected Map")} " +
            $"| Total Prefabs: {(Editor.activeMap != null ? Editor.activeMap.mapObjects.Count.ToString() : "0")}";
        var sizeOfText = skin.box.CalcSize(new GUIContent(debugText));
        Label(new Rect(5, 5, sizeOfText.x + 5, sizeOfText.y + 5), debugText);
    }

    void Update()
    {
        if (Editor == null)
        {
            Editor = FindObjectOfType<MapEditor>();
            if (Editor != null)
            {
                Initialize();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(CreateMapMenu.activeSelf)
            {
                CreateMapMenu.SetActive(false);
                SetAllowInput(true);
                return;
            }  

            SetAllowInput(EscapeMenu.activeSelf);
            EscapeMenu.SetActive(!EscapeMenu.activeSelf);
        }

        if (Disabled)
            return;

        if (timeout > Time.time)
            return;

        // Literally rewrite everything below this line, its trash.

        if (Input.GetMouseButton(0)) // Get left click
        {
            if (!ClickedMouseLeft && !HoldingMouseLeft)
            {
                ClickedMouseLeft = true;
                timeout = Time.time + 0.1f;
                return;
            }
            else
            {
                HoldingMouseLeft = true;
                ClickedMouseLeft = false;
            }

            OnHoldLeftClick();
            // Handle holding mouse left here
        }
        else
        {
            if (ClickedMouseLeft && !HoldingMouseLeft)
            {
                OnLeftClick();
                ClickedMouseLeft = false;
            }
            else if (!ClickedMouseLeft && HoldingMouseLeft)
            {
                OnStopHoldingLeftClick();
                HoldingMouseLeft = false;
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
        {
            // Ctrl + S for quick save
            if (Editor.activeMap.mapName.Equals(""))
                return;

            Editor.activeMap.Save();
        }

        if(Input.GetKeyDown(KeyCode.Tab))
        {
            PrefabListingMenu.SetActive(!PrefabListingMenu.activeSelf);
        }
    }
    #endregion

    #region Escape Menu
    public void OnGameQuit()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
              Application.Quit();
        #endif
    }
    public void OnReturnToMenu()
    {
        SceneManager.LoadScene((int)GameScenes.SceneMenu);
    }
    public void OnGameSave()
    {
        if (!Editor.activeMap.mapName.Equals(""))
        {
            Editor.activeMap.Save();
        }
        else
        {
            SetAllowInput(false);
            CreateMapMenu.SetActive(true);
            EscapeMenu.SetActive(false);
        }
    }
    #endregion

    #region Other Functions
    /// <summary>
    /// Disable or enable input
    /// </summary>
    /// <param name="status"></param>
    private void SetAllowInput(bool status)
    {
        Disabled = !status;
        FreeFly.Disabled = !status;
    }

    /// <summary>
    /// Resets the select object
    /// </summary>
    private void ResetSelectedObject()
    {
        if (SelectedObject == null)
            return;

        SelectedObject.layer = TagStorage;
        SelectedObject = null;
        TagStorage = 0;
    }

    /// <summary>
    /// Calls when a user spawns a prefab.
    /// TODO: Change the name and stuff
    /// </summary>
    /// <param name="index">The index of the prefab in the prefab list</param>
    private void OnSelectedPrefabChanged(int index)
    {
        if (Disabled)
            return;

        if (!Editor.activeMap.mapName.Equals(""))
        {
            var prefab = Editor.Prefabs[index];
            var pref = Instantiate(prefab.Reference);
            var info = pref.AddComponent<ObjectInfo>();
            info.prefab = prefab;
            CompleteObject(pref);
        }
        else
        {
            SetAllowInput(false);
            CreateMapMenu.SetActive(true);
        }
    }
    
    /// <summary>
    /// Called when the user creates a map using the map creation screen
    /// </summary>
    public void OnMapCreated()
    {
        Editor.activeMap = new MapEditor.CustomMap();
        Editor.activeMap.mapName = CreateMapMenu.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetComponent<TMPro.TMP_InputField>().text; // This is terrible
        Editor.activeMap.mapAuthor = CreateMapMenu.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(1).GetChild(1).GetComponent<TMPro.TMP_InputField>().text; // This is terrible

        // We need to verify data first! Then let the user do this, otherwise show error mesasge?
        SetAllowInput(true);
        CreateMapMenu.SetActive(false);
    }

    /// <summary>
    /// Saves the object into the map editor
    /// </summary>
    /// <param name="obj"></param>
    private void CompleteObject(GameObject obj)
    {
        ObjectInfo info = obj.GetComponent<ObjectInfo>();
        if(info == null)
        {
            Debug.LogError("Failed to find info for object: " + obj);
            return;
        }

        Editor.activeMap.AddObject(obj, info);
    }

    /// <summary>
    /// Called when the user left clicks
    /// </summary>
    private void OnLeftClick()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            ResetSelectedObject();
            if (hit.transform.gameObject.GetComponent<ObjectInfo>() == null)
                return;

            SelectedObject = hit.transform.gameObject;
            TagStorage = SelectedObject.layer;
            SelectedObject.layer = 8;
            timeout = Time.time + 0.08f;
        } else
        {
            ResetSelectedObject();
        }
    }

    /// <summary>
    /// Called whilst the user is holding left click
    /// </summary>
    private void OnHoldLeftClick()
    {
        if(SelectedObject != null)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                SelectedObject.transform.position = new Vector3(hit.point.x, SelectedObject.transform.position.y, hit.point.z);
            }
        }
    }

    /// <summary>
    /// Initializes the Mapper script, used instead of start because we need to make sure the MapEditor object exists.
    /// </summary>
    private void Initialize()
    {
        DrawUI = true;

        for (int i = 0; i < Editor.Prefabs.Count; i++)
        {
            var prefab = Editor.Prefabs[i];

            var obj = Instantiate(PrefabButtonObject, ScrollbarContentObject.transform);
            obj.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = prefab.Name;
            int copy = i; // For some reason Lambdas (below) really hate not using a copy in a for loop? So here you go Mr. Lambda
            obj.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => OnSelectedPrefabChanged(copy)));
        }

        FreeFly = GetComponent<FreeFlyCameraScript>();
    }

    private void OnStopHoldingLeftClick()
    {
        Editor.activeMap.UpdateObject(SelectedObject); // Whilst SelectedObject can be null, UpdateObject has a check for it!
        ResetSelectedObject();
    }
    #endregion
}
