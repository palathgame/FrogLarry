using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InicializationSelectLevelMenuScript : MonoBehaviour
{
    private int startIndex = 0;
    List<LevelScore> levelDataList;
    private TextMeshProUGUI chapterText;
    private TextMeshProUGUI starsText;
    // Start is called before the first frame update
    void Start()
    {
        levelDataList = FileController.getLevelsData();
        CreateHeader();
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

            // Last page; it may be partly empty when the level count is not a multiple of 10.
            startIndex = ((countFiles - 1) / 10) * 10;
        }

        UpdateHeader(countFiles);

        int levelButton = 0;

        for (int i = startIndex; i < startIndex + 10; i++)
        {
            levelButton++;
            Transform levelButtonObject = transform.Find("LevelsGrid/Level" + levelButton);

            bool exists = i < countFiles && i < levelDataList.Count;
            if (levelButtonObject != null)
            {
                levelButtonObject.gameObject.SetActive(exists);
            }
            if (!exists)
            {
                continue;
            }

            var level = levelDataList[i];

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

    // Header above the buttons, on a plaque like the Back button: "Chapter N" (each page of 10
    // levels is a chapter) and all stars earned so far.
    private void CreateHeader()
    {
        var grid = transform.Find("LevelsGrid");
        var font = grid.GetComponentInChildren<TextMeshProUGUI>(true).font;
        var back = grid.Find("Back");

        var plaque = new GameObject("Header", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
        plaque.transform.SetParent(grid, false);
        plaque.sprite = back != null ? back.GetComponent<Image>().sprite : null;
        plaque.raycastTarget = false;
        var rect = plaque.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -85);
        rect.sizeDelta = new Vector2(1000, 115);

        chapterText = MakeText(rect, "Chapter", font, 72, new Vector2(-230, 0), new Vector2(480, 110));

        var icon = new GameObject("Star", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
        icon.transform.SetParent(rect, false);
        icon.sprite = Resources.Load<Sprite>("Graphic/StarScore");
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        icon.rectTransform.anchoredPosition = new Vector2(130, 0);
        icon.rectTransform.sizeDelta = new Vector2(80, 80);
        starsText = MakeText(rect, "Stars", font, 66, new Vector2(320, 0), new Vector2(300, 110));
        starsText.alignment = TextAlignmentOptions.Left;
    }

    private void UpdateHeader(int countFiles)
    {
        if (chapterText == null) return;
        string format = LocalizationSettings.StringDatabase.GetLocalizedString("chapter");
        if (string.IsNullOrEmpty(format) || format.StartsWith("No translation")) format = "Chapter {0}";
        chapterText.text = string.Format(format, startIndex / 10 + 1);

        int earned = 0;
        for (int i = 0; i < countFiles && i < levelDataList.Count; i++)
        {
            if (levelDataList[i].score > 0) earned += levelDataList[i].score;
        }
        starsText.text = earned + " / " + countFiles * 3;
    }

    private static TextMeshProUGUI MakeText(Transform parent, string name, TMP_FontAsset font, float size, Vector2 position, Vector2 boxSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(0.27f, 0.17f, 0.08f);
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        var rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = boxSize;
        return text;
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



