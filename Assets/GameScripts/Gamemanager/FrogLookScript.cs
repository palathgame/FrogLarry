using System.Collections;
using UnityEngine;

// On the Frog prefab: applies the bought colour and, when the level starts,
// blinks an arrow above the frog a few times so the player sees where he is.
public class FrogLookScript : MonoBehaviour
{
    [SerializeField] private Sprite arrowSprite;        // Graphic/Other/TutorialArrow (points left)
    [SerializeField] private int blinks = 3;
    [SerializeField] private float arrowHeight = 1.1f;  // world units
    [SerializeField] private float gap = 0.6f;         // distance of the arrow tip above the frog centre

    private GameObject arrow;

    private void Awake()
    {
        var sprite = FrogSkins.LoadSprite(FrogSkins.Selected);
        if (sprite != null) GetComponent<SpriteRenderer>().sprite = sprite;
    }

    private void Start()
    {
        if (arrowSprite != null) StartCoroutine(BlinkArrow());
    }

    private IEnumerator BlinkArrow()
    {
        arrow = new GameObject("FrogArrow");
        var renderer = arrow.AddComponent<SpriteRenderer>();
        renderer.sprite = arrowSprite;
        renderer.sortingOrder = 10;
        renderer.enabled = false;
        // Rotated to point down; not parented so it stays upright while the frog turns.
        arrow.transform.rotation = Quaternion.Euler(0, 0, 90);
        float scale = arrowHeight / arrowSprite.bounds.size.x;
        arrow.transform.localScale = new Vector3(scale, scale, 1);
        float halfLength = arrowHeight / 2f;

        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < blinks; i++)
        {
            renderer.enabled = true;
            float t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float bob = Mathf.Sin(t / 0.5f * Mathf.PI) * 0.15f;
                arrow.transform.position = transform.position + Vector3.up * (gap + halfLength + bob);
                yield return null;
            }
            renderer.enabled = false;
            yield return new WaitForSeconds(0.2f);
        }
        Destroy(arrow);
    }

    // If the frog goes away mid-blink, do not leave the arrow hanging on screen.
    private void OnDestroy()
    {
        if (arrow != null) Destroy(arrow);
    }
}
