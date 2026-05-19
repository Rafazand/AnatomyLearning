using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MagnetSnapDual : MonoBehaviour
{
    [Header("Home Anchor")]
    public Transform homeAnchor;

    [Header("Home Snap Settings")]
    public float snapDistance = 0.4f;
    public float snapAngle = 180f;
    public float smoothTime = 0.12f;

    [Header("Restore On Snap")]
    public bool restorePosition = true;
    public bool restoreRotation = true;
    public bool restoreScale = true;

    [Header("SFX Behaviour")]
    public bool muteScaleSfxDuringSnap = true;
    public float scaleSfxMuteExtra = 0.05f;

    [Header("Events")]
    public UnityEvent OnPlacedOnTable;
    public UnityEvent OnSnappedBackHome;

    private Vector3 _homePos;
    private Quaternion _homeRot;
    private Vector3 _defaultScale;

    private bool _isGrabbed;
    private bool _isLocked;

    private Coroutine _snapRoutine;

    private InteractableSfx _sfx;
    private SnapZone _currentZone;
    private OrganId _organId;

    private void Start()
    {
        if (homeAnchor != null)
        {
            _homePos = homeAnchor.position;
            _homeRot = homeAnchor.rotation;
        }
        else
        {
            _homePos = transform.position;
            _homeRot = transform.rotation;
        }

        _defaultScale = transform.localScale;
        _sfx = GetComponent<InteractableSfx>();

        _organId = GetComponent<OrganId>();
        if (_organId == null) _organId = GetComponentInParent<OrganId>();
        if (_organId == null) _organId = GetComponentInChildren<OrganId>();

        if (_organId == null)
        {
            Debug.LogWarning($"{gameObject.name} tidak punya OrganId.");
        }
    }

    public void OnGrab()
    {
        if (_isLocked) return;

        _isGrabbed = true;
        StopSnapRoutine();

        if (_currentZone != null)
        {
            _currentZone.SetSilhouette(true);
        }
    }

    public void OnRelease()
    {
        if (_isLocked) return;

        _isGrabbed = false;

        if (_currentZone != null)
        {
            SnapZone targetZone = _currentZone;

            PrepareSfxForSnapIfNeeded();

            Transform anchor = targetZone.snapAnchor != null
                ? targetZone.snapAnchor
                : targetZone.transform;

            Vector3 targetPos = anchor.position;
            Quaternion targetRot = Quaternion.Euler(targetZone.answerRotationEuler);
            Vector3 targetScale = targetZone.answerScale;

            targetZone.SetSilhouette(false);

            SnapToAnswerBox(targetPos, targetRot, targetScale, true, () =>
            {
                if (_organId != null)
                {
                    Debug.Log($"Submit organ ke quiz: {_organId.id}");
                    targetZone.OnObjectPlaced(_organId.id, this);
                }

                OnPlacedOnTable?.Invoke();
            });

            return;
        }

        if (IsNearHome())
        {
            PrepareSfxForSnapIfNeeded();

            SnapToHome(_homePos, _homeRot, _defaultScale, true, () =>
            {
                OnSnappedBackHome?.Invoke();
            });

            return;
        }

        _sfx?.PlayRelease();
    }

    public void SetLocked(bool locked)
    {
        _isLocked = locked;
    }

    public void ForceReturnHome()
    {
        StopSnapRoutine();

        SnapToHome(_homePos, _homeRot, _defaultScale, false, () =>
        {
            OnSnappedBackHome?.Invoke();
        });
    }

    private bool IsNearHome()
    {
        float distance = Vector3.Distance(transform.position, _homePos);
        float angle = restoreRotation ? Quaternion.Angle(transform.rotation, _homeRot) : 0f;

        bool passAngle = !restoreRotation || angle <= snapAngle;

        return distance <= snapDistance && passAngle;
    }

    private void PrepareSfxForSnapIfNeeded()
    {
        if (_sfx == null || !muteScaleSfxDuringSnap) return;

        float muteFor = Mathf.Max(0f, smoothTime) + Mathf.Max(0f, scaleSfxMuteExtra);
        _sfx.MuteScaleFor(muteFor);
    }

    private void SnapToAnswerBox(
        Vector3 targetPos,
        Quaternion targetRot,
        Vector3 targetScale,
        bool playSnapSfx,
        System.Action onDone
    )
    {
        StopSnapRoutine();

        if (playSnapSfx)
        {
            _sfx?.PlaySnap();
        }

        StartCoroutine(SnapAnswerRoutine(targetPos, targetRot, targetScale, onDone));
    }

    private IEnumerator SnapAnswerRoutine(
        Vector3 targetPos,
        Quaternion targetRot,
        Vector3 targetScale,
        System.Action onDone
    )
    {
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 visualCenterOffset = GetRendererCenterOffset();
        Vector3 finalPos = targetPos - visualCenterOffset;

        float t = 0f;
        float duration = Mathf.Max(0.0001f, smoothTime);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            if (restoreScale)
            {
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            }

            if (restorePosition)
            {
                transform.position = Vector3.Lerp(startPos, finalPos, t);
            }

            if (restoreRotation)
            {
                transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            }

            yield return null;
        }

        if (restoreScale)
        {
            transform.localScale = targetScale;
        }

        if (restoreRotation)
        {
            transform.rotation = targetRot;
        }

        if (restorePosition)
        {
            Vector3 correctedOffset = GetRendererCenterOffset();
            transform.position = targetPos - correctedOffset;
        }

        _snapRoutine = null;
        onDone?.Invoke();
    }

    private void SnapToHome(
        Vector3 targetPos,
        Quaternion targetRot,
        Vector3 targetScale,
        bool playSnapSfx,
        System.Action onDone
    )
    {
        StopSnapRoutine();

        if (playSnapSfx)
        {
            _sfx?.PlaySnap();
        }

        if (smoothTime <= 0f)
        {
            ApplyTransform(targetPos, targetRot, targetScale);
            onDone?.Invoke();
        }
        else
        {
            _snapRoutine = StartCoroutine(SmoothSnap(targetPos, targetRot, targetScale, onDone));
        }
    }

    private IEnumerator SmoothSnap(
        Vector3 targetPos,
        Quaternion targetRot,
        Vector3 targetScale,
        System.Action onDone
    )
    {
        float t = 0f;
        float duration = Mathf.Max(0.0001f, smoothTime);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 startScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            if (restorePosition)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, t);
            }

            if (restoreRotation)
            {
                transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            }

            if (restoreScale)
            {
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            }

            yield return null;
        }

        ApplyTransform(targetPos, targetRot, targetScale);

        _snapRoutine = null;
        onDone?.Invoke();
    }

    private Vector3 GetRendererCenterOffset()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
        {
            return Vector3.zero;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.center - transform.position;
    }

    private void ApplyTransform(Vector3 targetPos, Quaternion targetRot, Vector3 targetScale)
    {
        if (restorePosition)
        {
            transform.position = targetPos;
        }

        if (restoreRotation)
        {
            transform.rotation = targetRot;
        }

        if (restoreScale)
        {
            transform.localScale = targetScale;
        }
    }

    private void StopSnapRoutine()
    {
        if (_snapRoutine != null)
        {
            StopCoroutine(_snapRoutine);
            _snapRoutine = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SnapZone zone = other.GetComponentInParent<SnapZone>();

        if (zone != null)
        {
            _currentZone = zone;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SnapZone zone = other.GetComponentInParent<SnapZone>();

        if (zone != null && _currentZone == zone)
        {
            if (!_isLocked && !_isGrabbed)
            {
                _currentZone = null;
            }
        }
    }
}