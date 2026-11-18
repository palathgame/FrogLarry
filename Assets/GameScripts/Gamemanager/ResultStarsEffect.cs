using System.Collections;
using UnityEngine;

// Added by ResultScoreScript.setWin: the result stars pop in one by one. Each earned star
// plays a chime (Resources/Sounds/StarDing, made by Tools/generate_effects.py) a little higher
// than the one before; a missed star pops in quietly and smaller. The "+N flies" row comes last.
public class ResultStarsEffect : MonoBehaviour
{
    private const float FirstDelay = 0.25f, Gap = 0.35f, PopTime = 0.3f;

    public void Play(Transform[] stars, int earned, Transform after)
    {
        StartCoroutine(Run(stars, earned, after));
    }

    private IEnumerator Run(Transform[] stars, int earned, Transform after)
    {
        var scales = new Vector3[stars.Length];
        for (int i = 0; i < stars.Length; i++)
        {
            scales[i] = stars[i].localScale;
            stars[i].localScale = Vector3.zero;
        }
        Vector3 afterScale = Vector3.one;
        if (after != null)
        {
            afterScale = after.localScale;
            after.localScale = Vector3.zero;
        }

        var clip = Resources.Load<AudioClip>("Sounds/StarDing");
        var source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0;
        source.volume = PlayerPrefs.GetFloat("GameSounds", 1.0f);

        yield return new WaitForSecondsRealtime(FirstDelay);
        for (int i = 0; i < stars.Length; i++)
        {
            bool won = i < earned;
            if (won && clip != null)
            {
                source.pitch = 1f + 0.12f * i;
                source.PlayOneShot(clip);
            }
            StartCoroutine(Pop(stars[i], scales[i], won ? 1.3f : 1.05f));
            yield return new WaitForSecondsRealtime(Gap);
        }
        if (after != null) yield return Pop(after, afterScale, 1.2f);
    }

    // Grows from nothing past full size and settles back.
    private static IEnumerator Pop(Transform target, Vector3 scale, float overshoot)
    {
        for (float t = 0; t < PopTime; t += Time.unscaledDeltaTime)
        {
            if (target == null) yield break;
            float k = t / PopTime;
            float s = k < 0.6f ? Mathf.Lerp(0f, overshoot, k / 0.6f) : Mathf.Lerp(overshoot, 1f, (k - 0.6f) / 0.4f);
            target.localScale = scale * s;
            yield return null;
        }
        if (target != null) target.localScale = scale;
    }
}
