using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorHandle : MonoBehaviour
{
    public HandleType Type;
    public HandleDirection Direction;

    public Material handleMaterial;

    public Color handleX = new Color(255, 0, 0, 0.5f); // Put these in a centralized class, or config (probably should use the config system. TODO: Implement color serialization)
    public Color handleY = new Color(255, 255, 0, 0.5f);
    public Color handleZ = new Color(0, 0, 255, 0.5f);

    private void Start()
    {
        Color c = Color.red;
        switch (Direction)
        {
            case HandleDirection.X:
                {
                    c = handleX;
                } break;
            case HandleDirection.Y:
                {
                    c = handleY;
                }
                break;
            case HandleDirection.Z:
                {
                    c = handleZ;
                }
                break;
        }

        Renderer renderer = GetComponent<Renderer>();
        renderer.material = handleMaterial;
        renderer.material.color = c;


        renderer = transform.parent.GetComponent<Renderer>();
        renderer.material = handleMaterial;
        renderer.material.color = c;
    }

    public Transform GetParent()
    {
        return transform.parent.parent;
    }

    public enum HandleType
    { 
        Move
    }

    public enum HandleDirection
    { 
        X,
        Y,
        Z
    }
}
