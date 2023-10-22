
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class GridManagerScript : MonoBehaviour
{

    [SerializeField] private Tilemap tilemap;          // Komponenta Tilemap pro uchování dlaždic
    [SerializeField] private TileBase tilePrefab;      // Pøeddefinovaná dlaždice pro použití na herním poli
    //[SerializeField] private Vector3 startPosition = new Vector3(0, 0, 0); // Poèáteèní pozice mapy
    [SerializeField] private Camera cam;
    private int maxMove = 0;
    private int catchMax = 0;
    private int catchFly = 0;
    private string level;

    private void Start()
    {
        level = PlayerPrefs.GetString("LastLevel");
        if (string.IsNullOrEmpty(level))
        {
            level = "level1";
            PlayerPrefs.SetString("LastLevel", "level1");
            PlayerPrefs.Save();
        }
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        string levelData = LoadLevelData(level);


        // Rozdìlení textového souboru na øádky

        string[] lines = levelData.Split('\n');
  
        // Rozdìlení každého øádku na slova (oddìlená mezerami)
        List<string[]> wordsInLines = new List<string[]>();

        foreach (string line in lines)
        {
            string[] words = line.Split('\t');
            wordsInLines.Add(words);
        }

        int y = 0;
        int x = 0;
        wordsInLines.Reverse();
        // Vypsání obsahu souboru na konzoli
        foreach (string[] words in wordsInLines)
        {

            foreach (string word in words)
            {
               
                Vector3Int tilePosition = new Vector3Int(x, y, 0);     // Vytvoøení pozice dlaždice pomocí souøadnic
                tilemap.SetTile(tilePosition, tilePrefab);             // Nastavení dlaždice na danou pozici v Tilemap

                //Debug.Log(word);
                MadeGenerateObject(word,tilePosition);

                x++;
            }
            y++;
            x=0;
        }

       // tilemap.transform.position = startPosition;

      //  Vector3 center = tilemap.localBounds.center;
       // tilemap.transform.TransformPoint(center);

        cam.GetComponent<CameraController>().CenterCam();
    }



    private string LoadLevelData(string levelName)
    {
        // Název souboru vèetnì cesty zaèínající od složky Resources (bez pøípony)
        string levelFilePath = "LevelFiles/" + levelName;
        // Naètení textových dat ze souboru
        TextAsset textAsset = Resources.Load<TextAsset>(levelFilePath);

        if (textAsset != null)
        {
            return textAsset.text;
        }
        else
        {
            return "FF";
            // Pokud se soubor nenalezl
        }
    }
    
    private void MadeGenerateObject(string shortCut, Vector3Int tilePosition) {
        string nameGenerateObject;
        Dictionary<char, string> translator = new Dictionary<char, string>();
        translator['T'] = "Tree";
        translator['R'] = "Frog";
        translator['F'] = "Fly";
        translator['P'] = "Pond";
        translator['S'] = "Stone";
        translator['N'] = "None";
        nameGenerateObject = translator[shortCut[0]];


        if (shortCut[0] == 'N')
        {
            maxMove = maxMove + int.Parse(shortCut[1].ToString());
        }
        else if (shortCut[0] == 'F')
        {

            if (shortCut != "FF")
            {
                catchMax++;
                maxMove += int.Parse(shortCut[1].ToString());
            }
        }
        else if (shortCut[0] == 'S')
        {

            maxMove = maxMove + int.Parse(shortCut[1].ToString());
            nameGenerateObject += shortCut[2];
        }
        
        else if (int.TryParse(shortCut[1].ToString(), out _))
        {

            nameGenerateObject += shortCut[1];
        }






        if (nameGenerateObject != "None")
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + nameGenerateObject);
            if (prefab != null)
            {
                float rotationAngle = 0f;

                if ((nameGenerateObject == "Pond" && shortCut[1] == 'V') || (nameGenerateObject == "Frog" && shortCut[1] == 'L')) {
                    rotationAngle = 90.0f;
                } else if (nameGenerateObject == "Frog" && shortCut[1] == 'R')
                {
                    rotationAngle = - 90.0f;


                }else if (nameGenerateObject == "Frog" && shortCut[1] == 'B')
                {
                    rotationAngle = 180.0f;


                }


                // Vytvoøte Quaternion pro rotaci o daný úhel kolem osy z
                Quaternion rotation = Quaternion.Euler(0, 0, rotationAngle);
                Vector3 prefabPosition = tilemap.GetCellCenterWorld(tilePosition);
                Instantiate(prefab, prefabPosition, rotation);
            }
            else
            {
                Debug.LogWarning("Prefab pro generovany objekt" + nameGenerateObject + " nebyl nalezen.");
            }
        }
    }
    public bool EvaluatedMove(int moveCount) {
        if (moveCount > maxMove)
        {
            return false;
        }
        return true;
    }

    public bool EvaluatedCatchFly()
    {
        if (catchFly < catchMax)
        {
            return false;
        }
        return true;
    }

    public void addCatchFly() { 
    
           catchFly++;
    
    }
    public int getCatchFly() { 
        return catchFly;
    }

    public string getLevel() {
    
        return level;
    }
}
