using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class InteractableSfx : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip grabClip;
    public AudioClip releaseClip;
    public AudioClip scaleClip;
    public AudioClip snapClip;

    [Header("Scale SFX Settings")]
    public float minScaleDeltaToTrigger = 0.01f;
    public float scaleSfxCooldown = 0.08f;

    private AudioSource _audio;
    private Vector3 _lastScale;
    private float _nextScaleSfxTime;

    private float _muteScaleUntil;   // NEW

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _lastScale = transform.localScale;
    }

    void Update()
    {
        // kalau sedang mute scale (misal saat snap), jangan bunyikan zoom
        if (Time.time < _muteScaleUntil)
        {
            _lastScale = transform.localScale; // tetap update supaya tidak "meledak" setelah mute
            return;
        }

        float delta = (transform.localScale - _lastScale).magnitude;
        if (delta >= minScaleDeltaToTrigger && Time.time >= _nextScaleSfxTime)
        {
            PlayScale();
            _nextScaleSfxTime = Time.time + scaleSfxCooldown;
            _lastScale = transform.localScale;
        }
        else if (delta > 0f)
        {
            _lastScale = transform.localScale;
        }
    }

    public void PlayGrab() => PlayOneShot(grabClip);
    public void PlayRelease() => PlayOneShot(releaseClip);
    public void PlayScale() => PlayOneShot(scaleClip);
    public void PlaySnap() => PlayOneShot(snapClip);

    // NEW: panggil ini sebelum snap supaya zoom/scale tidak bunyi
    public void MuteScaleFor(float seconds)
    {
        _muteScaleUntil = Mathf.Max(_muteScaleUntil, Time.time + Mathf.Max(0f, seconds));
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;
        _audio.PlayOneShot(clip);
    }
}
