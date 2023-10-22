using UnityEngine;

public class InstructionsScript : MonoBehaviour
{
    [SerializeField] GameObject[] instructions;
    private int instruction = 0;
    public void BackMenu()
    {
        Destroy(gameObject);
    }
    public void Next()
    {

        if (instruction < instructions.Length-1)
        {
            instruction = instruction + 1;
        }
        else
        {
            instruction = 0;
        }
        
        SetActiveInstruction();
    }
    public void Prev()
    {
        if (instruction > 0)
        {
            instruction = instruction - 1;
        }
        else
        {
            instruction = instructions.Length-1;
        }

        SetActiveInstruction();

    }

    private void SetActiveInstruction()
    {
        for (int i = 0; i < instructions.Length; i++)
        {
            Debug.Log(instruction);
            Debug.Log(i);
            if (i == instruction) {
                instructions[i].SetActive(true);
            }
            else
            {
                instructions[i].SetActive(false);
            }

        }

    }
}
