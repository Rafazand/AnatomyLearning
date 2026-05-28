using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menu1Panel;
    public GameObject menu2Panel;
    public GameObject explorePanel;
    public GameObject learnPanel; // object Quiz

    [Header("Scene Content")]
    public GameObject organsRoot;
    public GameObject organsRoot2;

    [Header("Quiz")]
    public QuizManager quizManager;

    public void OpenMenu1()
    {
        HideOrganInfoPanel();

        menu1Panel.SetActive(true);
        menu2Panel.SetActive(false);
        explorePanel.SetActive(false);
        learnPanel.SetActive(false);

        SetSceneContent(false, false);
    }

    public void OpenMenu2()
    {
        HideOrganInfoPanel();

        menu1Panel.SetActive(false);
        menu2Panel.SetActive(true);
        explorePanel.SetActive(false);
        learnPanel.SetActive(false);

        SetSceneContent(false, false);
    }

    public void OpenExplore()
    {
        menu1Panel.SetActive(false);
        menu2Panel.SetActive(false);
        explorePanel.SetActive(true);
        learnPanel.SetActive(false);

        SetSceneContent(true, false);
    }

    public void OpenQuiz()
    {
        HideOrganInfoPanel();

        menu1Panel.SetActive(false);
        menu2Panel.SetActive(false);
        explorePanel.SetActive(false);
        learnPanel.SetActive(true);

        // Quiz hanya pakai OrgansRoot2
        SetSceneContent(false, true);

        if (quizManager != null)
        {
            quizManager.StartQuiz();
        }
    }

    public void BackToMainMenu()
    {
        OpenMenu1();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private void SetSceneContent(bool showOrgansRoot, bool showOrgansRoot2)
    {
        if (organsRoot != null)
            organsRoot.SetActive(showOrgansRoot);

        if (organsRoot2 != null)
            organsRoot2.SetActive(showOrgansRoot2);
    }

    /// <summary>
    /// Sembunyikan info panel organ (Explore) saat berpindah ke mode lain.
    /// Aman dipanggil kapan saja — null-check dilakukan di dalam BodyFrontInfoController.
    /// </summary>
    private void HideOrganInfoPanel()
    {
        BodyFrontInfoController.Instance?.ForceHide();
    }
}
