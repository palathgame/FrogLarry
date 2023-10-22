using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RepeateScript : MonoBehaviour
{

    private List<string> comannds = new List<string>();
    private int lvl = 0;
    private int countCycle = 1;
    [SerializeField] public TextMeshProUGUI countRepeatText;
    private List<InternalRepeaterScript> InternalRepeaterScripts;
    public int GetRepeat() {
    
        return countCycle;
    
    }


    public void AddCommands(List<string> comannds)
    {
        this.comannds = comannds;
    }

    public List<string> GetCommands()
    {
        return comannds;
    }

    public void SetLvl(int lvl) { 
    
        this.lvl = lvl;
    }
    public int GetLvl() { 
    
            return lvl;
    
    }
    public int GetCountCycle()
    {

        return countCycle;

    }


    public void Plus() {
        if (countCycle <10)
        {
            countCycle++;
            countRepeatText.text = countCycle.ToString();
        }
        
    }
    public void Minus()
    {
        if (countCycle > 1)
        {
            countCycle--;
        }
        countRepeatText.text = countCycle.ToString();
    }

    public void AddInternalRepeaterScripts (List<InternalRepeaterScript> InternalRepeaterScripts) { 
    
        this.InternalRepeaterScripts = InternalRepeaterScripts;
    
    }
    public List<InternalRepeaterScript> GetInternalRepeaterScripts() { 
    
    
            return InternalRepeaterScripts;
    }

    public InternalRepeaterScript GetInternalRepeaterScript(int position)
    {

        return InternalRepeaterScripts[position];
    }


}
