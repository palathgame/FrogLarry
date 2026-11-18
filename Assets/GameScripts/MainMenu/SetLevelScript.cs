using System.Collections.Generic;
using UnityEngine;

public class SetLevelScript : MonoBehaviour
{
    private void Awake()
    {
       SetLevels();
       //File.Delete(FileController.pathJsonDataLevel());
    }

    private void SetLevels() {

        var levelDataList = FileController.getLevelsData();
        var countLevels = FileController.getCountLevels();
        var testDuplicate =  PlayerPrefs.GetInt("RemoveDuplicateLevels", 0);

        if (levelDataList.Count > 0 && testDuplicate == 0) { 
            levelDataList = RemoveDuplicateLevels(levelDataList);
        }

        PlayerPrefs.SetInt("RemoveDuplicateLevels", 1);

        levelDataList = MigrateTutorialLevels(levelDataList);

        if (levelDataList.Count < countLevels) {

            for (int i = levelDataList.Count; i < countLevels; i++)
            {
                var open = false;
                if (i == 0) {
                   open = true;
                }
                var level = new LevelScore("level" + (i + 1), -1, open);
                levelDataList.Add(level);

            }

           // Debug.Log(JsonUtility.ToJson(new LevelDataWrapper(levelDataList)));
            FileController.WriteJsonData(JsonUtility.ToJson(new LevelDataWrapper(levelDataList)));

        }


    }

    // Version 2 inserted TUTORIAL_LEVELS new levels before the original level1,
    // so saved progress and LastLevel from older versions are shifted to match.
    private const int TUTORIAL_LEVELS = 5;
    private const int LEVEL_LAYOUT_VERSION = 2;

    List<LevelScore> MigrateTutorialLevels(List<LevelScore> levelDataList)
    {
        if (PlayerPrefs.GetInt("LevelLayoutVersion", 1) >= LEVEL_LAYOUT_VERSION)
        {
            return levelDataList;
        }

        if (levelDataList.Count > 0)
        {
            foreach (var level in levelDataList)
            {
                level.level = ShiftLevelName(level.level);
            }

            List<LevelScore> migrated = new List<LevelScore>();
            for (int i = 0; i < TUTORIAL_LEVELS; i++)
            {
                migrated.Add(new LevelScore("level" + (i + 1), -1, true));
            }
            migrated.AddRange(levelDataList);
            levelDataList = migrated;
            FileController.WriteJsonData(JsonUtility.ToJson(new LevelDataWrapper(levelDataList)));
        }

        string lastLevel = PlayerPrefs.GetString("LastLevel");
        if (!string.IsNullOrEmpty(lastLevel))
        {
            PlayerPrefs.SetString("LastLevel", ShiftLevelName(lastLevel));
        }

        PlayerPrefs.SetInt("LevelLayoutVersion", LEVEL_LAYOUT_VERSION);
        PlayerPrefs.Save();
        return levelDataList;
    }

    private string ShiftLevelName(string levelName)
    {
        if (levelName != null && levelName.StartsWith("level") && int.TryParse(levelName.Substring(5), out int number))
        {
            return "level" + (number + TUTORIAL_LEVELS);
        }
        return levelName;
    }

    List<LevelScore> RemoveDuplicateLevels(List<LevelScore> levelDataList)
    {
        List<LevelScore> uniqueLevels = new List<LevelScore>();

        for (int i = 0; i < levelDataList.Count; i++)
        {
            if (i < 10)
            {
                uniqueLevels.Add(levelDataList[i]);
            }
            else
            {
                bool add = true;
                foreach (var uni in uniqueLevels)
                {
                    if (uni.level == levelDataList[i].level)
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                {
                    uniqueLevels.Add(levelDataList[i]);
                }

            }
        }
        return uniqueLevels;
    }
}
