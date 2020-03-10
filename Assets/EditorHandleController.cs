using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorHandleController : MonoBehaviour
{
    public EditorHandle MoveHandle;
    public EditorHandle ScaleHandle;

    public EditorMovement Movement;

    void Start()
    {
        Movement = GetComponentInParent<EditorMovement>();

        var handles = GetComponentsInChildren<EditorHandle>();
        foreach(var handle in handles)
        {
            if (handle.Type == EditorHandle.HandleType.Move)
                MoveHandle = handle;
            else if (handle.Type == EditorHandle.HandleType.Scale)
                ScaleHandle = handle;
        }

        if(ScaleHandle != null && ScaleHandle.gameObject != null)
        {
            ScaleHandle.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.W))
        {
            SwitchHandle(EditorHandle.HandleType.Move);
        } else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            SwitchHandle(EditorHandle.HandleType.Scale);
        }
    }

    public void SwitchHandle(EditorHandle.HandleType type)
    {
        if (type == EditorHandle.HandleType.Move)
        {
            if (ScaleHandle == null || ScaleHandle.gameObject == null)
                return;

            ScaleHandle.gameObject.SetActive(false);
            MoveHandle.gameObject.SetActive(true);
        } else if (type == EditorHandle.HandleType.Scale)
        {
            if (ScaleHandle == null || ScaleHandle.gameObject == null)
                return;

            ScaleHandle.gameObject.SetActive(true);
            MoveHandle.gameObject.SetActive(false);
        }
    }

    public EditorHandle GetActiveHandle()
    {
        if (MoveHandle.gameObject.activeSelf)
            return MoveHandle;
        else
            return ScaleHandle;
    }
}
