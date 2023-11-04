using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileController 
{

    public static List<LevelScore> getLevelsData() {

        string dataPath = pathJsonDataLevel();
        List<LevelScore> levelDataList = new List<LevelScore>();
        if (File.Exists(dataPath))
        {
            string jsonData = File.ReadAllText(dataPath);
            levelDataList = JsonUtility.FromJson<LevelDataWrapper>(jsonData).levels;
        }

        return levelDataList;
    }

    public static int getCountLevels()
    {
        Object[] levelFiles = Resources.LoadAll("LevelFiles", typeof(TextAsset));

        return levelFiles.Length;

    }

    public static void SaveLevelData(List<LevelScore> levelDataList) {
       
        string updatedJsonData = JsonUtility.ToJson(new LevelDataWrapper(levelDataList));
        try
        {
            WriteJsonData(updatedJsonData);

        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    public static bool newLevelsAdded() {
        
        return getLevelsData().Count < getCountLevels();
    }
    public static string pathJsonDataLevel() {

        return Path.Combine(Application.persistentDataPath, "levelData.json");
    }

    public static void WriteJsonData(string updatedJsonData) {

        File.WriteAllText(pathJsonDataLevel(), updatedJsonData);

    }

    public static bool existLevel(string nameLevel) {

        return Resources.Load("LevelFiles/" + nameLevel, typeof(TextAsset)) != null ;
    }

}
