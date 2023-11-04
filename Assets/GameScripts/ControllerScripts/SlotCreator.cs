using UnityEngine;
using UnityEngine.UI;

public class SlotCreator : MonoBehaviour
{
    public GameObject slotPrefab; // Prefab slotu
    public Transform panelTransform; // Transform panelu, kam se budou sloty vytváøet
    private float xOffset = 0; // Odstup mezi sloty
    [SerializeField] Color color;


    private void Start()
    {
        CreateSlots(10); // Vytvoøí 10 slotù na zaèátku hry
    }

    private void CreateSlots(int numSlots)
    {

    
        for (int i = 0; i < numSlots; i++)
        {

            GameObject slot = Instantiate(slotPrefab, panelTransform);
            var slotTransform = slotPrefab.GetComponent<RectTransform>();
            slot.transform.localPosition = new Vector3(xOffset, -95, 0);
            xOffset += slotTransform.rect.width + 2.5f;
            color.a = 1.0f;
            slot.GetComponent<Image>().color=color;

            // Pøevedení svìtové pozice slotu na viewport
            Vector3 slotViewportPosition = Camera.main.WorldToViewportPoint(slot.transform.position);

            // Zkontrolovat, zda je slot za hranicí canvasu
            if (slotViewportPosition.x > 0.9)
            {
                break; // Pøerušení tvorby dalších slotù
            }
        }
    }

}