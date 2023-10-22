using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class Localization
{
    private Dictionary<string, string> languageDictionary = new Dictionary<string, string>
    {
        { "en", "English" },
        { "fr", "Français" },
        { "cs", "Èesky" },
        { "de", "Deutsch" }
    };

    public string GetLanguageValue(string key)
    {
        if (languageDictionary.ContainsKey(key))
        {
            return languageDictionary[key];
        }
        else
        {

            return languageDictionary["en"];
        }
    }

    public string GetNextLanguageKey(string currentKey, bool ascending)
{
        var sortedKeys = ascending
            ? languageDictionary.Keys.OrderBy(k => k).ToList()
            : languageDictionary.Keys.OrderByDescending(k => k).ToList();

        int currentIndex = sortedKeys.IndexOf(currentKey);

        string keyLocalization;
        if (currentIndex >= 0 && currentIndex < sortedKeys.Count - 1)
        {
            // Pokud aktuální klíè není poslední v seznamu, vra následující klíè.
            keyLocalization = sortedKeys[currentIndex + 1];
        }
        else
        {
            // Pokud aktuální klíè neexistuje nebo je poslední v seznamu, vra první klíè.
            keyLocalization = sortedKeys.FirstOrDefault();
        }

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(keyLocalization);
        PlayerPrefs.SetString("Language", keyLocalization);
        return keyLocalization;
    }
}