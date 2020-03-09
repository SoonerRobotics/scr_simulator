using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorMovement : MonoBehaviour
{
    private EditorHandle editorHandle;

    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            if(editorHandle == null)
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

                var forward = Camera.main.transform.forward;
                var right = Camera.main.transform.right;
                forward.Normalize();
                right.Normalize();

                if (editorHandle.Direction == EditorHandle.HandleDirection.X)
                {
                    var desiredDirection = (forward * moveY + right * moveX).x * 5f;
                    parent.position += new Vector3(desiredDirection * Time.deltaTime * 6, 0, 0);
                } else if (editorHandle.Direction == EditorHandle.HandleDirection.Y)
                {
                    var desiredDirection = -(forward * moveY + right * moveX).y * 15f;
                    parent.position += new Vector3(0, desiredDirection * Time.deltaTime * 6, 0);
                } else if (editorHandle.Direction == EditorHandle.HandleDirection.Z)
                {
                    var desiredDirection = (forward * moveY + right * moveX).z * 5f;
                    parent.position += new Vector3(0, 0, desiredDirection * Time.deltaTime * 6);
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
