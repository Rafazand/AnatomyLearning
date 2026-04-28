using System.Collections.Generic;
using UnityEngine;

public class OrganTouchFeedback : MonoBehaviour
{
    [Header("UI")]
    public GameObject label;
    public GameObject outlineObject;

    [Header("Touch filter")]
    public string requiredTag = "TouchProbe";

    [Header("Anti-stuck")]
    public float autoOffDelay = 0.15f;

    private readonly HashSet<Collider> touching = new HashSet<Collider>();
    private float lastStayTime;

    void Awake()
    {
        SetState(false);
        touching.Clear();
    }

    void OnEnable()
    {
        SetState(false);
        touching.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsValid(other)) return;
        touching.Add(other);
        lastStayTime = Time.time;
        SetState(true);

        Debug.Log($"ENTER {other.name}", this);
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsValid(other)) return;
        touching.Add(other);
        lastStayTime = Time.time;
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsValid(other)) return;
        touching.Remove(other);
    }

    void Update()
    {
        touching.RemoveWhere(c =>
            c == null || !c.enabled || !c.gameObject.activeInHierarchy);

        bool active =
            touching.Count > 0 &&
            (Time.time - lastStayTime) <= autoOffDelay;

        SetState(active);

        if (!active)
            touching.Clear();
    }

    bool IsValid(Collider other)
    {
        if (string.IsNullOrEmpty(requiredTag)) return true;
        return other.CompareTag(requiredTag);
    }

    void SetState(bool on)
    {
        if (label) label.SetActive(on);
        if (outlineObject) outlineObject.SetActive(on);
    }
}
