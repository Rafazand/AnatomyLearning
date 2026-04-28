using UnityEngine;

public class SnapZone : MonoBehaviour
{
    public string acceptedId;
    public Transform snapAnchor;
    public GameObject silhouette;

    [Header("Visual")]
    public Renderer zoneRenderer;
    public Color defaultColor = Color.white;
    public Color correctColor = Color.blue;
    public Color wrongColor = Color.red;
    public Vector3 tableScale = Vector3.one;

    private void Start()
    {
        if (zoneRenderer != null)
        {
            zoneRenderer.material = new Material(zoneRenderer.material); // 🔥 clone
            SetColor(defaultColor);
        }
    }

    public bool Accepts(string organId)
    {
        return string.IsNullOrEmpty(acceptedId) || organId == acceptedId;
    }

    public void SetColor(Color color)
    {
        if (zoneRenderer != null)
        {
            zoneRenderer.material.color = color;
        }
    }

    public void OnObjectPlaced(string organId)
    {
        if (Accepts(organId))
        {
            SetColor(correctColor); // 🔵 benar
        }
        else
        {
            SetColor(wrongColor); // 🔴 salah
        }
    }

    public void OnObjectRemoved()
    {
        SetColor(defaultColor); // ⚪ reset
    }

    public void SetSilhouette(bool on)
    {
        if (silhouette) silhouette.SetActive(on);
    }
}