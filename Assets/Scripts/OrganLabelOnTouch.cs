using System.Collections.Generic;
using UnityEngine;

public class OrganLabelOnTouch : MonoBehaviour
{
    [Header("Assign the label GameObject (TMP object)")]
    public GameObject label;

    [Header("Optional: filter by tag (ex: TouchProbe). Leave empty to accept any collider.")]
    public string requiredTag = "TouchProbe";

    // Track collider yang sedang overlap
    private readonly HashSet<Collider> _touching = new HashSet<Collider>();

    void Awake()
    {
        ForceOff();
    }

    void OnEnable()
    {
        ForceOff();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsValidTouch(other)) return;

        _touching.Add(other);
        UpdateLabel();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsValidTouch(other)) return;

        _touching.Remove(other);
        UpdateLabel();
    }

    // Kalau ada collider yang tiba-tiba nonaktif/destroy tanpa exit, kita bersihin di sini
    void LateUpdate()
    {
        if (_touching.Count == 0) return;

        bool removedAny = false;
        _touching.RemoveWhere(c =>
        {
            bool dead = (c == null) || !c.enabled || !c.gameObject.activeInHierarchy;
            if (dead) removedAny = true;
            return dead;
        });

        if (removedAny) UpdateLabel();
    }

    private bool IsValidTouch(Collider other)
    {
        if (string.IsNullOrEmpty(requiredTag)) return true;
        return other.CompareTag(requiredTag);
    }

    private void UpdateLabel()
    {
        if (!label) return;
        label.SetActive(_touching.Count > 0);
    }

    private void ForceOff()
    {
        _touching.Clear();
        if (label) label.SetActive(false);
    }
}
