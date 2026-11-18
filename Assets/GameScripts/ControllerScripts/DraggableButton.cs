using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableButton : DragFromSlot, IBeginDragHandler, IDragHandler
{
    public GameObject buttonPrefab; // Pøeddefinované tlaèítko pro kopii

    public override void OnBeginDrag(PointerEventData eventData)
    {
        mainCamera.GetComponent<CameraController>().SetChangeview(false); // zastaví kameru
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(eventData.position);//ziska pozici dotyku
        touchPosition.z = 0;
        currentButtonCopy = Instantiate(buttonPrefab, touchPosition, Quaternion.identity);
        currentButtonCopy.transform.SetParent(transform.parent.parent, false); // nastavi rodice aby byl videt
        currentButtonCopy.transform.localScale = transform.localScale;
    }


}
    