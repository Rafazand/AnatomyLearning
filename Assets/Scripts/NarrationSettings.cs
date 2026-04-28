using UnityEngine;

public class NarrationSettings : MonoBehaviour
{
    public static NarrationSettings Instance { get; private set; }

    [Header("Narration Toggle")]
    public bool narrationEnabled = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // optional: biar kebawa antar scene
    }

    public void ToggleNarration()
    {
        narrationEnabled = !narrationEnabled;
    }

    public void SetNarration(bool enabled)
    {
        narrationEnabled = enabled;
    }
}
