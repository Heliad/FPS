using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactionMinDistance = 2.5F;
    [SerializeField][Range(0, 0.03F)] float HighlightedLineWidth = 0.001F;
    [SerializeField] Color lineColor = Color.white;
    PlayerInventory playerInventory;

    Ray ray;
    RaycastHit hit;  

    InteractableObject currentInteractable;

    public float interactionDistance { get { return interactionMinDistance; } }

    void Start ()
    {
        playerInventory = GetComponent<PlayerInventory>();
        ray = new Ray();
    }

	void Update ()
    {
        ray.origin = Camera.main.gameObject.transform.position;
        ray.direction = Camera.main.gameObject.transform.forward;

        if (Physics.Raycast(ray, out hit, interactionMinDistance))
        {
            if (hit.collider.gameObject.GetComponent<InteractableObject>() != null)
            {
                if (currentInteractable != null)
                {
                    if (hit.collider.gameObject.GetComponent<InteractableObject>() != currentInteractable)
                    {
                        currentInteractable.SetShaderFloat(0);
                    }
                }
                currentInteractable = hit.collider.gameObject.GetComponent<InteractableObject>();
                currentInteractable.SetShaderFloat(HighlightedLineWidth);
                currentInteractable.SetShaderColor(lineColor);
                
                if (Input.GetKey(KeyCode.E))
                {
                    playerInventory.AddObject(hit.collider.gameObject);
                    currentInteractable.SetShaderFloat(0);
                }
            }
            else if (currentInteractable != null)
            {
                currentInteractable.SetShaderFloat(0);
                currentInteractable = null;
            }
        }
        else if (currentInteractable != null)
        {
            currentInteractable.SetShaderFloat(0);
            currentInteractable = null;
        }
    }
}
