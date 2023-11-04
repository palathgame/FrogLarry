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
