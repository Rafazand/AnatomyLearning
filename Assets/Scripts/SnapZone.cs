using System.Collections;
using UnityEngine;

public class SnapZone : MonoBehaviour
{
    [Header("Snap")]
    public string acceptedId;
    public Transform snapAnchor;
    public GameObject silhouette;

    [Header("Quiz")]
    public QuizManager quizManager;
    public bool submitToQuiz = true;

    [Header("Answer Display")]
    public Vector3 answerScale = new Vector3(0.2f, 0.2f, 0.2f);
    public Vector3 answerRotationEuler = new Vector3(0f, 180f, 0f); // tidak dipakai saat snapAnchor ada, tapi dipertahankan untuk fallback

    [Header("Visual")]
    public Renderer zoneRenderer;
    public Color defaultColor = Color.white;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    [Header("Explore Feedback")]
    [Tooltip("Durasi tampil warna benar/salah sebelum zona direset (detik).")]
    public float feedbackDuration = 1f;
    [Tooltip("Jarak getaran kotak saat jawaban salah (meter). Nilai kecil sudah cukup terasa, misal 0.01.")]
    public float shakeMagnitude = 0.01f;
    [Tooltip("Kecepatan siklus getaran. Makin besar makin cepat bergetar.")]
    public float shakeSpeed = 40f;
    [Tooltip("SFX yang dimainkan saat organ diletakkan di kotak yang BENAR.")]
    public AudioClip correctClip;
    [Tooltip("SFX yang dimainkan saat organ diletakkan di kotak yang SALAH.")]
    public AudioClip wrongClip;
    [Tooltip("AudioSource untuk memutar SFX feedback. Biarkan kosong — akan dibuat otomatis jika tidak ada.")]
    public AudioSource feedbackAudioSource;

    private bool hasAnswered = false;
    private MagnetSnapDual currentOrgan;
    private string _placedOrganId;      // id organ yang sedang terpasang di zona ini
    private Vector3 _originalLocalPos;
    private Coroutine _feedbackRoutine;

    private void Awake()
    {
        if (quizManager == null)
            quizManager = FindObjectOfType<QuizManager>();

        // Setup AudioSource — pakai yang sudah ada, atau buat baru
        if (feedbackAudioSource == null)
            feedbackAudioSource = GetComponent<AudioSource>();
        if (feedbackAudioSource == null)
            feedbackAudioSource = gameObject.AddComponent<AudioSource>();

        feedbackAudioSource.playOnAwake = false;
        feedbackAudioSource.spatialBlend = 1f; // suara 3D mengikuti posisi zona
    }

    private void Start()
    {
        _originalLocalPos = transform.localPosition;

        if (zoneRenderer != null)
        {
            zoneRenderer.material = new Material(zoneRenderer.material);
            ResetZone();
        }
    }

    public bool Accepts(string organId)
    {
        // Quiz mode: QuizManager yang menentukan benar/salah
        if (submitToQuiz) return true;
        // Explore mode: semua organ boleh masuk — kebenaran dicek di OnObjectPlaced
        return true;
    }

    public void OnObjectPlaced(string organId, MagnetSnapDual organ)
    {
        if (hasAnswered) return;

        hasAnswered = true;
        currentOrgan = organ;
        _placedOrganId = organId;

        if (submitToQuiz)
        {
            // Quiz mode: kunci organ dan laporkan ke QuizManager
            if (currentOrgan != null)
                currentOrgan.SetLocked(true);
            if (quizManager != null)
                quizManager.SubmitAnswer(organId, this);
        }
        else
        {
            // Explore mode: beri feedback visual + SFX berdasarkan kebenaran
            bool isCorrect = !string.IsNullOrEmpty(acceptedId) && organId == acceptedId;

            if (_feedbackRoutine != null) StopCoroutine(_feedbackRoutine);
            _feedbackRoutine = StartCoroutine(
                isCorrect ? CorrectFeedbackRoutine() : WrongFeedbackRoutine()
            );
        }
    }

    // ── Explore Feedback Routines ──────────────────────────────────────────────

    // Organ diletakkan di kotak yang BENAR
    private IEnumerator CorrectFeedbackRoutine()
    {
        SetColor(correctColor);
        PlayClip(correctClip);

        // Tampilkan info panel segera — organ yang benar baru saja dipasang
        BodyFrontInfoController.Instance?.ShowInfo(_placedOrganId);

        yield return new WaitForSeconds(feedbackDuration);

        SetColor(defaultColor);
        _feedbackRoutine = null;
        // hasAnswered tetap true — organ tetap terpasang di zona
        // Info panel tetap tampil sampai organ diambil
    }

    // Organ diletakkan di kotak yang SALAH
    private IEnumerator WrongFeedbackRoutine()
    {
        SetColor(wrongColor);
        PlayClip(wrongClip);

        // Animasi getar — magnitude berkurang seiring waktu (ease-out)
        float elapsed = 0f;
        while (elapsed < feedbackDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / feedbackDuration;
            float shake = Mathf.Sin(elapsed * shakeSpeed) * shakeMagnitude * (1f - normalizedTime);
            transform.localPosition = _originalLocalPos + new Vector3(shake, 0f, 0f);
            yield return null;
        }

        transform.localPosition = _originalLocalPos;
        _feedbackRoutine = null;

        // Kembalikan organ ke home dan reset zona agar bisa diisi lagi
        ReturnCurrentOrganHome();
        ResetZone();
    }

    private void PlayClip(AudioClip clip)
    {
        if (feedbackAudioSource != null && clip != null)
            feedbackAudioSource.PlayOneShot(clip);
    }

    // ── Organ Lifecycle ────────────────────────────────────────────────────────

    // Dipanggil oleh MagnetSnapDual.OnGrab() saat organ diambil dari slot (Explore mode)
    public void OnOrganPickedUp()
    {
        // Hentikan feedback yang sedang berjalan dan pulihkan posisi zona
        if (_feedbackRoutine != null)
        {
            StopCoroutine(_feedbackRoutine);
            _feedbackRoutine = null;
            transform.localPosition = _originalLocalPos;
        }

        // Sembunyikan info panel — hanya jika organ yang diambil adalah yang sedang ditampilkan
        if (!submitToQuiz)
            BodyFrontInfoController.Instance?.HideInfo(_placedOrganId);

        _placedOrganId = null;
        hasAnswered = false;
        currentOrgan = null;
        SetColor(defaultColor);
    }

    public void ReturnCurrentOrganHome()
    {
        if (currentOrgan != null)
        {
            currentOrgan.SetLocked(false);
            currentOrgan.ForceReturnHome();
            currentOrgan = null;
        }
    }

    // ── Color & Silhouette ─────────────────────────────────────────────────────

    public void SetCorrectColor() => SetColor(correctColor);
    public void SetWrongColor()   => SetColor(wrongColor);

    public void ResetZone()
    {
        // Hentikan feedback yang sedang berjalan sebelum reset
        if (_feedbackRoutine != null)
        {
            StopCoroutine(_feedbackRoutine);
            _feedbackRoutine = null;
            transform.localPosition = _originalLocalPos;
        }

        hasAnswered = false;
        _placedOrganId = null;
        SetColor(defaultColor);
        SetSilhouette(false);
    }

    public void SetSilhouette(bool on)
    {
        if (silhouette != null)
            silhouette.SetActive(on);
    }

    public void SetColor(Color color)
    {
        if (zoneRenderer != null)
            zoneRenderer.material.color = color;
    }
}
