using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    [SerializeField] private AudioSource gameMusic; // Pøetáhnìte Audio Source komponentu do tohoto pole v inspektorovi.

    void Start()
    {
        // Získejte uloženou hlasitost z PlayerPrefs nebo použijte výchozí hodnotu 1.0f.
        float savedVolume = PlayerPrefs.GetFloat("GameMusic", 1.0f);
        // Nastavte hlasitost Audio Source komponenty.
        gameMusic.volume = savedVolume;
    }




}
