using UnityEngine;

[ExecuteAlways]
public class ChainValidator : MonoBehaviour
{
    void Start()
    {
        Validate();
    }

    // call from Editor or runtime
    public void Validate()
    {
        var joints = GetComponentsInChildren<HingeJoint2D>();
        if (joints.Length == 0) Debug.LogWarning("ChainValidator: no HingeJoint2D children found.", this);
        foreach (var j in joints)
        {
            string connected = j.connectedBody == null ? "WORLD" : j.connectedBody.gameObject.name;
            Debug.Log($"Joint on {j.gameObject.name} -> connected to: {connected}; useMotor={j.useMotor}; useLimits={j.useLimits}; enabled={j.enabled}", j.gameObject);
        }
    }
}