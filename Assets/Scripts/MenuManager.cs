using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menu1Panel;
    public GameObject menu2Panel;
    public GameObject explorePanel;
    public GameObject learnPanel;

    [Header("Scene Content")]
    public GameObject organsRoot; // parent dari semua organ/model
    public GameObject organsRoot2;

    void Start()
    {
        ShowMenu1();
    }

    void SetOnly(GameObject panelOn)
    {
        if (menu1Panel) menu1Panel.SetActive(panelOn == menu1Panel);
        if (menu2Panel) menu2Panel.SetActive(panelOn == menu2Panel);
        if (explorePanel) explorePanel.SetActive(panelOn == explorePanel);
        if (learnPanel) learnPanel.SetActive(panelOn == learnPanel);
    }

    public void ShowMenu1()
    {
        SetOnly(menu1Panel);
        if (organsRoot) organsRoot.SetActive(false);
    }

    public void ShowMenu2()
    {
        SetOnly(menu2Panel);
        if (organsRoot) organsRoot.SetActive(false);
    }

    public void StartExplore()
    {
        SetOnly(explorePanel);
        if (organsRoot) organsRoot.SetActive(true);
    }

    public void StartLearn()
    {
        SetOnly(learnPanel);
        if (organsRoot) organsRoot2.SetActive(true);
    }

    public void QuitApp()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
