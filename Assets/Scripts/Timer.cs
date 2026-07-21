using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RoundTimer : MonoBehaviour
{
    [Header("Round")]
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private float introductionDuration = 3f;
    [SerializeField] private AnimalActor player;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text resultText;

    private float remainingTime;
    private bool roundStarted;
    private bool roundEnded;

    private GameObject timerBackground;
    private GameObject resultBackground;

    private void Start()
    {
        Time.timeScale = 0f;

        remainingTime = roundDuration;
        roundStarted = false;
        roundEnded = false;

        ConfigureUI();

        timerText.gameObject.SetActive(false);
        timerBackground.SetActive(false);

        resultText.gameObject.SetActive(true);
        resultBackground.SetActive(true);

        resultText.text =
            "EAT THE MOST CARROTS\nBECOME THE BIGGEST PIG!";

        StartCoroutine(StartRoundRoutine());
    }

    private IEnumerator StartRoundRoutine()
    {
        yield return new WaitForSecondsRealtime(
            introductionDuration
        );

        resultText.text = string.Empty;
        resultText.gameObject.SetActive(false);
        resultBackground.SetActive(false);

        remainingTime = roundDuration;
        UpdateTimerText();

        timerText.gameObject.SetActive(true);
        timerBackground.SetActive(true);

        roundStarted = true;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (!roundStarted || roundEnded)
        {
            return;
        }

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerText();
            EndRound();
            return;
        }

        UpdateTimerText();
    }

    private void ConfigureUI()
    {
        ConfigureTimerText();
        ConfigureResultText();

        timerBackground = CreateBackground(
            timerText,
            "TimerBackground",
            new Color(0f, 0f, 0f, 0.65f),
            new Vector2(200f, 62f)
        );

        resultBackground = CreateBackground(
            resultText,
            "ResultBackground",
            new Color(0f, 0f, 0f, 0.78f),
            new Vector2(760f, 190f)
        );
    }

    private void ConfigureTimerText()
    {
        RectTransform rect =
            timerText.rectTransform;

        rect.anchorMin =
            new Vector2(0.5f, 1f);

        rect.anchorMax =
            new Vector2(0.5f, 1f);

        rect.pivot =
            new Vector2(0.5f, 1f);

        rect.anchoredPosition =
            new Vector2(0f, -18f);

        rect.sizeDelta =
            new Vector2(190f, 54f);

        timerText.enableAutoSizing = false;
        timerText.fontSize = 32f;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment =
            TextAlignmentOptions.Center;

        timerText.color = Color.white;
        timerText.outlineColor =
            new Color32(30, 20, 10, 255);

        timerText.outlineWidth = 0.22f;
        timerText.raycastTarget = false;
    }

    private void ConfigureResultText()
    {
        RectTransform rect =
            resultText.rectTransform;

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition =
            Vector2.zero;

        rect.sizeDelta =
            new Vector2(720f, 160f);

        resultText.enableAutoSizing = false;
        resultText.fontSize = 42f;
        resultText.fontStyle = FontStyles.Bold;
        resultText.alignment =
            TextAlignmentOptions.Center;

        resultText.color =
            new Color32(255, 224, 120, 255);

        resultText.outlineColor =
            new Color32(55, 25, 5, 255);

        resultText.outlineWidth = 0.25f;
        resultText.raycastTarget = false;
    }

    private GameObject CreateBackground(
        TMP_Text text,
        string objectName,
        Color color,
        Vector2 size
    )
    {
        Transform parent =
            text.transform.parent;

        Transform existing =
            parent.Find(objectName);

        GameObject background;

        if (existing != null)
        {
            background = existing.gameObject;
        }
        else
        {
            background =
                new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

            background.transform.SetParent(
                parent,
                false
            );
        }

        RectTransform rect =
            background.GetComponent<RectTransform>();

        RectTransform textRect =
            text.rectTransform;

        rect.anchorMin = textRect.anchorMin;
        rect.anchorMax = textRect.anchorMax;
        rect.pivot = textRect.pivot;
        rect.anchoredPosition =
            textRect.anchoredPosition;
        rect.sizeDelta = size;

        Image image =
            background.GetComponent<Image>();

        image.color = color;
        image.raycastTarget = false;

        background.transform.SetSiblingIndex(
            text.transform.GetSiblingIndex()
        );

        text.transform.SetAsLastSibling();

        return background;
    }

    private void UpdateTimerText()
    {
        int totalSeconds =
            Mathf.CeilToInt(remainingTime);

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;

        timerText.text =
            $"{minutes:00}:{seconds:00}";
    }

    private void EndRound()
    {
        roundEnded = true;

        AnimalActor[] animals =
            FindObjectsByType<AnimalActor>(
                FindObjectsSortMode.None
            );

        if (animals.Length == 0)
        {
            ShowResult("NO PIGS FOUND");
            Time.timeScale = 0f;
            return;
        }

        AnimalActor winner = null;
        float largestScale = float.MinValue;
        int winnerCount = 0;

        foreach (AnimalActor animal in animals)
        {
            float scale =
                animal.TotalScale;

            if (scale > largestScale + 0.001f)
            {
                largestScale = scale;
                winner = animal;
                winnerCount = 1;
            }
            else if (
                Mathf.Abs(
                    scale - largestScale
                ) <= 0.001f
            )
            {
                winnerCount++;
            }
        }

        if (winnerCount > 1)
        {
            ShowResult(
                $"DRAW!\nBIGGEST SCALE: {largestScale:F2}"
            );
        }
        else if (winner == player)
        {
            ShowResult(
                $"YOU WIN!\nYOUR SCALE: {largestScale:F2}"
            );
        }
        else
        {
            ShowResult(
                $"AI WINS!\nBIGGEST SCALE: {largestScale:F2}"
            );
        }

        Time.timeScale = 0f;
    }

    private void ShowResult(string message)
    {
        timerText.gameObject.SetActive(false);
        timerBackground.SetActive(false);

        resultText.gameObject.SetActive(true);
        resultBackground.SetActive(true);

        resultText.text = message;
    }
}