using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapper : MonoBehaviour
{
    public MapEditor Editor;
    public bool DrawUI = false;

    public void OnGUI()
    {
        if (!DrawUI)
            return;

        var debugText = $"{(Editor.activeMap != null ? $"Map: {Editor.activeMap.mapName} by {Editor.activeMap.mapAuthor}" : "Map: No Selected Map")} " +
            $"| Total Prefabs: {(Editor.activeMap != null ? Editor.activeMap.mapObjects.Count.ToString() : "0")}";
        var sizeOfText = GUI.skin.box.CalcSize(new GUIContent(debugText));
        GUI.Label(new Rect(5, 5, sizeOfText.x + 5, sizeOfText.y + 5), debugText);
    }

    void Update()
    {
        if(Editor == null)
        {
            Editor = FindObjectOfType<MapEditor>();
            if (Editor != null)
            {
                // Found the editor, treat this as our 'start' statement
                DrawUI = true;
            } else
            {
                return; // Still have not found the editor :(
            }
        }


    }
}
