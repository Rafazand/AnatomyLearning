using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FollowOVRSkeletonBoneRB : MonoBehaviour
{
    [Header("Assign OVRSkeleton from OVRLeftHandVisual / OVRRightHandVisual")]
    public OVRSkeleton skeleton;

    [Header("Bone to follow")]
    public OVRSkeleton.BoneId boneId = OVRSkeleton.BoneId.Hand_IndexTip;

    [Header("Optional offset (local to bone)")]
    public Vector3 localOffset;

    Rigidbody _rb;
    Transform _bone;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    void Update()
    {
        if (_bone != null) return;
        if (skeleton == null) return;

        var bones = skeleton.Bones;
        if (bones == null || bones.Count == 0) return;

        for (int i = 0; i < bones.Count; i++)
        {
            if (bones[i].Id == boneId)
            {
                _bone = bones[i].Transform;
                break;
            }
        }
    }

    void FixedUpdate()
    {
        if (_bone == null) return;

        _rb.MovePosition(_bone.TransformPoint(localOffset));
        _rb.MoveRotation(_bone.rotation);
    }
}
