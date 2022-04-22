using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MenuStack))]
public class MouseMode : MonoBehaviour
{
    MenuStack menuStack;
    EventSystem eventSystem;
    bool mouseEnabled;

    void Start()
    {
        menuStack = GetComponent<MenuStack>();
        eventSystem = EventSystem.current;
        
        mouseEnabled = !menuStack.autoSelectButton;
    }

    void Update()
    {
        // Disable mouse mode on any keyboard input.
        bool mouseButtons = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
        bool keyboardInput = Input.anyKeyDown && !Input.GetButtonDown("Cancel") && !mouseButtons;
        if (mouseEnabled && keyboardInput)
        {
            // Disable and hide cursor.
            mouseEnabled = false;
            Cursor.visible = false;
            
            // Auto select buttons on menu change.
            menuStack.autoSelectButton = true;

            // If on a menu, select the initial button.
            if (!menuStack.Empty() && !eventSystem.currentSelectedGameObject)
            {
                eventSystem.SetSelectedGameObject(menuStack.Peek().initialButton);
            }
        }
        // Enable mouse mode on any mouse input.
        else if (!mouseEnabled && (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0
            || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            // Enable and show mouse.
            mouseEnabled = true;
            Cursor.visible = true;

            // Don't auto select buttons on menu change.
            menuStack.autoSelectButton = false;

            // Deselect the current button.
            eventSystem.SetSelectedGameObject(null);
        }

        // Because the buttons are deselected when mouse input is started, I need to account for
        // the edge case where the mouse is already over a button when mouse mode is enabled.
        // This could be avoided by hovering instead of selecting on mouse over.
        if (mouseEnabled && !menuStack.Empty() 
            && eventSystem.currentSelectedGameObject == null)
        {
            // Get the pointer data.
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;

            // Perform a raycast on the current canvas using the pointer data.
            GameObject currentCanvas = menuStack.Peek().canvas;
            List<RaycastResult> results = new List<RaycastResult>();
            GraphicRaycaster raycaster = currentCanvas.GetComponent<GraphicRaycaster>();
            raycaster.Raycast(pointerEventData, results);

            // Select a button if the pointer is over it.
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.GetComponent<Button>())
                {
                    EventSystem.current.SetSelectedGameObject(result.gameObject);
                }
            }
        }
    }

    public bool Enabled()
    {
        return mouseEnabled;
    }
}
