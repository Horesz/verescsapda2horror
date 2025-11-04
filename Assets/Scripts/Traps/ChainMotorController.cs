using UnityEngine;

[RequireComponent(typeof(HingeJoint2D))]
public class ChainMotorController : MonoBehaviour
{
    public HingeJoint2D topJoint;         // assign Link0's hinge (or leave auto)
    public float motorSpeed = 80f;        // degrees/sec
    public float maxMotorTorque = 1000f;
    public float deadzone = 1f;

    HingeJoint2D joint;

    void Awake()
    {
        joint = topJoint != null ? topJoint : GetComponent<HingeJoint2D>();
        if (joint == null)
        {
            Debug.LogError("ChainMotorController: no HingeJoint2D assigned/found.", this);
            enabled = false;
            return;
        }

        joint.useMotor = true;
        JointMotor2D m = joint.motor;
        m.motorSpeed = motorSpeed;
        m.maxMotorTorque = maxMotorTorque;
        joint.motor = m;
    }

    void FixedUpdate()
    {
        if (joint == null) return;

        float angle = joint.jointAngle;
        float lower = joint.limits.min;
        float upper = joint.limits.max;
        JointMotor2D m = joint.motor;

        if (angle >= upper - deadzone && m.motorSpeed > 0f)
        {
            m.motorSpeed = -Mathf.Abs(m.motorSpeed);
            joint.motor = m;
        }
        else if (angle <= lower + deadzone && m.motorSpeed < 0f)
        {
            m.motorSpeed = Mathf.Abs(m.motorSpeed);
            joint.motor = m;
        }
    }
}