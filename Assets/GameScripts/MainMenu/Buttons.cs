using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    [SerializeField] private GameObject selectLevel;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject instructions;
    [SerializeField] private TextMeshProUGUI buttronStartText;
    [SerializeField] private AudioSource music;

    private void Start()
    {

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        ChangeLocale();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }

    }



    public void StartGame()
    {

        SceneManager.LoadScene("Game");

    }
    public void SlectLevel()
    {

        Instantiate(selectLevel, transform.position, transform.rotation);
    }

    public void Settings()
    {

        GameObject settingsObject = Instantiate(settings, transform.position, transform.rotation);
        AudioSource[] audioSources = { music };
        settingsObject.GetComponent<SettingsMenu>().SetVolumeMusic(audioSources);

    }
    public void Instructions()
    {
        GameObject settingsObject = Instantiate(instructions, transform.position, transform.rotation);
    }

    public void ExitGame()
    {
        Application.Quit();
    }


    private void OnLocaleChanged(Locale newLocale)
    {
        ChangeLocale();
    }
    private void ChangeLocale()
    {
        string lastLevel = PlayerPrefs.GetString("LastLevel");
        if (!string.IsNullOrEmpty(lastLevel))
        {
            buttronStartText.text = LocalizationSettings.StringDatabase.GetLocalizedString("level") + " " + lastLevel.Substring(5);
        }
        else
        {
            buttronStartText.text = LocalizationSettings.StringDatabase.GetLocalizedString("newGame");
        }
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(PlayerPrefs.GetString("Language", "en"));
    }
}
