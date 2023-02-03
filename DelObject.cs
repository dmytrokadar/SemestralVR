using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.XR;

public class DelObject : MonoBehaviour
{
    public XRNode inputR;
    public InputHelpers.Button button;
    private XRBaseInteractable currentInteractable;
    private GameObject gObj; 

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($" hovered over ");
        if (gObj)
        {
            //gObj && InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputR), button, out bool tmp) && tmp
            Destroy(gObj);
            gObj = null;
        }
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log($"{args.interactorObject} hovered over {args.interactableObject}", this);
    } 
    
    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log($"{args} hovered over {args}", this);
        gObj = args.interactable.gameObject;
    }
}
