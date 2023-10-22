using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RepeaterCanvasScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Nastavení nového sortingOrder
        int newSortingOrder = 25;
        GetComponent<Canvas>().overrideSorting = true;
        GetComponent<Canvas>().sortingOrder = newSortingOrder;

    }

  
}
