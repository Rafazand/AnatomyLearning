using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Data classes harus PUBLIC dan berada di luar class agar JsonUtility bisa deserialize.
// Private/nested class tidak didukung Unity serialization system.

[System.Serializable]
public class OrganInfoEntry
{
    public string id;
    public string title;
    public string description;
}

[System.Serializable]
public class OrganInfoCollection
{
    public OrganInfoEntry[] organs;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Panel info organ untuk mode Explore.
/// Menampilkan nama dan deskripsi organ berdasarkan data dari organ_data.json.
///
/// PENTING: Script ini boleh berada di GameObject yang mulai inactive —
/// Instance getter menggunakan FindObjectOfType(includeInactive:true) sebagai fallback.
/// </summary>
public class BodyFrontInfoController : MonoBehaviour
{
    // ── Singleton — tahan jika panel mulai inactive ─────────────────────────────
    private static BodyFrontInfoController _instance;
    public static BodyFrontInfoController Instance
    {
        get
        {
            if (_instance == null)
            {
                // FindObjectOfType dengan includeInactive = true:
                // menemukan komponen bahkan jika Organ_InfoPanel sedang inactive saat scene load.
                _instance = FindObjectOfType<BodyFrontInfoController>(true);

                // Jika Awake belum jalan (GO masih inactive), inisialisasi database sekarang.
                if (_instance != null && _instance._db == null)
                    _instance.LoadDatabase();
            }
            return _instance;
        }
    }

    // ── Inspector References ────────────────────────────────────────────────────
    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Audio (tidak digunakan — narasi dinonaktifkan)")]
    public AudioSource audioSource;
    public AudioClip narration;

    // ── Internal ────────────────────────────────────────────────────────────────
    private Dictionary<string, OrganInfoEntry> _db;
    private string _currentOrganId;

    // ── Lifecycle ───────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Singleton guard
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        LoadDatabase();

        // Panel disembunyikan saat start — ditampilkan oleh ShowInfo()
        if (panel != null) panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    // ── Database ────────────────────────────────────────────────────────────────

    private void LoadDatabase()
    {
        _db = new Dictionary<string, OrganInfoEntry>();

        TextAsset json = Resources.Load<TextAsset>("organ_data");
        if (json == null)
        {
            Debug.LogError("[OrganInfoPanel] organ_data.json TIDAK DITEMUKAN di Assets/Resources/. " +
                           "Klik kanan file di Project window → Reimport.");
            return;
        }

        OrganInfoCollection collection = JsonUtility.FromJson<OrganInfoCollection>(json.text);
        if (collection == null || collection.organs == null || collection.organs.Length == 0)
        {
            Debug.LogError("[OrganInfoPanel] JSON terbaca tapi array 'organs' kosong. " +
                           "Periksa format organ_data.json.");
            return;
        }

        foreach (OrganInfoEntry entry in collection.organs)
            if (!string.IsNullOrEmpty(entry.id))
                _db[entry.id] = entry;

        Debug.Log($"[OrganInfoPanel] Loaded {_db.Count} organ dari organ_data.json ✓");
    }

    // ── Public API ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Tampilkan panel dengan info organ sesuai organId.
    /// organId harus cocok persis (case-sensitive) dengan field 'id' di organ_data.json.
    /// </summary>
    public void ShowInfo(string organId)
    {
        // Lazy init — jika dipanggil sebelum Awake jalan (GO inactive)
        if (_db == null) LoadDatabase();

        // Cek referensi panel
        if (panel == null)
        {
            Debug.LogError("[OrganInfoPanel] Field 'Panel' kosong di Inspector BodyFrontInfoController!");
            return;
        }

        _currentOrganId = organId;

        if (_db.TryGetValue(organId, out OrganInfoEntry entry))
        {
            if (titleText != null)
                titleText.text = entry.title;
            else
                Debug.LogError("[OrganInfoPanel] 'Title Text' null — assign TitleText (TMP) di Inspector!");

            if (descriptionText != null)
                descriptionText.text = entry.description;
            else
                Debug.LogError("[OrganInfoPanel] 'Description Text' null — assign DescriptionText (TMP) di Inspector!");
        }
        else
        {
            Debug.LogWarning($"[OrganInfoPanel] Organ id='{organId}' tidak ada di organ_data.json. " +
                             $"IDs yang tersedia: {string.Join(", ", _db.Keys)}");
            if (titleText != null) titleText.text = organId;
            if (descriptionText != null) descriptionText.text = "(data tidak ditemukan)";
        }

        panel.SetActive(true);
    }

    /// <summary>
    /// Sembunyikan panel HANYA jika sedang menampilkan organ ini.
    /// Dipanggil dari SnapZone saat organ diambil lagi.
    /// </summary>
    public void HideInfo(string organId)
    {
        if (string.IsNullOrEmpty(organId)) return;
        if (_currentOrganId != organId) return;
        ForceHide();
    }

    /// <summary>
    /// Paksa sembunyikan panel tanpa syarat.
    /// Dipanggil MenuManager saat keluar dari mode Explore.
    /// </summary>
    public void ForceHide()
    {
        _currentOrganId = null;
        if (panel != null) panel.SetActive(false);
    }

    // ── Backward compat ─────────────────────────────────────────────────────────

    public void StopNarration()
    {
        if (audioSource != null) audioSource.Stop();
    }
}
