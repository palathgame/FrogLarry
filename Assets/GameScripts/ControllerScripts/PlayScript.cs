
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayScript : MonoBehaviour
{
    private GameObject menu;
    private GameObject frog;
    private Animator frogAnimator;
    private AudioSource[] frogSounds;
    [SerializeField] private GameObject resultScore;
    [SerializeField] private GameObject leftPanel;
    [SerializeField] private GameObject runPanel;
    private int countMoves = 0;
    private Coroutine ReadCommandsCoroutine;
    private bool win = false;
    private bool pause = false;
    private bool coroutineRun = false;
    private GameObject[] slotCreators;

  
    public void ReadCommands()
    {

        if (slotCreators == null) { slotCreators = GameObject.FindGameObjectsWithTag("SlotCreator"); }
        runPanel.GetComponent<RunPanelControll>().DisablePlay();
        frog = GameObject.FindGameObjectWithTag("Frog");
        frogAnimator = frog.GetComponent<Animator>();
        frogSounds = frog.GetComponents<AudioSource>();
        
        ReadCommandsCoroutine = StartCoroutine(ProcessCommands(slotCreators));
        
    }

    private IEnumerator ProcessCommands(GameObject[] slotCreators)
    {
        coroutineRun = true;
        foreach (GameObject slotCreator in slotCreators)
        {
            Transform slotTransform = slotCreator.transform;
            for (int i = 0; i < slotTransform.childCount; i++)
            {

                Transform childTransform = slotTransform.GetChild(i);
                if (childTransform.childCount > 0)
                {
                    if (pause )
                    {
                        pause = false;
                        StopCoroutine(ReadCommandsCoroutine);
                        break;
                    }

                    string tag = childTransform.GetChild(0).tag;
                    switch (tag)
                    {
                        case "Move":
                            yield return StartCoroutine(MoveObjectWithAnimation(1, "FrogMove"));
                            break;
                        case "Jump":
                            yield return StartCoroutine(MoveObjectWithAnimation(2, "FrogJump"));
                            break;
                        case "Eat":
                            yield return StartCoroutine(EatObjectWithAnimation());
                            break;
                        case "Left":
                            yield return StartCoroutine(RotateObjectWithAnimation(-90f, "FrogRight"));
                            break;
                        case "Right":
                            yield return StartCoroutine(RotateObjectWithAnimation(90f, "FrogLeft"));
                            break;
                        case "Repeater":
                            var repater = childTransform.GetChild(0).GetComponent<RepeateScript>();
                            yield return StartCoroutine(ProcessCommands(repater.GetCommands(), repater.GetCountCycle(), repater.GetInternalRepeaterScripts()));
                            break;
                        default:
                            Debug.LogWarning("Neznámý pøíkaz: " + tag);
                            break;
                    }

                    if (win)
                    {
                        MadeResult();
                        break;
                    }

                }
            }
        }

        runPanel.GetComponent<RunPanelControll>().EnablePlay();
    }



    private IEnumerator ProcessCommands(List<string> commands, int repeat, List<InternalRepeaterScript> internalRepeaterScripts)
    {
        coroutineRun = true;
        bool endMove = false;
        for (int i = 0; i < repeat; i++)
        {

            if (endMove) {
                StopCoroutine(ReadCommandsCoroutine);
                break;
            }
           
            int numberInternalRepeater = -1;
            foreach (string command in commands)
            {
                if (pause)
                {
                    endMove = true;
                    pause = false;
                    StopCoroutine(ReadCommandsCoroutine);
                    break;
                }

                switch (command)
                {
                    case "Move":
                        yield return StartCoroutine(MoveObjectWithAnimation(1, "FrogMove"));
                        break;
                    case "Jump":
                        yield return StartCoroutine(MoveObjectWithAnimation(2, "FrogJump"));
                        break;
                    case "Eat":
                        yield return StartCoroutine(EatObjectWithAnimation());
                        break;
                    case "Left":
                        yield return StartCoroutine(RotateObjectWithAnimation(-90f, "FrogRight"));
                        break;
                    case "Right":
                        yield return StartCoroutine(RotateObjectWithAnimation(90f, "FrogLeft"));
                        break;
                    case "Repeater":
                        numberInternalRepeater++;
                        if (internalRepeaterScripts == null || internalRepeaterScripts.Count == 0) { break; }
                        var internalScript = internalRepeaterScripts[numberInternalRepeater];
                        yield return StartCoroutine(ProcessCommands(internalScript.GetCommands(), internalScript.GetRepeat(), internalScript.GetInternalRepeaters()));
                        break;
                    default:
                        Debug.LogWarning("Neznámý pøíkaz: " + tag);
                        break;
                }

                if (win)
                {
                    MadeResult();
                    break;
                }


            }
        }
    }

    private IEnumerator MoveObjectWithAnimation(int gridSize, string animation)
    {
        Collider2D hit = null;
        Collider2D hitBehind = null;
        countMoves += gridSize;

        Vector3 direction = GetDirectionFromRotation(frog.transform); // Lokální smìr, mùžete zmìnit podle potøeby

        Vector3 newPosition = frog.transform.position + direction * 1;
        int layerMask = LayerMask.GetMask("GameElements"); // názvem vrstvy
        Collider2D[] colliders = Physics2D.OverlapCircleAll(newPosition, 0.1f, layerMask);

        if (colliders.Length > 0)
        {
            hit = colliders[0];
        }
        if (gridSize > 1)
        {
            newPosition = newPosition + direction * 1;
            Collider2D[] colliders2 = Physics2D.OverlapCircleAll(newPosition, 0.1f, layerMask);
            if (colliders2.Length > 0)
            {
                hitBehind = colliders2[0];
            }

            if (hit?.tag == "Tree" || hitBehind?.tag == "Stone" || hitBehind?.tag == "Tree")
            {
                yield break;
            }

            if (hit?.tag == "Fly")
            {
                hit.GetComponent<FlyScript>().Api("Escape");
            }
            if (hitBehind?.tag == "Fly")
            {
                hitBehind.GetComponent<FlyScript>().Api("Escape");
            }
            if (hitBehind?.tag == "Pond")
            {
                win = true;

            }

            float duration = frogAnimator.GetCurrentAnimatorStateInfo(0).length;
            float elapsedTime = 0;
            Vector3 startingPosition = frog.transform.position;
            frogAnimator.SetTrigger(animation);
            frogSounds[1].volume = PlayerPrefs.GetFloat("GameSounds", 1.0f);
            frogSounds[1].Play();
            while (elapsedTime < duration)
            {
                frog.transform.position = Vector3.Lerp(startingPosition, newPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            frog.transform.position = newPosition;
        }
        else
        {

            if (hit == null || hit.CompareTag("Fly") || hit.CompareTag("Pond"))
            {
                if (hit != null)
                {
                    if (hit.CompareTag("Fly"))
                    {
                        hit.GetComponent<FlyScript>().Api("Escape");
                    }

                }
                float duration = frogAnimator.GetCurrentAnimatorStateInfo(0).length;
                float elapsedTime = 0;
                Vector3 startingPosition = frog.transform.position;
                frogAnimator.SetTrigger(animation);
                frogSounds[0].volume = PlayerPrefs.GetFloat("GameSounds", 1.0f);
                frogSounds[0].Play();
                if (hit?.tag == "Pond")
                {
                    win = true;

                }
                while (elapsedTime < duration)
                {
                    frog.transform.position = Vector3.Lerp(startingPosition, newPosition, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                frog.transform.position = newPosition;
            }

        }

        yield break;

    }

    private IEnumerator RotateObjectWithAnimation(float angle, string trigger)
    {
        countMoves++;
        Quaternion startRotation = frog.transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, angle);
        float duration = frogAnimator.GetCurrentAnimatorStateInfo(0).length;
        frogAnimator.SetTrigger(trigger);
        frogSounds[2].volume = PlayerPrefs.GetFloat("GameSounds", 1.0f); // Nastavíme hlasitost pro každou komponentu.
        frogSounds[2].Play();

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            frog.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            // Oprava pro zajištìní pøesného dosažení cílové rotace
            if (elapsedTime >= duration)
            {

                frog.transform.rotation = targetRotation; // Nastavíme rotaci na pøesný cíl
                frog.transform.eulerAngles = new Vector3(0, 0, Mathf.Round(frog.transform.eulerAngles.z / 90) * 90);

            }

            yield return null;
        }

        // Pøidáme èekání na dokonèení animace
        yield return new WaitForSeconds(frogAnimator.GetCurrentAnimatorStateInfo(0).length);
        // if (openResult) { Time.timeScale = 0.0f; openResult = false; StopCoroutine(ReadCommandsCoroutine); }
       
    }

    private IEnumerator EatObjectWithAnimation()
    {
        frogAnimator.SetTrigger("FrogCatch");

        float duration = frogAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(duration);

    }

    private Vector3 GetDirectionFromRotation(Transform transform)
    {
        float angle = transform.rotation.eulerAngles.z;
        if (angle == 0f) // Zaba je natoèena nahoru
        {
            return Vector3.up;
        }
        else if (angle == 90f) // Zaba je natoèena doleva
        {
            return Vector3.left;
        }
        else if (angle == 180f) // Zaba je natoèena dolu
        {
            return Vector3.down;
        }
        else if (angle == 270f) // Zaba je natoèena doprava
        {
            return Vector3.right;
        }
        else
        {
            return Vector3.up; // Defaultní smìr, pokud rotace není ve známém úhlu
        }
    }
    private void MadeResult()
    {
        setResult();
        menu.GetComponent<ResultScoreScript>().setMoves(countMoves);
        menu.GetComponent<ResultScoreScript>().setWin();
    }

    public void StopCode()
    {
       
        if (coroutineRun && ReadCommandsCoroutine != null)
        {
            pause = true;
            coroutineRun = false;
        }
        else if (!coroutineRun)
        {
            slotCreators = GameObject.FindGameObjectsWithTag("SlotCreator");


            foreach (GameObject slotCreator in slotCreators)
            {

                Transform slotTransform = slotCreator.transform;
                for (int i = 0; i < slotTransform.childCount; i++)
                {
                    Transform childTransform = slotTransform.GetChild(i);
                    if (childTransform.childCount > 0)
                    {
                        Destroy(childTransform.GetChild(0).gameObject);

                    }

                }
            }
        }

        runPanel.GetComponent<RunPanelControll>().EnablePlay();
    }


private void setResult()
    {
        StopCode();
        menu = Instantiate(resultScore);
        leftPanel.GetComponent<RollControll>().HideAndDisable();
        runPanel.GetComponent<RunPanelControll>().HideAndDisable();

    }

    public void setOpenResult()
    {
        GameObject myObject = GameObject.FindGameObjectWithTag("ResultScore");
        if (myObject == null)
        {
            setResult();
            menu.GetComponent<ResultScoreScript>().SetPause();
        }
        

    }


}
