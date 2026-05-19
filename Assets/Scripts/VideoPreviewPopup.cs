using UnityEngine;
using UnityEngine.Video;

public class VideoPreviewPopup : MonoBehaviour
{
    public GameObject videoPreviewPanel;
    public VideoPlayer videoPlayer;
    public MenuManager menuManager;

    public void OpenVideoPopup()
    {
        videoPreviewPanel.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.Play();
        }
    }

    public void CloseVideoPopup()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        videoPreviewPanel.SetActive(false);

        if (menuManager != null)
        {
            menuManager.OpenMenu2();
        }
    }
}