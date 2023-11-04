using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ResultScoreScript : MonoBehaviour
{

    [SerializeField] private GameObject settings;
    private int countStar = 1;
    private int moves = 0;
    private GameObject leftPanel;
    private GameObject runPanel;
    private string thisLevel;

    private void Awake()
    {
        thisLevel = PlayerPrefs.GetString("LastLevel");
    }
    private void Start()
    {
        leftPanel = GameObject.Find("LeftPanel");
        runPanel = GameObject.Find("RunPanel");
        
    }
    public void setWin() {
        
        var gridManagerScript = GameObject.Find("GridManager").GetComponent<GridManagerScript>();
        if (gridManagerScript.EvaluatedMove(moves)) { 
            countStar++;
        };
        if (gridManagerScript.EvaluatedCatchFly()) {
            countStar++;
        }
 
        Sprite success = Resources.Load<Sprite>("Graphic/StarSuccess");
        var image = gameObject.transform.Find("Image");
        var next = image.transform.Find("Next");


        for (int i = 0; i < 3; i++)
        {
        
            image.transform.Find("Star" + i).gameObject.SetActive(true);
            if (i < countStar) {
                Image starImage = image.transform.Find("Star" + i).GetComponent<Image>();
                starImage.sprite = success;             
            }
        }

        
        var level = gridManagerScript.getLevel();
        saveScore(level, countStar);

        int number;
        if (int.TryParse(level.Substring(5), out number))
        {
            number++; 
            string nextLevel = level.Substring(0, 5) + number.ToString(); // Vytvoøení nového øetìzce.
            if (FileController.existLevel(nextLevel))
            {
                PlayerPrefs.SetString("LastLevel", nextLevel);
                next.gameObject.SetActive(true);
            }
       
            
        }
    }


    public void startNextLevel() {        
       SceneManager.LoadScene("Game");
    }
    public void SetPause()
    {
        var image = gameObject.transform.Find("Image");
        var play = image.transform.Find("Play");
        play.gameObject.SetActive(true);
 
    }
    public void Play()
    {
        setPanelActive();
        Destroy(gameObject);

    }
    public void Reset()
    {
        PlayerPrefs.SetString("LastLevel", thisLevel);
        SceneManager.LoadScene("Game");
        
    }

    private void saveScore(string levelName, int score)
    {
        string dataPath = FileController.pathJsonDataLevel();

        if (File.Exists(dataPath))
        {
            string jsonData = File.ReadAllText(dataPath);
            List<LevelScore> levelDataList = JsonUtility.FromJson<LevelDataWrapper>(jsonData).levels;

            bool setOpen = false;

            foreach (LevelScore level in levelDataList)
            {

                if (setOpen) {
                    level.SetOpen(setOpen);
                    break;  
                    }
                if (level.GetLevel() == levelName)
                {
                    setOpen = true;
                    if (level.GetScore() < score) {
                        level.SetScore(score);
                    };                   
                }
            }

            FileController.WriteJsonData(JsonUtility.ToJson(new LevelDataWrapper(levelDataList)));

        }

    }

    public void setMoves(int moves) { 
            this.moves = moves;
    
    }

    public void EndGame()
    {
        SceneManager.LoadScene("Menu");
    }


    private void setPanelActive()
    {
 
        leftPanel.GetComponent<RollControll>().AllowScrol();
        runPanel.GetComponent<RunPanelControll>().AllowScrol();


    }

    public void Settings()
    {
        var music = GameObject.Find("GameBackground").GetComponents<AudioSource>();
        GameObject settingsObject = Instantiate(settings, transform.position, transform.rotation);
       // AudioSource[] audioSources = { music };
        settingsObject.GetComponent<SettingsMenu>().SetVolumeMusic(music);

    }
}
