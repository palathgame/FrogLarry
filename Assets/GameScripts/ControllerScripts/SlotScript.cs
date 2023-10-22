using UnityEngine;
using UnityEngine.EventSystems;

public class SlotScript : MonoBehaviour, IDropHandler
{
    public GameObject currentItem; // Aktuálnì obsazený objekt v slotu

    public void OnDrop(PointerEventData eventData)
    {
    
    }
}