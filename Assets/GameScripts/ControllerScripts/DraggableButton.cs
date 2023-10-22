using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableButton : DragFromSlot, IBeginDragHandler, IDragHandler
{
    public GameObject buttonPrefab; // Pøeddefinované tlaèítko pro kopii

    public override void OnBeginDrag(PointerEventData eventData)
    {
        mainCamera.GetComponent<CameraController>().SetChangeview(false);
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        touchPosition.z = 0;
        currentButtonCopy = Instantiate(buttonPrefab, touchPosition, Quaternion.identity);
        currentButtonCopy.transform.SetParent(transform.parent.parent, false);
        // Nastavíme stejný scale jako má rodiè.
        currentButtonCopy.transform.localScale = transform.localScale;
    }


}
    