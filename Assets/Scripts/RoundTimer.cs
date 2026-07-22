using System.Collections;
using TMPro;
using UnityEngine;

public sealed class RoundTimer : MonoBehaviour
{
    private const float ScaleTolerance = 0.001f;

    private const string IntroductionMessage =
        "EAT THE MOST CARROTS\nBECOME THE BIGGEST PIG!";

    [Header("Round")]
    [SerializeField, Min(0f)] private float roundDuration = 60f;
    [SerializeField, Min(0f)] private float introductionDuration = 3f;
    [SerializeField] private AnimalActor player;

    [Header("UI")]
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;

    private float remainingTime;
    private bool roundActive;

    private void Start()
    {
        Time.timeScale = 0f;

        ShowResult(IntroductionMessage);
        StartCoroutine(StartRoundRoutine());
    }

    private IEnumerator StartRoundRoutine()
    {
        // Use real time because the game is paused.
        yield return new WaitForSecondsRealtime(
            introductionDuration
        );

        remainingTime = roundDuration;
        UpdateTimerText();

        resultPanel.SetActive(false);
        timerPanel.SetActive(true);

        roundActive = true;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (!roundActive)
        {
            return;
        }

        remainingTime =
            Mathf.Max(
                0f,
                remainingTime - Time.deltaTime
            );

        UpdateTimerText();

        if (remainingTime <= 0f)
        {
            EndRound();
        }
    }

    private void UpdateTimerText()
    {
        int totalSeconds =
            Mathf.CeilToInt(remainingTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text =
            $"{minutes:00}:{seconds:00}";
    }

    private void EndRound()
    {
        roundActive = false;
        Time.timeScale = 0f;

        AnimalActor[] animals =
            FindObjectsByType<AnimalActor>(
                FindObjectsSortMode.None
            );

        if (animals.Length == 0)
        {
            ShowResult("NO PIGS FOUND");
            return;
        }

        AnimalActor winner = null;
        float largestScale = float.MinValue;
        int winnerCount = 0;

        foreach (AnimalActor animal in animals)
        {
            float scale = animal.TotalScale;

            if (scale > largestScale + ScaleTolerance)
            {
                largestScale = scale;
                winner = animal;
                winnerCount = 1;
            }
            else if (
                Mathf.Abs(scale - largestScale)
                <= ScaleTolerance)
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
    }

    private void ShowResult(string message)
    {
        timerPanel.SetActive(false);
        resultPanel.SetActive(true);
        resultText.text = message;
    }
}