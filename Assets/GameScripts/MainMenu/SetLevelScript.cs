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
        levelDataList = MigrateMovedLevel(levelDataList);

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

        UnlockAfterWins(levelDataList);


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

    // A level after a won one is open. Levels added in an update come after the last won level,
    // and the game only unlocks the next level at the moment of a win, so open them here.
    void UnlockAfterWins(List<LevelScore> levelDataList)
    {
        bool changed = false;
        for (int i = 1; i < levelDataList.Count; i++)
        {
            if (!levelDataList[i].open && levelDataList[i - 1].score > 0)
            {
                levelDataList[i].open = true;
                changed = true;
            }
        }
        if (changed)
        {
            FileController.WriteJsonData(JsonUtility.ToJson(new LevelDataWrapper(levelDataList)));
        }
    }

    // Version 3 moved the old level18 (too hard that early) to level25; old level19..25 became level18..24.
    // Scores follow their maps; unlocking stays by position, plus every level that already has a score.
    private const int MOVED_LEVEL_VERSION = 3;
    private const int MOVED_FROM = 18;
    private const int MOVED_TO = 25;

    List<LevelScore> MigrateMovedLevel(List<LevelScore> levelDataList)
    {
        if (PlayerPrefs.GetInt("LevelLayoutVersion", 1) >= MOVED_LEVEL_VERSION)
        {
            return levelDataList;
        }

        if (levelDataList.Count >= MOVED_TO)
        {
            var oldScores = new Dictionary<string, int>();
            foreach (var level in levelDataList)
            {
                oldScores[level.level] = level.score;
            }
            foreach (var level in levelDataList)
            {
                string oldName = OldLevelName(level.level);
                if (oldName != level.level && oldScores.TryGetValue(oldName, out int score))
                {
                    level.score = score;
                    level.open = level.open || score >= 0;
                }
            }
            FileController.WriteJsonData(JsonUtility.ToJson(new LevelDataWrapper(levelDataList)));
        }

        string lastLevel = PlayerPrefs.GetString("LastLevel");
        if (!string.IsNullOrEmpty(lastLevel))
        {
            PlayerPrefs.SetString("LastLevel", NewLevelName(lastLevel));
        }

        PlayerPrefs.SetInt("LevelLayoutVersion", MOVED_LEVEL_VERSION);
        PlayerPrefs.Save();
        return levelDataList;
    }

    // Which old level now sits at this name.
    private static string OldLevelName(string levelName)
    {
        if (!TryLevelNumber(levelName, out int n)) return levelName;
        if (n == MOVED_TO) return "level" + MOVED_FROM;
        if (n >= MOVED_FROM && n < MOVED_TO) return "level" + (n + 1);
        return levelName;
    }

    // Where an old level went.
    private static string NewLevelName(string levelName)
    {
        if (!TryLevelNumber(levelName, out int n)) return levelName;
        if (n == MOVED_FROM) return "level" + MOVED_TO;
        if (n > MOVED_FROM && n <= MOVED_TO) return "level" + (n - 1);
        return levelName;
    }

    private static bool TryLevelNumber(string levelName, out int number)
    {
        number = 0;
        return levelName != null && levelName.StartsWith("level") && int.TryParse(levelName.Substring(5), out number);
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
