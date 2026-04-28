using UnityEngine;
using TMPro;

public class QuizSimple : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text questionText;
    public TMP_Text feedbackText;

    [Header("Answers A-D (isi text tombolnya)")]
    public TMP_Text answerAText;
    public TMP_Text answerBText;
    public TMP_Text answerCText;
    public TMP_Text answerDText;

    [Header("Question Data")]
    [TextArea] public string question = "Organ apakah yang berfungsi memompa darah?";
    public string answerA = "Paru-paru";
    public string answerB = "Jantung";
    public string answerC = "Lambung";
    public string answerD = "Ginjal";
    public char correct = 'B';

    void OnEnable()
    {
        if (questionText) questionText.text = question;
        if (answerAText) answerAText.text = $"A. {answerA}";
        if (answerBText) answerBText.text = $"B. {answerB}";
        if (answerCText) answerCText.text = $"C. {answerC}";
        if (answerDText) answerDText.text = $"D. {answerD}";
        if (feedbackText) feedbackText.text = "";
    }

    public void PickA() => Check('A');
    public void PickB() => Check('B');
    public void PickC() => Check('C');
    public void PickD() => Check('D');

    void Check(char picked)
    {
        if (!feedbackText) return;
        feedbackText.text = (picked == correct) ? "✅ Benar!" : "❌ Salah, coba lagi.";
    }
}
