using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MagnetSnapDual : MonoBehaviour
{
    [Header("Home Anchor (tubuh)")]
    public Transform homeAnchor;

    [Header("Home Snap Settings")]
    public float snapDistance = 0.20f;
    public float snapAngle = 25f;
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
    private Coroutine _snapRoutine;

    private InteractableSfx _sfx;
    private SnapZone _currentZone;
    private OrganId _organId;

    void Start()
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
    }

    public void OnGrab()
    {
        _isGrabbed = true;
        StopSnapRoutine();

        if (_currentZone != null)
            _currentZone.SetSilhouette(true);
    }

    public void OnRelease()
    {
        _isGrabbed = false;

        if (_currentZone != null)
        {
            PrepareSfxForSnapIfNeeded();

            Transform anchor = _currentZone.snapAnchor != null ? _currentZone.snapAnchor : _currentZone.transform;
            Vector3 targetPos = anchor.position;
            Quaternion targetRot = anchor.rotation;

            _currentZone.SetSilhouette(false);

            Vector3 targetScale = _currentZone.tableScale;

            SnapTo(targetPos, targetRot, targetScale, playSnapSfx: true, onDone: () =>
            {
                if (_organId != null)
                    _currentZone.OnObjectPlaced(_organId.id); // 🔥 INI PENTING

                OnPlacedOnTable?.Invoke();
            });

            return;
        }

        if (IsNearHome())
        {
            PrepareSfxForSnapIfNeeded();
            SnapTo(homeAnchor.position, homeAnchor.rotation, _defaultScale, playSnapSfx: true, onDone: () =>
            {
                OnSnappedBackHome?.Invoke();
            });

        }
        else
        {
            _sfx?.PlayRelease();
        }
    }

    private bool IsZoneValid(SnapZone z)
    {
        if (_organId == null) return true;
        return z.Accepts(_organId.id);
    }

    private bool IsNearHome()
    {
        float d = Vector3.Distance(transform.position, _homePos);
        float a = restoreRotation ? Quaternion.Angle(transform.rotation, _homeRot) : 0f;

        bool passAngle = !restoreRotation || a <= snapAngle;
        return d <= snapDistance && passAngle;
    }

    private void PrepareSfxForSnapIfNeeded()
    {
        if (_sfx == null || !muteScaleSfxDuringSnap) return;
        float muteFor = Mathf.Max(0f, smoothTime) + Mathf.Max(0f, scaleSfxMuteExtra);
        _sfx.MuteScaleFor(muteFor);
    }

    private void SnapTo(Vector3 pos, Quaternion rot, Vector3 scale, bool playSnapSfx, System.Action onDone)
    {
        StopSnapRoutine();
        if (playSnapSfx) _sfx?.PlaySnap();

        if (smoothTime <= 0f)
        {
            ApplyTransform(pos, rot, scale);
            onDone?.Invoke();
        }
        else
        {
            _snapRoutine = StartCoroutine(SmoothSnap(pos, rot, scale, onDone));
        }
    }

    private IEnumerator SmoothSnap(Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, System.Action onDone)

    {
        float t = 0f;
        float duration = Mathf.Max(0.0001f, smoothTime);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 startScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            if (restorePosition) transform.position = Vector3.Lerp(startPos, targetPos, t);
            if (restoreRotation) transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            if (restoreScale) transform.localScale = Vector3.Lerp(startScale, targetScale, t);


            yield return null;
        }

        _snapRoutine = null;
        onDone?.Invoke();
    }

    private void ApplyTransform(Vector3 targetPos, Quaternion targetRot, Vector3 targetScale)
    {
        if (restorePosition) transform.position = targetPos;
        if (restoreRotation) transform.rotation = targetRot;
        if (restoreScale) transform.localScale = targetScale;
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
        var zone = other.GetComponent<SnapZone>();
        if (zone != null)
        {
            _currentZone = zone;
            zone.SetColor(Color.yellow); // preview sebelum drop
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var zone = other.GetComponent<SnapZone>();
        if (zone != null && _currentZone == zone)
        {
            zone.OnObjectRemoved(); // 🔥 reset warna
            _currentZone = null;
        }
    }
}
