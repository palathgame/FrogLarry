using UnityEngine;

public class GameAudioScript : MonoBehaviour
{
   [SerializeField] private AudioSource audioSource; // Pøetáhnìte sem svou komponentu AudioSource.
    void Start()
    {
        float loadedVolume = PlayerPrefs.GetFloat("SavedVolume", 1.0f);
        audioSource.volume = loadedVolume;
        int randomNumber = UnityEngine.Random.Range(1, 8);
        audioSource.clip = Resources.Load<AudioClip>("Sounds/Song"+ randomNumber);
        audioSource.loop = true;
        audioSource.volume = PlayerPrefs.GetFloat("GameMusic", 1.0f); ;
        audioSource.Play();
    }

}
