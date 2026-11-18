using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

// Guided step-by-step tutorial for the first levels.
// Each step shows a text bubble and either a "ghost" block flying from a button
// to a slot (drag hint) or a bouncing arrow over a button (tap hint).
// A step ends when its condition is met; pressing Play at any time skips ahead.
public class TutorialScript : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Sprite bubbleSprite;
    [SerializeField] private Sprite arrowSprite;
    [SerializeField] private TMP_FontAsset font;

    private class Step
    {
        public string key;
        public Func<bool> done;
        public Func<RectTransform> ghostFrom;
        public Func<RectTransform> ghostTo;
        public Func<RectTransform> arrowAt;
        public bool arrowFromAbove;
    }

    private enum State { Steps, Running, Retry, Finished }

    private List<Step> steps;
    private int stepIndex;
    private string hintKey;
    private State state = State.Steps;
    private float animTime;

    private RectTransform root;
    private RectTransform bubble;
    private TextMeshProUGUI bubbleText;
    private Image ghost;
    private RectTransform arrow;

    private Button playButton;

    private void Start()
    {
        // GridManagerScript falls back to level1 when nothing is saved yet (first launch).
        string level = PlayerPrefs.GetString("LastLevel");
        steps = BuildSteps(string.IsNullOrEmpty(level) ? "level1" : level);
        if (steps == null)
        {
            gameObject.SetActive(false);
            return;
        }
        playButton = GameObject.Find("Controll/RunPanel/Play").GetComponent<Button>();
        CreateUi();
        ShowStep(0);
    }

    private List<Step> BuildSteps(string level)
    {
        switch (level)
        {
            case "level1":
                hintKey = "tut_hint_l1";
                return new List<Step>
                {
                    new Step { key = "tut_l1_drag", ghostFrom = () => LeftButton("Move"), ghostTo = () => FirstEmpty("SlotCreator"),
                               done = () => Count("SlotCreator", "Move") >= 1 },
                    new Step { key = "tut_l1_more", ghostFrom = () => LeftButton("Move"), ghostTo = () => FirstEmpty("SlotCreator"),
                               done = () => Count("SlotCreator", "Move") >= 3 },
                    PlayStep(),
                };
            case "level2":
                hintKey = "tut_hint_l2";
                return new List<Step>
                {
                    // The "TurnRight" button creates a block tagged "Left" (rotates clockwise).
                    // The ghost shows the slot where the turn belongs (after two Moves).
                    new Step { key = "tut_l2_turn", ghostFrom = () => LeftButton("TurnRight"), ghostTo = () => Slot("SlotCreator", 2),
                               done = () => Count("SlotCreator", "Left") >= 1 },
                    new Step { key = "tut_l2_build", ghostFrom = () => LeftButton("Move"), ghostTo = () => FirstEmptyUpTo("SlotCreator", 6),
                               done = () => BlockCount("SlotCreator") >= 6 },
                    PlayStep(),
                };
            case "level3":
                hintKey = "tut_hint_l3";
                return new List<Step>
                {
                    new Step { key = "tut_l3_jump", ghostFrom = () => LeftButton("Jump"), ghostTo = () => Slot("SlotCreator", 1),
                               done = () => Count("SlotCreator", "Jump") >= 1 },
                    new Step { key = "tut_l3_build", ghostFrom = () => LeftButton("Move"), ghostTo = () => FirstEmptyUpTo("SlotCreator", 3),
                               done = () => BlockCount("SlotCreator") >= 3 },
                    PlayStep(),
                };
            case "level4":
                hintKey = "tut_hint_l4";
                return new List<Step>
                {
                    new Step { key = "tut_l4_eat", ghostFrom = () => LeftButton("Eat"), ghostTo = () => Slot("SlotCreator", 2),
                               done = () => Count("SlotCreator", "Eat") >= 1 },
                    new Step { key = "tut_l4_build", ghostFrom = () => LeftButton("Move"), ghostTo = () => FirstEmptyUpTo("SlotCreator", 6),
                               done = () => BlockCount("SlotCreator") >= 6 },
                    PlayStep(),
                };
            case "level5":
                hintKey = "tut_hint_l5";
                return new List<Step>
                {
                    new Step { key = "tut_l5_long", ghostFrom = () => LeftButton("Move"), ghostTo = () => FirstEmpty("SlotCreator2"),
                               done = () => Count("SlotCreator2", "Move") >= 3 || Repeater("SlotCreator2") != null },
                    new Step { key = "tut_l5_make", arrowAt = () => RunButton("MakeRepeater"),
                               done = () => Repeater("SlotCreator2") != null || Repeater("SlotCreator") != null },
                    new Step { key = "tut_l5_count", arrowAt = () => Repeater("SlotCreator2"),
                               done = () => RepeaterCount() >= 4 || Repeater("SlotCreator") != null },
                    new Step { key = "tut_l5_drag", ghostFrom = () => Repeater("SlotCreator2"), ghostTo = () => FirstEmpty("SlotCreator"),
                               done = () => Repeater("SlotCreator") != null },
                    PlayStep(),
                };
            default:
                return null;
        }
    }

    private Step PlayStep()
    {
        // Above Play there is free space; from the side the arrow would cover the first slot.
        return new Step { key = "tut_play", arrowAt = () => RunButton("Play"), arrowFromAbove = true, done = () => false };
    }

    private void Update()
    {
        if (GameObject.FindGameObjectWithTag("ResultScore") != null && state != State.Retry)
        {
            // Win (or pause menu): hide everything; on pause the tutorial resumes below.
            root.gameObject.SetActive(false);
            return;
        }
        root.gameObject.SetActive(state != State.Finished);

        bool running = !playButton.enabled;
        switch (state)
        {
            case State.Steps:
                if (running)
                {
                    state = State.Running;
                    SetVisible(false, false, false);
                    break;
                }
                if (steps[stepIndex].done() && stepIndex < steps.Count - 1)
                {
                    ShowStep(stepIndex + 1);
                }
                Animate(steps[stepIndex]);
                break;

            case State.Running:
                if (!running)
                {
                    // The run ended without a win (a win shows the ResultScore panel):
                    // show this level's hint and point at the menu, where the level can be restarted.
                    state = State.Retry;
                    animTime = 0;
                    SetText(hintKey);
                    SetVisible(true, false, true);
                }
                break;

            case State.Retry:
                if (GameObject.FindGameObjectWithTag("ResultScore") != null)
                {
                    state = State.Finished;
                    break;
                }
                Animate(new Step { arrowAt = () => (RectTransform)GameObject.Find("Controll/Menu").transform });
                break;
        }
    }

    private void ShowStep(int index)
    {
        stepIndex = index;
        animTime = 0;
        var step = steps[index];
        SetText(step.key);
        SetVisible(true, step.ghostFrom != null, step.arrowAt != null);
    }

    // ---------- animation ----------

    private void Animate(Step step)
    {
        animTime += Time.unscaledDeltaTime;

        if (step.ghostFrom != null)
        {
            var from = step.ghostFrom();
            var to = step.ghostTo();
            ghost.gameObject.SetActive(from != null && to != null);
            if (from != null && to != null)
            {
                const float travel = 1.2f, hold = 0.5f;
                float t = animTime % (travel + hold);
                float k = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / travel));
                var fromImage = from.GetComponent<Image>();
                if (fromImage != null) ghost.sprite = fromImage.sprite;
                ghost.rectTransform.position = Vector3.Lerp(Center(from), Center(to), k);
                ghost.rectTransform.sizeDelta = from.rect.size * (from.lossyScale.x / root.lossyScale.x);
                float alpha = t < travel ? 0.75f : Mathf.Lerp(0.75f, 0f, (t - travel) / hold);
                ghost.color = new Color(1, 1, 1, alpha);
            }
        }

        if (step.arrowAt != null)
        {
            var target = step.arrowAt();
            arrow.gameObject.SetActive(target != null);
            if (target != null)
            {
                Vector3[] corners = new Vector3[4];
                target.GetWorldCorners(corners);
                Vector3 center = (corners[0] + corners[2]) * 0.5f;
                if (step.arrowFromAbove)
                {
                    float lift = Mathf.Abs(Mathf.Sin(animTime * 4f)) * 30f * root.lossyScale.y;
                    arrow.localScale = Vector3.one;
                    arrow.localRotation = Quaternion.Euler(0, 0, 90f);
                    arrow.position = new Vector3(center.x, corners[1].y, center.z) + Vector3.up * lift;
                    return;
                }
                // Otherwise point from the side that faces the middle of the screen, so the arrow
                // never leaves the screen: targets on the left get an arrow on their right side.
                arrow.localRotation = Quaternion.identity;
                Vector3[] rootCorners = new Vector3[4];
                root.GetWorldCorners(rootCorners);
                bool fromRight = center.x < (rootCorners[0].x + rootCorners[2].x) * 0.5f;
                float bounce = Mathf.Abs(Mathf.Sin(animTime * 4f)) * 30f * root.lossyScale.x;
                float side = fromRight ? 1f : -1f;
                Vector3 edge = new Vector3(fromRight ? corners[2].x : corners[0].x, center.y, center.z);
                arrow.localScale = new Vector3(side, 1f, 1f);
                arrow.position = edge + Vector3.right * side * bounce;
            }
        }
    }

    // ---------- UI ----------

    private void CreateUi()
    {
        root = NewRect("Tutorial UI", canvas.transform);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;
        root.SetAsLastSibling();

        bubble = NewRect("Bubble", root);
        bubble.anchorMin = bubble.anchorMax = new Vector2(0.6f, 1f);
        bubble.pivot = new Vector2(0.5f, 1f);
        bubble.anchoredPosition = new Vector2(0, -12);
        bubble.sizeDelta = new Vector2(1150, 230);
        var bubbleImage = bubble.gameObject.AddComponent<Image>();
        bubbleImage.sprite = bubbleSprite;
        bubbleImage.raycastTarget = false;

        var textRect = NewRect("Text", bubble);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(120, 30);
        textRect.offsetMax = new Vector2(-120, -30);
        bubbleText = textRect.gameObject.AddComponent<TextMeshProUGUI>();
        bubbleText.font = font;
        bubbleText.color = Color.black;
        bubbleText.alignment = TextAlignmentOptions.Center;
        bubbleText.enableAutoSizing = true;
        bubbleText.fontSizeMin = 28;
        bubbleText.fontSizeMax = 56;
        bubbleText.raycastTarget = false;

        ghost = NewRect("Ghost", root).gameObject.AddComponent<Image>();
        ghost.raycastTarget = false;
        ghost.preserveAspect = true;

        arrow = NewRect("Arrow", root);
        // The sprite points left; the pivot is its tip. Mirrored (scale.x = -1) it points right.
        arrow.pivot = new Vector2(0f, 0.5f);
        arrow.sizeDelta = new Vector2(150, 105);
        var arrowImage = arrow.gameObject.AddComponent<Image>();
        arrowImage.sprite = arrowSprite;
        arrowImage.preserveAspect = true;
        arrowImage.raycastTarget = false;
    }

    private static Vector3 Center(RectTransform rect)
    {
        return rect.TransformPoint(rect.rect.center);
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private void SetVisible(bool showBubble, bool showGhost, bool showArrow)
    {
        bubble.gameObject.SetActive(showBubble);
        ghost.gameObject.SetActive(showGhost);
        arrow.gameObject.SetActive(showArrow);
    }

    private void SetText(string key)
    {
        bubbleText.text = LocalizationSettings.StringDatabase.GetLocalizedString(key);
    }

    // ---------- game state queries ----------

    private static RectTransform LeftButton(string name)
    {
        return (RectTransform)GameObject.Find("Controll/LeftPanel/" + name).transform;
    }

    private static RectTransform RunButton(string name)
    {
        return (RectTransform)GameObject.Find("Controll/RunPanel/" + name).transform;
    }

    private static Transform Row(string tag)
    {
        var row = GameObject.FindGameObjectWithTag(tag);
        return row != null ? row.transform : null;
    }

    private static RectTransform Slot(string tag, int index)
    {
        var row = Row(tag);
        return row != null && index < row.childCount ? (RectTransform)row.GetChild(index) : null;
    }

    private static RectTransform FirstEmptyUpTo(string tag, int count)
    {
        var row = Row(tag);
        if (row == null) return null;
        for (int i = 0; i < count && i < row.childCount; i++)
        {
            if (row.GetChild(i).childCount == 0) return (RectTransform)row.GetChild(i);
        }
        return null;
    }

    private static RectTransform FirstEmpty(string tag)
    {
        var row = Row(tag);
        if (row == null) return null;
        for (int i = 0; i < row.childCount; i++)
        {
            if (row.GetChild(i).childCount == 0) return (RectTransform)row.GetChild(i);
        }
        return null;
    }

    private static int Count(string tag, string commandTag)
    {
        var row = Row(tag);
        int count = 0;
        if (row == null) return 0;
        for (int i = 0; i < row.childCount; i++)
        {
            var slot = row.GetChild(i);
            if (slot.childCount > 0 && slot.GetChild(0).CompareTag(commandTag)) count++;
        }
        return count;
    }

    private static int BlockCount(string tag)
    {
        var row = Row(tag);
        int count = 0;
        if (row == null) return 0;
        for (int i = 0; i < row.childCount; i++)
        {
            if (row.GetChild(i).childCount > 0) count++;
        }
        return count;
    }

    private static RectTransform Repeater(string tag)
    {
        var row = Row(tag);
        if (row == null) return null;
        for (int i = 0; i < row.childCount; i++)
        {
            var slot = row.GetChild(i);
            if (slot.childCount > 0 && slot.GetChild(0).CompareTag("Repeater")) return (RectTransform)slot.GetChild(0);
        }
        return null;
    }

    private static int RepeaterCount()
    {
        var repeater = Repeater("SlotCreator2") ?? Repeater("SlotCreator");
        return repeater != null ? repeater.GetComponent<RepeateScript>().GetCountCycle() : 0;
    }
}
