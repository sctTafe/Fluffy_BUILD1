using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Physicanimator : MonoBehaviour
{
    [System.Serializable]
    public struct JointData
    {
        public ConfigurableJoint joint;
        public Transform animBone;
        public float weight;
        public Quaternion initialRotation;
    }

    public JointData[] joints;

    [Range(0.0f, 9999.0f)] public float jointSpringsStrength = 420;
    [Range(0.0f, 1000.0f)] public float jointSpringDamper = 1;

    public Material debugMat;
    public Transform staticAnimRoot;
    public bool limp;
    public bool lockHipsToAnim;
    public bool debugDraw = true; // Toggle debug visualization

    void Start()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            joints[i].initialRotation = joints[i].joint.transform.localRotation;
            //joints[i].weight = 1.0f; // Default: full animation influence
        }

        SetJointSprings();
    }

    void FixedUpdate()
    {
        UpdateJointTargets();
        ResetStaticAnimPos();
    }

    private void UpdateJointTargets()
    {
        for (int i = limp ? 1 : 0; i < joints.Length; i++)
        {
            JointData jd = joints[i];
            Quaternion targetRotation = jd.animBone.localRotation;
            float weight = jd.weight;

            // Blend between physics and animation based on weight
            Quaternion blendedRotation = Quaternion.Slerp(jd.joint.transform.localRotation, targetRotation, weight);
            ConfigurableJointExtensions.SetTargetRotationLocal(jd.joint, blendedRotation, jd.initialRotation);
        }
    }

    void SetJointSprings()
    {
        for (int i = 0; i < joints.Length; i++)
            SetJointParams(joints[i].joint, jointSpringsStrength, jointSpringDamper);
    }

    public void SetBoneWeight(int index, float weight)
    {
        if (index >= 0 && index < joints.Length)
            joints[index].weight = Mathf.Clamp01(weight);
    }

    public void SetJointParams(ConfigurableJoint cj, float posSpring, float posDamper)
    {
        JointDrive jDrivex = cj.angularXDrive;
        JointDrive jDriveyz = cj.angularYZDrive;
        jDrivex.positionSpring = posSpring;
        jDriveyz.positionSpring = posSpring;
        jDrivex.positionDamper = posDamper;
        jDriveyz.positionDamper = posDamper;
        cj.angularXDrive = jDrivex;
        cj.angularYZDrive = jDriveyz;
    }

    void ResetStaticAnimPos()
    {
        if (joints.Length > 0)
        {
            joints[0].animBone.parent.localPosition = new Vector3(
                joints[0].joint.transform.localPosition.x,
                0.9648402f,
                joints[0].joint.transform.localPosition.z);
        }
    }

    // ** DEBUG DRAW GIZMOS **
    void OnDrawGizmos()
    {
        if (!debugDraw || joints == null) return;

        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i].animBone == null || joints[i].joint == null) continue;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(joints[i].joint.transform.position, 0.02f);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(joints[i].animBone.position, 0.02f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(joints[i].joint.transform.position, joints[i].animBone.position);
        }
    }
}
