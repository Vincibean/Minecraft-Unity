using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour {

    [SerializeField]
    private UIItemSlot cursorSlot = null;

    private ItemSlot cursorItemSlot;

    [SerializeField]
    private GraphicRaycaster m_Raycaster = null;

    private PointerEventData m_PointerEventData;

    [SerializeField]
    private EventSystem m_EventSystem = null;

    World world;

    private void Start() {
        world = GameObject.Find("World").GetComponent<World>();
        cursorItemSlot = new ItemSlot(cursorSlot);
    }

    private void Update() {
        if (!world.inUI) {
            return;
        }

        cursorSlot.transform.position = Input.mousePosition;

        if (Input.GetMouseButtonDown(0)) {
            HandleSlotClick(CheckForSlot());
        }
    }

    private void HandleSlotClick (UIItemSlot clickedSlot) {
        if (clickedSlot == null)
            return;
        
        if (!cursorSlot.HasItem && !clickedSlot.HasItem)
            return;

        if (clickedSlot.itemSlot.isCreative) {
            cursorItemSlot.EmptySlot();
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.stack);
        }

        if (!cursorSlot.HasItem && clickedSlot.HasItem) {
            cursorItemSlot.InsertStack(clickedSlot.itemSlot.TakeAll());
            return;
        }

        if (cursorSlot.HasItem && !clickedSlot.HasItem) {
            clickedSlot.itemSlot.InsertStack(cursorItemSlot.TakeAll());
            return;
        }

        if (cursorSlot.HasItem && clickedSlot.HasItem) {
            if (cursorSlot.itemSlot.stack.id != clickedSlot.itemSlot.stack.id) {
                ItemStack oldCursorSlot = cursorSlot.itemSlot.TakeAll();
                ItemStack oldSlot = clickedSlot.itemSlot.TakeAll();

                clickedSlot.itemSlot.InsertStack(oldCursorSlot);
                cursorSlot.itemSlot.InsertStack(oldSlot);
            }

        }

    }

    private UIItemSlot CheckForSlot () {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;

        // It's going to cast a ray at the mouse position and it's going to return 
        // a list containing all the UI elements that are under the mouse at that position.
        // If for example there is an inventory slot, the inventory slot has an icon in it, 
        // and both the invenory slot and the icon are over the top of the toolbar window, 
        // the code below is going to return all of these objects in this list.
        List<RaycastResult> results = new List<RaycastResult>();
        m_Raycaster.Raycast(m_PointerEventData, results);

        // ensure that we are only looking at items we are interested in
        foreach (RaycastResult result in results) {
            if (result.gameObject.tag == "UIItemSlot")
                return result.gameObject.GetComponent<UIItemSlot>();
        }

        return null;
    }

}
