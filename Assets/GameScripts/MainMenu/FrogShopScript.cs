using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

// Menu shop where eaten flies buy frog colours (see FrogSkins). On the Menu object;
// the "Shop" button calls Open(). The screen is built in code so no prefab is needed.
public class FrogShopScript : MonoBehaviour
{
    [SerializeField] private Sprite background;   // Graphic/Other/BackgroudMenu
    [SerializeField] private Sprite buttonSprite; // Graphic/ButtonGrafic/Menu
    [SerializeField] private TMP_FontAsset font;  // SourceSansPro-Bold SDF

    private static readonly Color TextColor = new Color(0.27f, 0.17f, 0.08f);
    private static readonly Color CardColor = new Color(0.25f, 0.15f, 0.05f, 0.55f);
    private static readonly Color SelectedColor = new Color(1f, 0.85f, 0.2f, 1f);

    private GameObject root;
    private TextMeshProUGUI walletText;
    private Sprite flyIcon;

    public void Open()
    {
        if (root != null) return;
        flyIcon = FrogSkins.LoadIcon("FlyIcon");

        root = new GameObject("FrogShop", typeof(RectTransform));
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        var bg = MakeImage(root.transform, "Background", background, Color.white);
        Stretch(bg.rectTransform);

        var plaque = MakeImage(root.transform, "Title", buttonSprite, Color.white);
        Place(plaque.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -165), new Vector2(560, 125));
        var title = MakeText(plaque.transform, "Text", Localized("shop", "Frogs"), 80);
        Stretch(title.rectTransform);

        // Wallet: fly icon + count, top right.
        var wallet = MakeImage(root.transform, "Wallet", buttonSprite, Color.white);
        Place(wallet.rectTransform, new Vector2(1, 1), new Vector2(-330, -165), new Vector2(300, 115));
        var walletIcon = MakeImage(wallet.transform, "Fly", flyIcon, Color.white);
        walletIcon.preserveAspect = true;
        Place(walletIcon.rectTransform, new Vector2(0, 0.5f), new Vector2(70, 0), new Vector2(95, 95));
        walletText = MakeText(wallet.transform, "Count", "", 72);
        Place(walletText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(45, 0), new Vector2(200, 110));

        var grid = new GameObject("Grid", typeof(RectTransform)).GetComponent<RectTransform>();
        grid.SetParent(root.transform, false);
        Place(grid, new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1400, 620));
        var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(320, 295);
        layout.spacing = new Vector2(30, 30);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 4;

        var back = MakeButton(root.transform, "Back", Localized("Back", "Back"), 72, Close);
        Place((RectTransform)back.transform, new Vector2(0.5f, 0), new Vector2(0, 100), new Vector2(600, 140));

        Refresh(grid);
    }

    public void Close()
    {
        if (root != null) Destroy(root);
        root = null;
    }

    private void Refresh(RectTransform grid)
    {
        foreach (Transform child in grid) Destroy(child.gameObject);
        walletText.text = FrogSkins.Flies.ToString();

        string selected = FrogSkins.Selected;
        foreach (var skin in FrogSkins.All)
        {
            bool owned = FrogSkins.IsOwned(skin.id);
            bool isSelected = skin.id == selected;

            var card = MakeImage(grid, skin.id, null, CardColor);
            if (isSelected)
            {
                var outline = card.gameObject.AddComponent<Outline>();
                outline.effectColor = SelectedColor;
                outline.effectDistance = new Vector2(8, -8);
                card.color = new Color(1f, 0.9f, 0.5f, 0.45f);
            }

            var icon = MakeImage(card.transform, "Icon", FrogSkins.LoadIcon(skin.id), owned ? Color.white : new Color(1, 1, 1, 0.55f));
            icon.preserveAspect = true;
            Place(icon.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -105), new Vector2(190, 190));

            var s = skin;
            Button button;
            if (isSelected)
            {
                button = MakeButton(card.transform, "Action", Localized("shop_selected", "Selected"), 44, null);
                button.interactable = false;
            }
            else if (owned)
            {
                button = MakeButton(card.transform, "Action", Localized("shop_select", "Choose"), 48, () => { FrogSkins.Select(s.id); Refresh(grid); });
            }
            else
            {
                button = MakeButton(card.transform, "Action", skin.price.ToString(), 54, () => { if (FrogSkins.TryBuy(s)) { FrogSkins.Select(s.id); Refresh(grid); } });
                var price = button.GetComponentInChildren<TextMeshProUGUI>().rectTransform;
                price.anchoredPosition = new Vector2(30, 0);
                var fly = MakeImage(button.transform, "Fly", flyIcon, Color.white);
                fly.preserveAspect = true;
                fly.raycastTarget = false;
                Place(fly.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-50, 0), new Vector2(62, 62));
                button.interactable = FrogSkins.Flies >= skin.price;
            }
            Place((RectTransform)button.transform, new Vector2(0.5f, 0), new Vector2(0, 52), new Vector2(270, 82));
        }
    }

    private static string Localized(string key, string fallback)
    {
        var text = LocalizationSettings.StringDatabase.GetLocalizedString(key);
        return string.IsNullOrEmpty(text) || text.StartsWith("No translation") ? fallback : text;
    }

    private Image MakeImage(Transform parent, string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private TextMeshProUGUI MakeText(Transform parent, string name, string text, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = TextColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    private Button MakeButton(Transform parent, string name, string text, float size, UnityEngine.Events.UnityAction onClick)
    {
        var image = MakeImage(parent, name, buttonSprite, Color.white);
        var button = image.gameObject.AddComponent<Button>();
        if (onClick != null) button.onClick.AddListener(onClick);
        var label = MakeText(image.transform, "Text", text, size);
        Stretch(label.rectTransform);
        return button;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
