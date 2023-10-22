using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InicializationSelectLevelMenuScript : MonoBehaviour
{
    private int startIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        SetSelectMenu();

    }

    private void SetSelectMenu() {

        bool updateList = false;
        string dataPath = Path.Combine(Application.persistentDataPath, "levelData.json");
        List<LevelScore> levelDataList = new List<LevelScore>();
       
        // Použijte Resources.LoadAll k naètení všech objektù ve složce
        Object[] levelFiles = Resources.LoadAll("LevelFiles", typeof(TextAsset));

        int countFiles = levelFiles.Length;      

        if (countFiles <= startIndex) {
            startIndex = 0;

        }
        if (startIndex < 0) {

            startIndex = countFiles - 10;
        }
        

        if (File.Exists(dataPath))
        {
            string jsonData = File.ReadAllText(dataPath);                    
            levelDataList = JsonUtility.FromJson<LevelDataWrapper>(jsonData).levels;
        }

        
        int levelButton = 0;
        
        for (int i = startIndex; i < startIndex + 10; i++)
        {
            bool open = false;
            LevelScore level;
            levelButton++;
            if (countFiles <= levelDataList.Count)
            {
                level = levelDataList[i];
                open = level.IsOpen();
            }
            else
            {
                Debug.Log(i);
                if (i == 0) { open = true; }
                level = new LevelScore("level" + (i+1), -1, open);
                levelDataList.Add(level);
                updateList = true;

            }


            Transform levelButtonObject = transform.Find("LevelsGrid/Level" + levelButton);
            if (levelButtonObject != null)
            {

                levelButtonObject.GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
                levelButtonObject.GetComponent<Button>().interactable = open;

                for (int star = 1; star < 4; star++)
                {
                    var starImage = levelButtonObject.Find("Star" + star);

                    if (level.score < 0)
                    {
                        starImage.gameObject.SetActive(false);
                    }
                    else {
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

        if (updateList)
        {
            string updatedJsonData = JsonUtility.ToJson(new LevelDataWrapper(levelDataList));
            try
            {
                File.WriteAllText(dataPath, updatedJsonData);
               
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

        }
       // Debug.Log("Test");
     // File.Delete(dataPath);
    }

    public void NextPage() {
        startIndex += 10;
        SetSelectMenu();
    }

    public void PreviousPage()
    {
        startIndex -= 10;
        SetSelectMenu();
    }

    public void BackMenu() {
        Destroy(gameObject);
    }


    public void HandleSelectLevel(TextMeshProUGUI textMeshPro)
    {
        PlayerPrefs.SetString("LastLevel","level"+textMeshPro.text);
        SceneManager.LoadScene("Game");
    }
}



