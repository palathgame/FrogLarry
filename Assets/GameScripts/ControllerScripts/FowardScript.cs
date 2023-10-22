using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FowardScript : MonoBehaviour
{
    private Button button;
    private void Start()
    {
        button = GetComponent<Button>();
        if (Time.timeScale == 2.0)
        {
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);

        }
    }
    public void SetForward()
    {

        Color currentColor = button.image.color;

        if (Time.timeScale == 1)
        {
            Time.timeScale = 2.0f;
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);

        }
        else
        {
            Time.timeScale = 1.0f;
        }

    }

}