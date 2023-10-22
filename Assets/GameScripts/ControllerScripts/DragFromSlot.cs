using UnityEngine;
using UnityEngine.EventSystems;

public class DragFromSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    protected GameObject mainCamera;
    protected static GameObject currentButtonCopy;

    private void Start()
    {
        mainCamera = GameObject.FindWithTag("MainCamera");

    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        mainCamera.GetComponent<CameraController>().SetChangeview(false);
        if (transform.childCount > 0)
        {
            currentButtonCopy = transform.GetChild(0).gameObject;
            currentButtonCopy.transform.SetParent(transform.parent.parent, false);
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (currentButtonCopy != null)
        {
            
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(eventData.position);
            touchPosition.z = 0;
            currentButtonCopy.transform.position = touchPosition;
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {       
 
        if (currentButtonCopy != null)
        {
            mainCamera.GetComponent<CameraController>().SetChangeview(true);
            // Získáme pozici dotyku ve svìtových souøadnicích
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(eventData.position);
            touchPosition.z = 0;
            // Získáme Collider2D komponentu tlaèítka
            BoxCollider2D buttonCollider = currentButtonCopy.GetComponent<BoxCollider2D>();

            // Pokud máme Collider2D, zkontrolujeme, zda kolizuje s nìjakým objektem
            if (buttonCollider != null)
            {
                // Detekujeme kolize s jinými objekty s PolygonCollider2D
                Collider2D[] colliders = Physics2D.OverlapPointAll(touchPosition);
                foreach (Collider2D collider in colliders)
                {
                    if (collider != buttonCollider)
                    {
                        // Pokud koliduje se SlotPrefab, pøilepíme kopii na nìj
                        if (collider.gameObject.CompareTag("SlotPrefab"))
                        {

                            var Slot = collider.gameObject.transform;
                            foreach (Transform child in Slot)
                            {
                                Destroy(child.gameObject);
                            }

                            currentButtonCopy.transform.position = Slot.position;

                            currentButtonCopy.transform.SetParent(Slot);
                            currentButtonCopy = null;
                            return; // Ukonèíme hledání kolizí
                        }
                    }
                }
                // Znièíme kopii tlaèítka
                Destroy(currentButtonCopy);
            }

            
        }
    }
}
