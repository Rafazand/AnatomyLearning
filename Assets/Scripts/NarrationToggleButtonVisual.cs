using UnityEngine;

public class NarrationToggleButtonVisual : MonoBehaviour
{
    public Renderer targetRenderer;
    public string enabledColorHex = "#3BFF6A"; // hijau
    public string disabledColorHex = "#FF3B3B"; // merah

    private Color enabledColor;
    private Color disabledColor;

    void Awake()
    {
        ColorUtility.TryParseHtmlString(enabledColorHex, out enabledColor);
        ColorUtility.TryParseHtmlString(disabledColorHex, out disabledColor);
        UpdateVisual();
    }

    public void ToggleAndUpdate()
    {
        NarrationSettings.Instance.ToggleNarration();
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (targetRenderer == null) return;
        bool on = NarrationSettings.Instance != null && NarrationSettings.Instance.narrationEnabled;
        targetRenderer.material.color = on ? enabledColor : disabledColor;
    }
}
