using UnityEngine;

public class SnapZone : MonoBehaviour
{
    [Header("Snap")]
    public string acceptedId;
    public Transform snapAnchor;
    public GameObject silhouette;

    [Header("Quiz")]
    public QuizManager quizManager;
    public bool submitToQuiz = true;

    [Header("Answer Display")]
    public Vector3 answerScale = new Vector3(0.2f, 0.2f, 0.2f);
    public Vector3 answerRotationEuler = new Vector3(0f, 180f, 0f);

    [Header("Visual")]
    public Renderer zoneRenderer;
    public Color defaultColor = Color.white;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    private bool hasAnswered = false;
    private MagnetSnapDual currentOrgan;

    private void Awake()
    {
        if (quizManager == null)
        {
            quizManager = FindObjectOfType<QuizManager>();
        }
    }

    private void Start()
    {
        if (zoneRenderer != null)
        {
            zoneRenderer.material = new Material(zoneRenderer.material);
            ResetZone();
        }
    }

    public bool Accepts(string organId)
    {
        if (submitToQuiz)
        {
            return true;
        }

        return string.IsNullOrEmpty(acceptedId) || organId == acceptedId;
    }

    public void OnObjectPlaced(string organId, MagnetSnapDual organ)
    {
        if (hasAnswered) return;

        hasAnswered = true;
        currentOrgan = organ;

        if (currentOrgan != null)
        {
            currentOrgan.SetLocked(true);
        }

        if (submitToQuiz && quizManager != null)
        {
            quizManager.SubmitAnswer(organId, this);
        }
    }

    public void ReturnCurrentOrganHome()
    {
        if (currentOrgan != null)
        {
            currentOrgan.SetLocked(false);
            currentOrgan.ForceReturnHome();
            currentOrgan = null;
        }
    }

    public void SetCorrectColor()
    {
        SetColor(correctColor);
    }

    public void SetWrongColor()
    {
        SetColor(wrongColor);
    }

    public void ResetZone()
    {
        hasAnswered = false;
        SetColor(defaultColor);
        SetSilhouette(false);
    }

    public void SetSilhouette(bool on)
    {
        if (silhouette != null)
        {
            silhouette.SetActive(on);
        }
    }

    public void SetColor(Color color)
    {
        if (zoneRenderer != null)
        {
            zoneRenderer.material.color = color;
        }
    }
}