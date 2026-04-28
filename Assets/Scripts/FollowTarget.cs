using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FollowTarget : MonoBehaviour
{
    public Transform target;
    public bool followRotation = true;
    public Vector3 positionOffset;
    public Vector3 rotationOffsetEuler;

    Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;

        // Optional tapi bagus untuk gerakan halus
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        // Optional: kalau suka “nembus” karena cepat
        // _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    void FixedUpdate()
    {
        if (!target) return;

        _rb.MovePosition(target.position + positionOffset);

        if (followRotation)
            _rb.MoveRotation(target.rotation * Quaternion.Euler(rotationOffsetEuler));
    }
}

