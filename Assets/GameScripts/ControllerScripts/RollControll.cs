using UnityEngine;
using UnityEngine.UI;

public class RollControll : MonoBehaviour
{

    public RectTransform canvasObjectRectTransform;
    private bool isScrolled = false;
    [SerializeField] private float x;
    [SerializeField] private float y;
    [SerializeField] private Button scrollButton;
    public void Scroll()
    {
        
        Vector2 newPosition;
        if (isScrolled) {
            newPosition = canvasObjectRectTransform.anchoredPosition + new Vector2(x, y);
        }
        else {
            newPosition = canvasObjectRectTransform.anchoredPosition + new Vector2(-x, -y);           
        }
        canvasObjectRectTransform.anchoredPosition = newPosition;
        isScrolled = !isScrolled;

    }

    public void HideAndDisable() {
        if (!isScrolled)
        {        
            Scroll();
        }
        scrollButton.enabled = false;
    }

    public void AllowScrol() { 
        scrollButton.enabled = true;
        Scroll();
    }
}
