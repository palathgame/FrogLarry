using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

// On GridManager. When a run ends without reaching the pond, offers to put Larry back
// on the start: the frog, the flies and the move/fly counters reset, the blocks in the
// slots stay. "Continue" keeps playing from where the frog stands.
// The offer can be turned off in Settings (PlayerPrefs "OfferRestart", see OfferEnabled).
public class LevelResetScript : MonoBehaviour
{
    public const string OfferKey = "OfferRestart";
    public static bool OfferEnabled
    {
        get => PlayerPrefs.GetInt(OfferKey, 1) == 1;
        set { PlayerPrefs.SetInt(OfferKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    [SerializeField] private Canvas canvas;        // Controll
    [SerializeField] private Sprite panelSprite;   // Graphic/Other/Bubble
    [SerializeField] private Sprite buttonSprite;  // Graphic/ButtonGrafic/Menu
    [SerializeField] private TMP_FontAsset font;

    private struct Placed
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    private Placed frogStart;
    private readonly List<Placed> flies = new List<Placed>();
    private GameObject flyPrefab;
    private GameObject offer;
    private Button playButton;

    // The "To start" button while the offer is shown (the tutorial points its arrow at it).
    public RectTransform StartButton { get; private set; }

    // How many times the level went back to the start (the tutorial hides its retry hint after one).
    public int Resets { get; private set; }

    private IEnumerator Start()
    {
        flyPrefab = Resources.Load<GameObject>("Prefabs/Fly");
        var play = GameObject.Find("Controll/RunPanel/Play");
        if (play != null) playButton = play.GetComponent<Button>();
        yield return null; // GridManagerScript builds the level in its Start
        var frog = GameObject.FindGameObjectWithTag("Frog");
        if (frog != null) frogStart = new Placed { position = frog.transform.position, rotation = frog.transform.rotation };
        foreach (var fly in GameObject.FindGameObjectsWithTag("Fly"))
        {
            flies.Add(new Placed { position = fly.transform.position, rotation = fly.transform.rotation });
        }
    }

    private void Update()
    {
        // Pressing Play again means the player chose to continue.
        if (offer != null && playButton != null && !playButton.enabled) HideOffer();
    }

    // Called by PlayScript when the commands ran out and the frog is not in the pond.
    public void OnRunEnded()
    {
        if (OfferEnabled && offer == null) ShowOffer();
    }

    public void ResetLevel()
    {
        HideOffer();
        Resets++;
        var frog = GameObject.FindGameObjectWithTag("Frog");
        if (frog != null)
        {
            frog.transform.SetPositionAndRotation(frogStart.position, frogStart.rotation);
            var animator = frog.GetComponent<Animator>();
            if (animator != null) { animator.Rebind(); animator.Update(0); }
            var look = frog.GetComponent<FrogLookScript>();
            if (look != null) look.ShowArrow();
        }

        foreach (var fly in GameObject.FindGameObjectsWithTag("Fly")) Destroy(fly);
        if (flyPrefab != null)
        {
            foreach (var fly in flies) Instantiate(flyPrefab, fly.position, fly.rotation);
        }

        GetComponent<GridManagerScript>().ResetCatchFly();
        var play = FindAnyObjectByType<PlayScript>();
        if (play != null) play.ResetMoves();
    }

    private void ShowOffer()
    {
        offer = new GameObject("RestartOffer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rect = (RectTransform)offer.transform;
        rect.SetParent(canvas.transform, false);
        // Low on the board, just above the slots, so the frog stays visible.
        rect.anchorMin = rect.anchorMax = new Vector2(0.6f, 0.4f);
        rect.sizeDelta = new Vector2(940, 340);
        var image = offer.GetComponent<Image>();
        image.sprite = panelSprite;

        var text = MakeText(rect, Localized("restart_question", "Larry didn't reach the pond. Back to the start?"), 52);
        text.rectTransform.anchorMin = new Vector2(0, 0.5f);
        text.rectTransform.anchorMax = new Vector2(1, 1);
        text.rectTransform.offsetMin = new Vector2(150, 0);
        text.rectTransform.offsetMax = new Vector2(-150, -45);
        text.enableAutoSizing = true;
        text.fontSizeMin = 28;
        text.fontSizeMax = 46;

        StartButton = MakeButton(rect, Localized("restart_yes", "To start"), new Vector2(-180, 95), ResetLevel);
        MakeButton(rect, Localized("restart_no", "Continue"), new Vector2(180, 95), HideOffer);
    }

    private void HideOffer()
    {
        if (offer != null) Destroy(offer);
        offer = null;
        StartButton = null;
    }

    private RectTransform MakeButton(RectTransform parent, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(330, 100);
        go.GetComponent<Image>().sprite = buttonSprite;
        go.GetComponent<Button>().onClick.AddListener(onClick);
        var text = MakeText(rect, label, 46);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
        return rect;
    }

    private TextMeshProUGUI MakeText(RectTransform parent, string value, float size)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = new Color(0.27f, 0.17f, 0.08f);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    public static string Localized(string key, string fallback)
    {
        var value = LocalizationSettings.StringDatabase.GetLocalizedString(key);
        return string.IsNullOrEmpty(value) || value.StartsWith("No translation") ? fallback : value;
    }
}
