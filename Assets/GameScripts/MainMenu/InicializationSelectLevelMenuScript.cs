using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InicializationSelectLevelMenuScript : MonoBehaviour
{
    private int startIndex = 0;
    List<LevelScore> levelDataList;
    // Start is called before the first frame update
    void Start()
    {
        levelDataList = FileController.getLevelsData();
        SetSelectMenu();

    }

    private void SetSelectMenu()
    {


        int countFiles = FileController.getCountLevels();

        if (countFiles <= startIndex)
        {
            startIndex = 0;

        }
        if (startIndex < 0)
        {

            startIndex = countFiles - 10;
        }

        int levelButton = 0;

        for (int i = startIndex; i < startIndex + 10; i++)
        {

            var level = levelDataList[i]; 
            levelButton++;


            Transform levelButtonObject = transform.Find("LevelsGrid/Level" + levelButton);
            if (levelButtonObject != null)
            {

                levelButtonObject.GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
                levelButtonObject.GetComponent<Button>().interactable = level.open;

                for (int star = 1; star < 4; star++)
                {
                    var starImage = levelButtonObject.Find("Star" + star);

                    if (level.score < 0)
                    {
                        starImage.gameObject.SetActive(false);
                    }
                    else
                    {
                        starImage.gameObject.SetActive(true);
                        var StarScore = Resources.Load<Sprite>("Graphic/StarScore");
                        var StarScoreF = Resources.Load<Sprite>("Graphic/StarScoreF");

                        if (level.score >= star)
                        {
                            starImage.GetComponent<Image>().sprite = StarScore;
                        }
                        else
                        {
                            starImage.GetComponent<Image>().sprite = StarScoreF;
                        }
                    }
                }

            }
        }
    }

    public void NextPage()
    {
        startIndex += 10;
        SetSelectMenu();
    }

    public void PreviousPage()
    {
        startIndex -= 10;
        SetSelectMenu();
    }

    public void BackMenu()
    {
        Destroy(gameObject);
    }


    public void HandleSelectLevel(TextMeshProUGUI textMeshPro)
    {
        PlayerPrefs.SetString("LastLevel", "level" + textMeshPro.text);
        SceneManager.LoadScene("Game");
    }



}



