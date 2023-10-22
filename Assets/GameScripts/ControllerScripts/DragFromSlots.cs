using UnityEngine;
using UnityEngine.EventSystems;


public class DragFromSlots : DragFromSlot, IPointerDownHandler, IPointerUpHandler
{

    public float requiredHoldTime = 1.0f; // Èas (v sekundách), který uživatel musí podržet prst na objektu.
    private bool isPointerDown = false;
    private float pointerDownTime = 0f;
    private bool isDragging = false;
    private GameObject inRepeaterInstance;
    [SerializeField] private GameObject inRepeaterPrefab;

    private void Update()
    {


        if (isPointerDown && !isDragging)
        {
            pointerDownTime += Time.deltaTime;
            if (pointerDownTime >= requiredHoldTime)
            {
                // Vyvolání funkce po dlouhém stisku
                OnLongPress();
                pointerDownTime = 0f; // Reset èasu po dlouhém stisku
                isPointerDown = false; // Nastavte zpìt na false, aby se znovu nevyvolal.
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {

        if ((transform.childCount > 0) && transform.GetChild(0).gameObject.tag == "Repeater")
        {
            
            isPointerDown = true;
            pointerDownTime = 0f; // Reset èasu pøi stisku
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        pointerDownTime = 0f; // Reset èasu pøi uvolnìní
        Destroy(inRepeaterInstance);
    }

    private void OnLongPress()
    {
        var position = Input.mousePosition;
        position.y = position.y + 20f;
        inRepeaterInstance = Instantiate(inRepeaterPrefab, position, Quaternion.identity);
        inRepeaterInstance.GetComponent<InRepeaterScript>().setImages(transform.GetChild(0).gameObject.GetComponent<RepeateScript>().GetCommands());

    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (currentButtonCopy != null)
        {
            if (inRepeaterInstance != null)
            {
                Destroy(inRepeaterInstance);
            }

            isDragging = true;
            pointerDownTime = 0f;
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(eventData.position);
            touchPosition.z = 0;
            currentButtonCopy.transform.position = touchPosition;
        }
    }


    public override void OnEndDrag(PointerEventData eventData) {
        base.OnEndDrag(eventData);
        isDragging = false;
    }
}
