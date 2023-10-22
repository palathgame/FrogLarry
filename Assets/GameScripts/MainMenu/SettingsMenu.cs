using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    [SerializeField] private Slider music;
    [SerializeField] private Slider sounds;
    [SerializeField] private TextMeshProUGUI language;

    private float loadedSounds;
    private float loadedMusic;
    string keyLanguage;
    private Localization localization = new Localization();
    private AudioSource[] audioSourcesMusic;
    private AudioSource[] audioSourcesSounds;
    // Start is called before the first frame update
    void Start()
    {
        loadedSounds = PlayerPrefs.GetFloat("GameSounds", 1.0f); // Defaultní hodnota je 1.0f (100%).
        loadedMusic = PlayerPrefs.GetFloat("GameMusic", 1.0f); // Defaultní hodnota je 1.0f (100%).
        keyLanguage = PlayerPrefs.GetString("Language", "en"); // Defaultní hodnota je 1.0f (100%).
        language.text = localization.GetLanguageValue(keyLanguage);
        music.value = loadedMusic;   
        music.onValueChanged.AddListener(OnMusicVolumeChanged);
        sounds.value = loadedSounds;
        sounds.onValueChanged.AddListener(OnSoundsVolumeChanged);

    }

    private void OnMusicVolumeChanged(float newVolume)
    {
        // Porovnáme novou hodnotu s pøedchozí hodnotou.
        if (newVolume != loadedMusic)
        {
            // Aktualizujeme pøedchozí hodnotu na novou hodnotu.
            loadedMusic = newVolume;
            if (audioSourcesMusic != null && audioSourcesMusic.Length > 0)
            {
                setVolume(audioSourcesMusic, loadedMusic);
            }
        }
    }
    private void OnSoundsVolumeChanged(float newVolume)
    {
        // Porovnáme novou hodnotu s pøedchozí hodnotou.
        if (newVolume != loadedSounds)
        {
            // Aktualizujeme pøedchozí hodnotu na novou hodnotu.
            loadedSounds = newVolume;
            if (audioSourcesSounds != null && audioSourcesSounds.Length > 0)
            {
                setVolume(audioSourcesSounds, loadedSounds);
            }
        }
    }

    public void NextLanguage()
    {
        setLanguage(true);
    }

    public void PreviousLanguage()
    {
        setLanguage(false);
    }

    private void setLanguage(bool direction) {

        keyLanguage = localization.GetNextLanguageKey(keyLanguage, direction);
        language.text = localization.GetLanguageValue(keyLanguage);
    }
    public void BackMenu()
    {
        PlayerPrefs.SetFloat("GameSounds", loadedSounds); // Defaultní hodnota je 1.0f (100%).
        PlayerPrefs.SetFloat("GameMusic", loadedMusic); // Defaultní hodnota je 1.0f (100%).
        PlayerPrefs.SetString("Language", keyLanguage);

        Destroy(gameObject);
    }


    public void SetVolumeMusic(AudioSource[] audioSourcesMusic) {

        this.audioSourcesMusic = audioSourcesMusic;

    } 
    public void SetVolumeSounds(AudioSource[] audioSourcesSounds) {

        this.audioSourcesSounds = audioSourcesSounds;

    }

    private void setVolume( AudioSource[] audioSources, float volume)
    {
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.volume = volume; // Nastavíme hlasitost pro každou komponentu.
        }

    }


}
