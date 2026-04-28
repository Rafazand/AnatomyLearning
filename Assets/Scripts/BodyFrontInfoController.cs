using UnityEngine;
using TMPro;

public class BodyFrontInfoController : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public AudioSource audioSource;
    public AudioClip narration;

    public void ShowInfo()
    {
        panel.SetActive(true);

        titleText.text = "Body Front";
        descriptionText.text = "This section represents the front part of the human body anatomy.";

        // ✅ hanya play kalau narration enabled
        if (narration != null)
        {
            audioSource.clip = narration;
            audioSource.Play();
        }
    }

    public void HideInfo()
    {
        panel.SetActive(false);
        audioSource.Stop();
    }

    // ✅ bisa dipanggil tombol untuk stop langsung
    public void StopNarration()
    {
        audioSource.Stop();
    }
}
