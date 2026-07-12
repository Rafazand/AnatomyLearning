using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    [Header("JSON")]
    public TextAsset quizJson;

    [Header("UI")]
    public TMP_Text questionText;
    public TMP_Text scoreText;
    public TMP_Text finalScoreText;

    [Header("Panels")]
    public GameObject quizPanel;
    public GameObject resultPanel;

    [Header("Dynamic Panel Size")]
    [Tooltip("Main Panel (background) yang akan menyesuaikan tinggi dengan panjang soal.")]
    public RectTransform mainPanelRect;
    public float panelPaddingY = 80f;
    public float panelMinHeight = 220f;
    public float panelMaxHeight = 500f;

    [Header("Settings")]
    public int totalQuestions = 10;
    public float nextQuestionDelay = 1.2f;

    private List<QuizQuestion> selectedQuestions = new List<QuizQuestion>();
    private int currentIndex;
    private int score;
    private bool canAnswer;
    private bool quizRunning;
    private Coroutine nextQuestionCoroutine;

    public bool CanAnswer => canAnswer;

    private void Start()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    public void StartQuiz()
    {
        if (quizJson == null)
        {
            Debug.LogError("Quiz JSON belum dipasang.");
            return;
        }

        QuizQuestionList data = JsonUtility.FromJson<QuizQuestionList>(quizJson.text);

        if (data == null || data.questions == null || data.questions.Count == 0)
        {
            Debug.LogError("JSON kosong atau format salah.");
            return;
        }

        StopPendingNextQuestion();

        selectedQuestions = data.questions
            .OrderBy(q => Random.value)
            .Take(totalQuestions)
            .ToList();

        currentIndex = 0;
        score = 0;
        canAnswer = true;
        quizRunning = true;

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        ShowQuestion();
    }

    private void ShowQuestion()
    {
        if (!quizRunning) return;

        canAnswer = true;

        if (questionText != null)
        {
            questionText.text = selectedQuestions[currentIndex].question;
            FitPanelToQuestion();
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}/{selectedQuestions.Count}";
        }
    }

    private void FitPanelToQuestion()
    {
        if (mainPanelRect == null) return;

        questionText.ForceMeshUpdate();
        float targetHeight = Mathf.Clamp(questionText.preferredHeight + panelPaddingY, panelMinHeight, panelMaxHeight);

        Vector2 panelSize = mainPanelRect.sizeDelta;
        panelSize.y = targetHeight;
        mainPanelRect.sizeDelta = panelSize;

        RectTransform questionRect = questionText.rectTransform;
        Vector2 questionSize = questionRect.sizeDelta;
        questionSize.y = targetHeight - panelPaddingY * 0.5f;
        questionRect.sizeDelta = questionSize;
    }

    public void SubmitAnswer(string organId, SnapZone snapZone)
    {
        if (!quizRunning) return;
        if (!canAnswer) return;

        canAnswer = false;

        string correctAnswer = selectedQuestions[currentIndex].answerId;
        bool isCorrect = organId == correctAnswer;

        Debug.Log($"Jawaban masuk: {organId}, jawaban benar: {correctAnswer}, hasil: {isCorrect}");

        if (isCorrect)
        {
            score++;

            if (snapZone != null)
            {
                snapZone.SetCorrectColor();
            }
        }
        else
        {
            if (snapZone != null)
            {
                snapZone.SetWrongColor();
            }
        }

        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}/{selectedQuestions.Count}";
        }

        nextQuestionCoroutine = StartCoroutine(NextQuestionRoutine(snapZone));
    }

    private IEnumerator NextQuestionRoutine(SnapZone snapZone)
    {
        yield return new WaitForSeconds(nextQuestionDelay);

        nextQuestionCoroutine = null;

        if (snapZone != null)
        {
            snapZone.ReturnCurrentOrganHome();
            snapZone.ResetZone();
        }

        currentIndex++;

        if (currentIndex >= selectedQuestions.Count)
        {
            FinishQuiz();
        }
        else
        {
            ShowQuestion();
        }
    }

    private void StopPendingNextQuestion()
    {
        if (nextQuestionCoroutine != null)
        {
            StopCoroutine(nextQuestionCoroutine);
            nextQuestionCoroutine = null;
        }
    }

    private void FinishQuiz()
    {
        canAnswer = false;
        quizRunning = false;

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Score Akhir: {score}/{selectedQuestions.Count}";
        }
    }

    public void RetryQuiz()
    {
        StartQuiz();
    }

    public void BackToMenu()
    {
        canAnswer = false;
        quizRunning = false;

        StopPendingNextQuestion();

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
}