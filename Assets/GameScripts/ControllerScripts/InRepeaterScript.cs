using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InRepeaterScript : MonoBehaviour
{
    [SerializeField] Image[] images;
    public void setImages(List<string> commands) { 
        

       for (int i = 0; i < images.Length; i++) {

            if (i < commands.Count)
            {

                images[i].sprite = Resources.Load<Sprite>("ButtonGraphics/" + commands[i]);

            }
            else {

                images[i].enabled = false;
            }
        
        }
        
    
    }
}
