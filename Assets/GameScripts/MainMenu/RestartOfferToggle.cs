using TMPro;
using UnityEngine;
using UnityEngine.UI;

// On the Settings prefab: an on/off row for the "back to the start?" offer after a run
// that did not reach the pond (see LevelResetScript.OfferEnabled). Saved immediately.
public class RestartOfferToggle : MonoBehaviour
{
    [SerializeField] private RectTransform parent;  // the settings Canvas holding the other rows
    [SerializeField] private Sprite buttonSprite;   // Graphic/ButtonGrafic/Menu
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private float y = -900f;       // same anchoring as the other rows (top centre)

    private TextMeshProUGUI state;

    private void Start()
    {
        var label = MakeText(parent, LevelResetScript.Localized("settings_restart", "Back to start"), 82);
        label.alignment = TextAlignmentOptions.Left;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        Place(label.rectTransform, new Vector2(-250, y), new Vector2(900, 130)); // left edge in line with the slider labels

        var go = new GameObject("RestartOffer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        Place(rect, new Vector2(150, y), new Vector2(420, 150)); // right edge in line with the sliders
        go.GetComponent<Image>().sprite = buttonSprite;
        go.GetComponent<Button>().onClick.AddListener(Toggle);
        state = MakeText(rect, "", 80);
        state.rectTransform.anchorMin = Vector2.zero;
        state.rectTransform.anchorMax = Vector2.one;
        state.rectTransform.offsetMin = state.rectTransform.offsetMax = Vector2.zero;
        Refresh();
    }

    private void Toggle()
    {
        LevelResetScript.OfferEnabled = !LevelResetScript.OfferEnabled;
        Refresh();
    }

    private void Refresh()
    {
        state.text = LevelResetScript.OfferEnabled
            ? LevelResetScript.Localized("on", "On")
            : LevelResetScript.Localized("off", "Off");
    }

    private TextMeshProUGUI MakeText(Transform parentTransform, string value, float size)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(parentTransform, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(0.17f, 0.17f, 0.2f);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static void Place(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
