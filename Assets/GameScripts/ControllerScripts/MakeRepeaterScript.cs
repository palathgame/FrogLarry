
using System.Collections.Generic;
using UnityEngine;

public class MakeRepeaterScript : MonoBehaviour
{
    [SerializeField]private GameObject repeaterPrefab;

    public void ReadCommands()
    {
        bool addLvl = false;
        List<string> commands = new List<string>();
        GameObject[] slotCreators = GameObject.FindGameObjectsWithTag("SlotCreator2");
        List<InternalRepeaterScript> internalRepeaterScripts = new List<InternalRepeaterScript>();
        int setLvl = 0;
        foreach (GameObject slotCreator in slotCreators)
        {

            Transform slotTransform = slotCreator.transform;
            for (int i = 0; i < slotTransform.childCount; i++)
            {
                Transform childTransform = slotTransform.GetChild(i);
                if (childTransform.childCount > 0)
                {
                    if (childTransform.GetChild(0).tag == "Repeater") {
                       int actualLvl = childTransform.GetChild(0).GetComponent<RepeateScript>().GetLvl();
                        if (actualLvl > 2)
                        {
                            return;
                        }
                        else if(setLvl<=actualLvl) {
                            setLvl = actualLvl+1;
                        }
                        addLvl = true;

                        
                        

                    }
                }
            }
        }

        foreach (GameObject slotCreator in slotCreators)
        {

           Transform slotTransform = slotCreator.transform;
            for (int i = 0; i < slotTransform.childCount; i++)
            {
                Transform childTransform = slotTransform.GetChild(i);
                if (childTransform.childCount > 0)
                {
                    var command = childTransform.GetChild(0).tag;
                    if(command == "Repeater")
                    {                       
                        var repeateScript = childTransform.GetChild(0).GetComponent<RepeateScript>();
                        InternalRepeaterScript internalRepeaterScript = 
                            new InternalRepeaterScript(repeateScript.GetCommands(), repeateScript.GetInternalRepeaterScripts(), repeateScript.GetRepeat());
                        internalRepeaterScripts.Add(internalRepeaterScript);
                    }
                    commands.Add(command);
                    Destroy(childTransform.GetChild(0).gameObject);
                }
            }
        }

        if (commands.Count > 0)
        {
            GameObject repeaterInstance = Instantiate(repeaterPrefab);
            repeaterInstance.GetComponent<RepeateScript>().AddCommands(commands);
            repeaterInstance.GetComponent<RepeateScript>().AddInternalRepeaterScripts(internalRepeaterScripts);

           // 

            repeaterInstance.transform.SetParent(slotCreators[0].transform.GetChild(0).transform);

            // repeaterInstance.transform.SetParent(slotCreators[0].transform);

            // Nastaví pozici a rotaci instance dle potøeby
            repeaterInstance.transform.localPosition = Vector3.zero;
            repeaterInstance.transform.localRotation = Quaternion.identity;
            repeaterInstance.transform.localScale = transform.localScale;
            if (addLvl) { 
                repeaterInstance.GetComponent<RepeateScript>().SetLvl(setLvl); 
            }


        }
    } 
}
