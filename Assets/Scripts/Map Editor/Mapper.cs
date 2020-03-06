using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GUI;

public class Mapper : MonoBehaviour
{
    public MapEditor Editor;
    public bool DrawUI = false;

    private int TagStorage = 0;

    public GameObject SelectedObject;
    public GameObject SelectedPrefab;
    public GameObject update_draggingObject;
    public GameObject CreateMapMenu;
    public GameObject PrefabListingMenu;
    private float timeout = 0;
    public bool update_leftCickHold = false;

    public GameObject ScrollbarContentObject;
    public GameObject PrefabButtonObject;

    public bool Disabled = false;

    public Vector2 scrollPosition;

    public void OnGUI()
    {
        if (!DrawUI)
            return;

        var debugText = $"{(Editor.activeMap != null ? $"Map: {Editor.activeMap.mapName} by {Editor.activeMap.mapAuthor}" : "Map: No Selected Map")} " +
            $"| Total Prefabs: {(Editor.activeMap != null ? Editor.activeMap.mapObjects.Count.ToString() : "0")}";
        var sizeOfText = skin.box.CalcSize(new GUIContent(debugText));
        Label(new Rect(5, 5, sizeOfText.x + 5, sizeOfText.y + 5), debugText);
    }

    private void ResetSelectedObject()
    {
        if (SelectedObject == null)
            return;

        SelectedObject.layer = TagStorage;
        SelectedObject = null;
        TagStorage = 0;
        update_leftCickHold = false;
        update_draggingObject = null;
    }

    private void OnSelectedPrefabChanged(int index)
    {
        var prefab = Editor.Prefabs[index];
        SelectedPrefab = Instantiate(prefab.Reference);
    }
    
    public void OnMapCreated()
    {
        Editor.activeMap = new MapEditor.CustomMap();
        Editor.activeMap.mapName = CreateMapMenu.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(1).GetComponent<TMPro.TMP_InputField>().text; // This is terrible
        Editor.activeMap.mapAuthor = CreateMapMenu.transform.GetChild(0).GetChild(0).GetChild(1).GetChild(1).GetChild(1).GetComponent<TMPro.TMP_InputField>().text; // This is terrible

        // We need to verify data first! Then let the user do this, otherwise show error mesasge?
        Disabled = false;
        CreateMapMenu.SetActive(false);
        PrefabListingMenu.SetActive(true);
    }

    private void AddObject(GameObject obj)
    {
        if(Editor.activeMap != null)
        {
            PrefabScriptableObject prefab = obj.GetComponent<PrefabScriptableObject>();
        } else
        {
            Disabled = true;
            CreateMapMenu.SetActive(true);
            PrefabListingMenu.SetActive(false);
        }
    }

    void Update()
    {
        if (Editor == null)
        {
            Editor = FindObjectOfType<MapEditor>();
            if (Editor != null)
            {
                // Found the editor, treat this as our 'start' statement
                DrawUI = true;

                for(int i = 0; i < Editor.Prefabs.Count; i++)
                {
                    var prefab = Editor.Prefabs[i];

                    var obj = Instantiate(PrefabButtonObject, ScrollbarContentObject.transform);
                    obj.transform.GetChild(1).GetComponent<TMPro.TMP_Text>().text = prefab.Name;
                    int copy = i; // For some reason Lambdas (below) really hate not using a copy in a for loop? So here you go Mr. Lambda
                    obj.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => OnSelectedPrefabChanged(copy)));
                }
            } else
            {
                return; // Still have not found the editor :(
            }
        }

        if (Disabled)
            return;

        if (timeout > Time.time)
            return;

        // Literally rewrite everything below this line, its trash.

        if(Input.GetMouseButtonDown(0))
        {
            if(SelectedPrefab != null)
            {
                SelectedPrefab = null;
                if(Input.GetKey(KeyCode.LeftShift))
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, 100.0f))
                    {
                        SelectedPrefab.transform.position = new Vector3(hit.point.x, SelectedPrefab.transform.position.y, hit.point.z);
                    }
                }
                timeout = Time.time + 0.25f;
                return;
            } else
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    if (hit.transform.gameObject.tag == "Stuck")
                        return;

                    ResetSelectedObject();

                    SelectedObject = hit.transform.gameObject;
                    TagStorage = SelectedObject.layer;
                    SelectedObject.layer = 8;
                    timeout = Time.time + 0.08f;
                }
                else
                {
                    ResetSelectedObject();
                }
                return; // Skip the next drag frame just incase 
            }
        }

        if(Input.GetMouseButton(0)) // While left click is being held
        {
            if(!update_leftCickHold)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    if (hit.transform.gameObject.tag == "Stuck")
                        return;

                    update_draggingObject = hit.transform.gameObject;
                    update_leftCickHold = true;
                }   
            }

            if(update_draggingObject != null)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    update_draggingObject.transform.position = new Vector3(hit.point.x, update_draggingObject.transform.position.y, hit.point.z);
                }
            }
        } else
        {
            if(update_leftCickHold)
            {
                ResetSelectedObject();
            }
        }
    }
}
