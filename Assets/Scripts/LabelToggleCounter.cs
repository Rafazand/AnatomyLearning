using UnityEngine;

public class LabelToggleCounter : MonoBehaviour
{
    [SerializeField] private GameObject labelRoot;

    private int _count;

    void Awake()
    {
        if (!labelRoot) labelRoot = gameObject;
        ForceOff();
    }

    public void Show()
    {
        _count++;
        if (labelRoot) labelRoot.SetActive(true);
    }

    public void Hide()
    {
        _count = Mathf.Max(0, _count - 1);
        if (_count == 0 && labelRoot) labelRoot.SetActive(false);
    }

    public void ForceOff()
    {
        _count = 0;
        if (labelRoot) labelRoot.SetActive(false);
    }

    void OnDisable()
    {
        _count = 0;
    }
}
