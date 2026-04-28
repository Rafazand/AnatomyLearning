using UnityEngine;

public class OrganId : MonoBehaviour
{
    [Tooltip("Kalau kosong, id akan di-generate otomatis dari nama GameObject.")]
    public string id;

    void Awake()
    {
        if (!string.IsNullOrEmpty(id)) return;

        // contoh: "Liver_Interactable" -> "Liver"
        string n = gameObject.name;
        int idx = n.IndexOf("_Interactable");
        if (idx > 0) n = n.Substring(0, idx);

        id = n;
    }
}
