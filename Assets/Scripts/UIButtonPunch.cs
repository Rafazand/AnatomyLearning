using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Efek "punch" scale saat tombol ditekan — feedback taktil sederhana (squash lalu kembali).
/// Tempel langsung di GameObject Button yang sama. Tidak perlu wiring OnClick tambahan,
/// memanfaatkan UGUI Event System (IPointerDownHandler) yang sama dipakai Button.OnClick.
/// </summary>
public class UIButtonPunch : MonoBehaviour, IPointerDownHandler
{
    [Range(0.5f, 1f)] public float punchScale = 0.9f;
    public float duration = 0.12f;

    private Vector3 _baseScale;
    private Coroutine _routine;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(PunchRoutine());
    }

    private IEnumerator PunchRoutine()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            float s = Mathf.Sin(p * Mathf.PI); // 0 -> 1 -> 0 (squash lalu kembali)
            transform.localScale = Vector3.LerpUnclamped(_baseScale, _baseScale * punchScale, s);
            yield return null;
        }

        transform.localScale = _baseScale;
        _routine = null;
    }
}
