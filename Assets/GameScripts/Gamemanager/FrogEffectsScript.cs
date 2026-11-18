using System.Collections;
using TMPro;
using UnityEngine;

// On the Frog prefab: small feedback effects.
//  - FlyEaten: "+1" with a fly icon floats up above Larry.
//  - Bump: Larry bumps into a bush or stone he cannot pass: he nudges forward and back,
//    the obstacle shakes, a soft thump plays (Sounds/Bump.wav, made by Tools/generate_effects.py)
//    and dizzy stars circle above his head.
public class FrogEffectsScript : MonoBehaviour
{
    [SerializeField] private Sprite flyIcon;      // Resources/FrogSkins/Icons/FlyIcon
    [SerializeField] private Sprite starSprite;   // Graphic/Effects/DizzyStar
    [SerializeField] private AudioClip bumpClip;  // Sounds/Bump
    [SerializeField] private TMP_FontAsset font;

    private const int SortingOrder = 20; // above everything on the board (frog is 3)

    public void FlyEaten()
    {
        StartCoroutine(FloatPlusOne());
    }

    // Runs inside the command sequence (PlayScript waits for it), so it stays short.
    public IEnumerator Bump(Vector3 direction, Collider2D obstacle)
    {
        if (bumpClip != null)
        {
            AudioSource.PlayClipAtPoint(bumpClip, Camera.main != null ? Camera.main.transform.position : transform.position,
                PlayerPrefs.GetFloat("GameSounds", 1.0f));
        }
        if (obstacle != null) StartCoroutine(Shake(obstacle.transform, 0.35f, 0.06f));
        StartCoroutine(DizzyStars());

        Vector3 start = transform.position;
        // Nudge towards the obstacle and bounce back.
        const float nudge = 0.18f, nudgeTime = 0.16f;
        for (float t = 0; t < nudgeTime; t += Time.deltaTime)
        {
            transform.position = start + direction * nudge * Mathf.Sin(t / nudgeTime * Mathf.PI);
            yield return null;
        }
        transform.position = start;
        yield return Shake(transform, 0.25f, 0.05f);
        transform.position = start;
        yield return new WaitForSeconds(0.15f);
    }

    private IEnumerator Shake(Transform target, float duration, float strength)
    {
        if (target == null) yield break;
        Vector3 start = target.position;
        for (float t = 0; t < duration && target != null; t += Time.deltaTime)
        {
            float fade = 1f - t / duration;
            target.position = start + new Vector3(Mathf.Sin(t * 70f), Mathf.Cos(t * 55f) * 0.5f, 0) * strength * fade;
            yield return null;
        }
        if (target != null) target.position = start;
    }

    private IEnumerator DizzyStars()
    {
        if (starSprite == null) yield break;
        const int count = 3;
        const float duration = 1.2f, radius = 0.42f, height = 0.6f;
        var stars = new SpriteRenderer[count];
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("DizzyStar");
            go.transform.localScale = Vector3.one * 0.6f;
            stars[i] = go.AddComponent<SpriteRenderer>();
            stars[i].sprite = starSprite;
            stars[i].sortingOrder = SortingOrder;
        }
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float alpha = Mathf.Clamp01((duration - t) / 0.3f);
            for (int i = 0; i < count; i++)
            {
                if (stars[i] == null) continue;
                float angle = t * 7f + i * Mathf.PI * 2f / count;
                // Flattened circle above the head (screen-up, not frog-up, so it reads the same at any facing).
                stars[i].transform.position = transform.position + new Vector3(Mathf.Cos(angle) * radius, height + Mathf.Sin(angle) * radius * 0.35f, 0);
                stars[i].transform.rotation = Quaternion.Euler(0, 0, t * 300f);
                stars[i].color = new Color(1, 1, 1, alpha);
            }
            yield return null;
        }
        foreach (var star in stars) if (star != null) Destroy(star.gameObject);
    }

    private IEnumerator FloatPlusOne()
    {
        var root = new GameObject("PlusOne");
        root.transform.position = transform.position;

        var text = new GameObject("Text").AddComponent<TextMeshPro>();
        text.transform.SetParent(root.transform, false);
        text.transform.localPosition = new Vector3(-0.28f, 0, 0);
        text.font = font;
        text.text = "+1";
        text.fontSize = 6.5f;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(1f, 0.9f, 0.3f);
        text.outlineColor = new Color32(70, 40, 15, 255);
        text.outlineWidth = 0.25f;
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = new Vector2(1.2f, 0.6f);
        text.sortingOrder = SortingOrder;

        SpriteRenderer icon = null;
        if (flyIcon != null)
        {
            icon = new GameObject("Fly").AddComponent<SpriteRenderer>();
            icon.transform.SetParent(root.transform, false);
            icon.transform.localPosition = new Vector3(0.4f, 0, 0);
            icon.sprite = flyIcon;
            icon.sortingOrder = SortingOrder;
            float size = Mathf.Max(flyIcon.bounds.size.x, flyIcon.bounds.size.y);
            icon.transform.localScale = Vector3.one * (0.6f / size);
        }

        const float duration = 1.2f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            // Follows Larry (screen-up, whatever way he faces) while it rises.
            root.transform.position = transform.position + Vector3.up * (0.5f + k * 0.9f);
            float pop = k < 0.15f ? Mathf.Lerp(0.4f, 1.15f, k / 0.15f) : Mathf.Lerp(1.15f, 1f, Mathf.Clamp01((k - 0.15f) / 0.15f));
            root.transform.localScale = Vector3.one * pop;
            float alpha = Mathf.Clamp01((1f - k) / 0.35f);
            text.alpha = alpha;
            if (icon != null) icon.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        Destroy(root);
    }
}
