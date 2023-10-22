using System.Collections.Generic;

[System.Serializable]
public class LevelScore
{
    public string level;
    public int score;
    public bool open;  

    public LevelScore(string level, int score, bool open)
    {
        this.level = level;
        this.score = score;
        this.open = open;
    }

    // Getter pro level
    public string GetLevel()
    {
        return level;
    }

    // Getter pro score
    public int GetScore()
    {   
        return score;
    }

    // Getter pro open
    public bool IsOpen()
    {
        return open;
    }

    public void SetScore(int score) { 
        this.score = score;
           
    }
    public void SetOpen(bool open)
    {
        this.open = open;
    }
    // Setter pro všechny tøi hodnoty najednou
    public void SetValues(string level, int score, bool open)
    {
        this.level = level;
        this.score = score;
        this.open = open;
    }
}

[System.Serializable]
public class LevelDataWrapper
{
    public List<LevelScore> levels;

    public LevelDataWrapper(List<LevelScore> levels)
    {
        this.levels = levels;
    }
}