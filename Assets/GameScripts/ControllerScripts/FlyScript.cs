using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyScript : MonoBehaviour
{
    private Animator flyAnimator;
    private bool flyRunning = false;
    private bool isDead = false;
    public AudioSource[] audioSources;
    private void Start()
    {
        flyAnimator = GetComponent<Animator>();
        StartCoroutine(RandomAnimationCoroutine());
        audioSources = GetComponents<AudioSource>();


    }

    private IEnumerator RandomAnimationCoroutine()
    {
        
        while (!flyRunning)
        {
            int randomAnimationIndex = Random.Range(0, 3);
            string[] animationTriggers = { "HandShake", "Move", "BodyMove" };
            string randomTrigger = animationTriggers[randomAnimationIndex];

            flyAnimator.SetTrigger(randomTrigger);

            float animationDuration = flyAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationDuration);

            yield return new WaitForSeconds(Random.Range(3f, 7f)); // Èekáme náhodnì mezi 1 a 3 sekundami
        }
    }

    public void Api(string flyComand) { 
        if(flyComand == "Escape") { 
            StartCoroutine(EscapeAndFly());
        }
        else if (flyComand == "Die"){
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        audioSources[1].volume = PlayerPrefs.GetFloat("GameSounds", 1.0f);
        audioSources[1].Play();
        isDead = true;
        float animationDuration = flyAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationDuration*0.45f); // Poèkáme na dokonèení animace
       // StartCoroutine(FlytWithAnimation(1, "Move"));          
        Destroy(gameObject); // Znièení mouchy
            
        
    }

    private IEnumerator EscapeAndFly()
    {
        audioSources[0].volume = PlayerPrefs.GetFloat("GameSounds", 1.0f);
        audioSources[0].Play();
        flyRunning = true;
        while (flyRunning && !isDead)
        {
            StartCoroutine(FlytWithAnimation(3, "Flying"));
            yield return new WaitForSeconds(0.1f); // Krátká pauza mezi jednotlivými lety

            // Získání pozice mouchy ve viewport souøadnicích
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(transform.position);

            // Pokud je moucha mimo zorné pole kamery, ukonèit let
            if (viewportPosition.x < 0 || viewportPosition.x > 1 || viewportPosition.y < 0 || viewportPosition.y > 1)
            {
                flyRunning = false;
                Destroy(gameObject); // Znièení mouchy
            }
        }
    }
    private IEnumerator FlytWithAnimation(int gridSize, string animation)
    {
        
        Vector3 direction = GetDirectionFromRotation(transform); // Lokální smìr, mùžete zmìnit podle potøeby

        Vector3 newPosition = transform.position + direction * gridSize;

      
            float duration = flyAnimator.GetCurrentAnimatorStateInfo(0).length;
            float elapsedTime = 0;
            Vector3 startingPosition = transform.position;
            flyAnimator.SetTrigger(animation);
            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startingPosition, newPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
      
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

}
