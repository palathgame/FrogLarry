using UnityEngine;

// Frog colours bought with eaten flies. State lives in PlayerPrefs:
//   "Flies"                 fly wallet (every fly eaten in a won level adds 1, also on replays)
//   "FrogSkin"              selected skin id
//   "FrogSkinOwned_<id>"    1 when bought
// Sprites: Resources/FrogSkins/Frog<id>.png (recoloured Fog.png with the same bones,
// made by Tools/generate_frog_skins.py) and Resources/FrogSkins/Icons/<id>.png for the shop.
public static class FrogSkins
{
    public struct Skin
    {
        public string id;
        public int price;
        public Skin(string id, int price) { this.id = id; this.price = price; }
    }

    public const string DefaultSkin = "Green";

    public static readonly Skin[] All =
    {
        new Skin(DefaultSkin, 0),
        new Skin("Yellow", 5),
        new Skin("Blue", 5),
        new Skin("Orange", 10),
        new Skin("Teal", 10),
        new Skin("Red", 15),
        new Skin("Purple", 15),
        new Skin("Pink", 20),
    };

    private const string FliesKey = "Flies";
    private const string SelectedKey = "FrogSkin";
    private const string OwnedPrefix = "FrogSkinOwned_";

    public static int Flies => PlayerPrefs.GetInt(FliesKey, 0);

    public static void AddFlies(int count)
    {
        if (count <= 0) return;
        PlayerPrefs.SetInt(FliesKey, Flies + count);
        PlayerPrefs.Save();
    }

    public static bool IsOwned(string id)
    {
        return id == DefaultSkin || PlayerPrefs.GetInt(OwnedPrefix + id, 0) == 1;
    }

    public static bool TryBuy(Skin skin)
    {
        if (IsOwned(skin.id) || Flies < skin.price) return false;
        PlayerPrefs.SetInt(FliesKey, Flies - skin.price);
        PlayerPrefs.SetInt(OwnedPrefix + skin.id, 1);
        PlayerPrefs.Save();
        return true;
    }

    public static string Selected
    {
        get
        {
            string id = PlayerPrefs.GetString(SelectedKey, DefaultSkin);
            return IsOwned(id) ? id : DefaultSkin;
        }
    }

    public static void Select(string id)
    {
        if (!IsOwned(id)) return;
        PlayerPrefs.SetString(SelectedKey, id);
        PlayerPrefs.Save();
    }

    // null for the default skin: keep the sprite the prefab already has.
    public static Sprite LoadSprite(string id)
    {
        return id == DefaultSkin ? null : Resources.Load<Sprite>("FrogSkins/Frog" + id);
    }

    public static Sprite LoadIcon(string id)
    {
        return Resources.Load<Sprite>("FrogSkins/Icons/" + id);
    }
}
