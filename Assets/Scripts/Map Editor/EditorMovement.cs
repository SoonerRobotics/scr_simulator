using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorMovement : MonoBehaviour
{
    public EditorHandle editorHandle;

    private List<Transform> DetachChildren(GameObject obj)
    {
        List<Transform> children = new List<Transform>();

        foreach(Transform child in obj.transform)
        {
            children.Add(child);
        }

        obj.transform.DetachChildren();
        return children;
    }

    private void AttachChildren(List<Transform> children, GameObject obj)
    {
        foreach(Transform child in children)
        {
            child.SetParent(obj.transform, true);
        }
    }

    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            if (editorHandle == null || editorHandle.gameObject == null)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    EditorHandle handle = hit.transform.gameObject.GetComponent<EditorHandle>();
                    if (handle != null)
                    {
                        editorHandle = handle;
                    }
                }
            } else
            {
                float moveX = Input.GetAxis("Mouse X");
                float moveY = Input.GetAxis("Mouse Y");

                if (moveX == 0 || moveY == 0)
                    return;
                Transform parent = editorHandle.GetParent();
                if (parent == null)
                    return;

                Vector3 forward = Camera.main.transform.forward.normalized;
                Vector3 right = Camera.main.transform.right.normalized;
                Vector3 up = Camera.main.transform.up.normalized;

                float desiredDirection;
                Vector3 editedValue;
                if (editorHandle.Direction == EditorHandle.HandleDirection.X)
                {
                    desiredDirection = (forward * moveY + right * moveX).x * 5f;
                    editedValue = new Vector3(desiredDirection * Time.deltaTime * 6, 0, 0);
                }
                else if (editorHandle.Direction == EditorHandle.HandleDirection.Y)
                {
                    desiredDirection = (up * moveY).y * 5f;
                    editedValue = new Vector3(0, desiredDirection * Time.deltaTime * 6, 0);
                }
                else if (editorHandle.Direction == EditorHandle.HandleDirection.Z)
                {
                    desiredDirection = (right * moveY - forward * moveX).x * 5f;
                    editedValue = new Vector3(0, 0, desiredDirection * Time.deltaTime * 6);
                }
                else
                {
                    editedValue = Vector3.zero;
                }

                if (editorHandle.Type == EditorHandle.HandleType.Move)
                    parent.position += editedValue;
                else if (editorHandle.Type == EditorHandle.HandleType.Scale)
                {
                    editedValue *= 2;
                    List<Transform> childList = DetachChildren(parent.gameObject);

                    Transform stick = editorHandle.GetStick();
                    parent.localScale += editedValue;
                    if (editorHandle.Direction == EditorHandle.HandleDirection.X)
                    {
                        stick.localScale += editedValue;
                        editorHandle.transform.position += (editedValue / 2);
                    }
                    else if (editorHandle.Direction == EditorHandle.HandleDirection.Y)
                    {
                        stick.localScale += new Vector3(editedValue.y, 0, 0);
                        editorHandle.transform.position += (editedValue / 2);
                    }
                    else if (editorHandle.Direction == EditorHandle.HandleDirection.Z)
                    {
                        stick.localScale += new Vector3(editedValue.z, 0, 0);
                        editorHandle.transform.position += new Vector3(0, 0, editedValue.z) / 2;
                    }

                    AttachChildren(childList, parent.gameObject);
                }
            }
        } else
        {
            if(editorHandle != null)
            {
                editorHandle = null;
            }
        }
    }
}
