using UnityEngine;
using UnityEngine.UI;

public class RunPanelControll : RollControll
{
    [SerializeField] private Button play;
    private Color useColor = new Color(0.784f, 0.784f, 0.784f);
    private ColorBlock colorBlock;
    public void EnablePlay()
    {

        play.enabled = true;
        play.image.color = Color.white;
    }

    public void DisablePlay()
    {
        play.enabled = false;

        play.image.color = useColor;
    }
}
