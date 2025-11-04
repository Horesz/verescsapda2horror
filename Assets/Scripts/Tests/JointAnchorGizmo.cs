using UnityEngine;

[ExecuteInEditMode]
public class JointAnchorGizmo : MonoBehaviour
{
    public Color anchorColor = Color.yellow;
    public Color connectedAnchorColor = Color.cyan;
    public float radius = 0.06f;

    void OnDrawGizmos()
    {
        var joints = GetComponentsInChildren<HingeJoint2D>();
        foreach (var j in joints)
        {
            if (j == null) continue;
            Transform t = j.transform;
            // anchor world
            Vector3 anchorWorld = t.TransformPoint(new Vector3(j.anchor.x, j.anchor.y, 0f));
            Gizmos.color = anchorColor;
            Gizmos.DrawWireSphere(anchorWorld, radius);

            // connected anchor world
            Vector3 connectedWorld;
            if (j.connectedBody != null)
            {
                connectedWorld = j.connectedBody.transform.TransformPoint(new Vector3(j.connectedAnchor.x, j.connectedAnchor.y, 0f));
            }
            else
            {
                // connected to world: connectedAnchor is world space (approx)
                connectedWorld = new Vector3(j.connectedAnchor.x, j.connectedAnchor.y, 0f);
            }
            Gizmos.color = connectedAnchorColor;
            Gizmos.DrawWireSphere(connectedWorld, radius * 0.8f);

            // draw line
            Gizmos.color = Color.white;
            Gizmos.DrawLine(anchorWorld, connectedWorld);
        }
    }
}