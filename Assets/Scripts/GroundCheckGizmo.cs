using UnityEngine;

// Attach this to the Player (or any manager). Assign the GroundCheck transform and radius.
// Draws the ground-check circle in the Scene view (always visible when Gizmos enabled).
[ExecuteAlways]
public class GroundCheckGizmo : MonoBehaviour
{
    public Transform groundCheck;        // drag the GroundCheck transform here
    public float radius = 0.08f;         // match this to PlayerController.groundCheckRadius
    public Color gizmoColor = Color.green;

    void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(groundCheck.position, radius);
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.15f);
        Gizmos.DrawSphere(groundCheck.position, radius * 0.18f);
    }
}