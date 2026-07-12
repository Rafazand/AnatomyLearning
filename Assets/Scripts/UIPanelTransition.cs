using System.Collections;
using UnityEngine;

/// <summary>
/// Animasi fade + scale-in otomatis setiap kali panel ini diaktifkan (SetActive(true)).
/// Tempel di GameObject panel yang punya CanvasGroup (mis. Menu1Panel, Menu2Panel).
/// Tidak perlu perubahan di MenuManager — cukup jalan lewat OnEnable().
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIPanelTransition : MonoBehaviour
{
    [Header("Fade + Scale In")]
    public float duration = 0.25f;
    [Range(0.5f, 1f)] public float startScale = 0.85f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup _canvasGroup;
    private Vector3 _targetScale;
    private Coroutine _routine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _targetScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(PlayIn());
    }

    private IEnumerator PlayIn()
    {
        float t = 0f;
        _canvasGroup.alpha = 0f;
        transform.localScale = _targetScale * startScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = easeCurve.Evaluate(Mathf.Clamp01(t / duration));
            _canvasGroup.alpha = p;
            transform.localScale = Vector3.LerpUnclamped(_targetScale * startScale, _targetScale, p);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        transform.localScale = _targetScale;
        _routine = null;
    }
}
