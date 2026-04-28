using System.Collections;
using UnityEngine;

public class MagnetSnapBack : MonoBehaviour
{
    [Header("Home Anchor (titik semula)")]
    public Transform homeAnchor;

    [Header("Magnet Settings")]
    [Tooltip("Jarak maksimum agar dianggap dekat dengan home dan boleh snap.")]
    public float snapDistance = 0.20f;

    [Tooltip("Toleransi beda rotasi (derajat). Set 180 kalau rotasi tidak mau jadi syarat.")]
    public float snapAngle = 25f;

    [Tooltip("Jika true, snap hanya ketika dilepas. Jika false, bisa snap juga saat masih dipegang.")]
    public bool snapOnlyOnRelease = true;

    [Tooltip("0 = snap langsung. >0 = snap halus (lebih natural).")]
    public float smoothTime = 0.12f;

    [Header("What to restore on snap")]
    public bool restorePosition = true;
    public bool restoreRotation = true;
    public bool restoreScale = true;

    [Header("SFX Behaviour")]
    [Tooltip("Mute scale/zoom SFX selama proses snap supaya tidak double dengan snap sound.")]
    public bool muteScaleSfxDuringSnap = true;

    [Tooltip("Tambahan waktu mute (detik) setelah smoothTime.")]
    public float scaleSfxMuteExtra = 0.05f;

    [Header("Debug (optional)")]
    public bool drawGizmos = false;

    private Vector3 _defaultPos;
    private Quaternion _defaultRot;
    private Vector3 _defaultScale;

    private bool _isGrabbed;
    private Coroutine _snapRoutine;

    // optional: kalau ada InteractableSfx pada object yang sama
    private InteractableSfx _sfx;

    void Start()
    {
        // Simpan default dari anchor (kalau ada)
        if (homeAnchor != null)
        {
            _defaultPos = homeAnchor.position;
            _defaultRot = homeAnchor.rotation;
        }
        else
        {
            _defaultPos = transform.position;
            _defaultRot = transform.rotation;
        }

        _defaultScale = transform.localScale;
        _sfx = GetComponent<InteractableSfx>();
    }

    void Update()
    {
        if (homeAnchor == null) return;

        // Mode magnet "tarik saat masih dipegang"
        if (!snapOnlyOnRelease && _isGrabbed)
        {
            if (IsNearHome())
            {
                // Jika snap saat masih dipegang, tetap mute scale SFX biar tidak spam
                PrepareSfxForSnapIfNeeded();
                SnapToHome();
            }
        }
    }

    // Hubungkan ini ke event Oculus: When Select()
    public void OnGrab()
    {
        _isGrabbed = true;
        StopSnapRoutine();
    }

    // Hubungkan ini ke event Oculus: When Unselect()
    public void OnRelease()
    {
        _isGrabbed = false;

        // Snap hanya jika dekat home
        if (IsNearHome())
        {
            // Saat snap: jangan mainkan release sound, cukup snap sound
            PrepareSfxForSnapIfNeeded();
            SnapToHome();
        }
        else
        {
            // Kalau tidak snap: mainkan release/unselect sound
            _sfx?.PlayRelease();
        }
    }

    private bool IsNearHome()
    {
        // Pakai nilai default yang disimpan (lebih aman daripada anchor yang bisa saja ikut bergerak)
        float d = Vector3.Distance(transform.position, _defaultPos);

        // Kalau kamu tidak mau syarat rotasi, set restoreRotation=false atau snapAngle=180
        float a = restoreRotation ? Quaternion.Angle(transform.rotation, _defaultRot) : 0f;

        bool passAngle = !restoreRotation || a <= snapAngle;
        return d <= snapDistance && passAngle;
    }

    private void PrepareSfxForSnapIfNeeded()
    {
        if (_sfx == null) return;
        if (!muteScaleSfxDuringSnap) return;

        // Mute scale SFX selama proses snap (smoothTime + buffer)
        float muteFor = Mathf.Max(0f, smoothTime) + Mathf.Max(0f, scaleSfxMuteExtra);
        _sfx.MuteScaleFor(muteFor);
    }

    private void SnapToHome()
    {
        StopSnapRoutine();

        // Bunyi snap saja (release tidak dimainkan di jalur snap)
        _sfx?.PlaySnap();

        if (smoothTime <= 0f)
        {
            ApplySnapTransform();
        }
        else
        {
            _snapRoutine = StartCoroutine(SmoothSnap());
        }
    }

    private IEnumerator SmoothSnap()
    {
        float t = 0f;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 startScale = transform.localScale;

        float duration = Mathf.Max(0.0001f, smoothTime);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            if (restorePosition)
                transform.position = Vector3.Lerp(startPos, _defaultPos, t);

            if (restoreRotation)
                transform.rotation = Quaternion.Slerp(startRot, _defaultRot, t);

            if (restoreScale)
                transform.localScale = Vector3.Lerp(startScale, _defaultScale, t);

            yield return null;
        }

        _snapRoutine = null;
    }

    private void ApplySnapTransform()
    {
        if (restorePosition)
            transform.position = _defaultPos;

        if (restoreRotation)
            transform.rotation = _defaultRot;

        if (restoreScale)
            transform.localScale = _defaultScale;
    }

    private void StopSnapRoutine()
    {
        if (_snapRoutine != null)
        {
            StopCoroutine(_snapRoutine);
            _snapRoutine = null;
        }
    }

    // Optional: jika mau update default saat runtime
    public void RebindHomeToCurrent()
    {
        _defaultPos = transform.position;
        _defaultRot = transform.rotation;
        _defaultScale = transform.localScale;

        if (homeAnchor != null)
        {
            homeAnchor.position = _defaultPos;
            homeAnchor.rotation = _defaultRot;
            homeAnchor.localScale = _defaultScale;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.yellow;
        Vector3 p = Application.isPlaying ? _defaultPos : (homeAnchor != null ? homeAnchor.position : transform.position);
        Gizmos.DrawWireSphere(p, snapDistance);
    }
}
