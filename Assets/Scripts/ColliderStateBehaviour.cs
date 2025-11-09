using UnityEngine;

/// <summary>
/// Attach this as a StateMachineBehaviour to an Animator state to enable/disable named collider GameObjects
/// while that state is active (useful for attack hitboxes or temporary hurtboxes).
/// In the state's Behaviour inspector, list the child GameObject names (relative to the player root).
/// </summary>
public class ColliderStateBehaviour : StateMachineBehaviour
{
    [Tooltip("Names of child GameObjects that have colliders to ENABLE while this state is active.")]
    public string[] enableColliderObjects;

    [Tooltip("Names of child GameObjects that have colliders to DISABLE while this state is active.")]
    public string[] disableColliderObjects;

    Collider2D[] enabledCols;
    Collider2D[] disabledCols;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enabledCols = ResolveColliders(animator.gameObject, enableColliderObjects);
        SetCollidersEnabled(enabledCols, true);

        disabledCols = ResolveColliders(animator.gameObject, disableColliderObjects);
        SetCollidersEnabled(disabledCols, false);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetCollidersEnabled(enabledCols, false);
        SetCollidersEnabled(disabledCols, true);
    }

    Collider2D[] ResolveColliders(GameObject root, string[] names)
    {
        if (names == null || names.Length == 0) return new Collider2D[0];
        var list = new System.Collections.Generic.List<Collider2D>();
        foreach (var n in names)
        {
            if (string.IsNullOrEmpty(n)) continue;
            var child = root.transform.Find(n);
            if (child != null)
            {
                var col = child.GetComponent<Collider2D>();
                if (col != null) list.Add(col);
                else
                {
                    var cols = child.GetComponentsInChildren<Collider2D>();
                    if (cols != null && cols.Length > 0) list.AddRange(cols);
                }
            }
        }
        return list.ToArray();
    }

    void SetCollidersEnabled(Collider2D[] cols, bool enabled)
    {
        if (cols == null) return;
        foreach (var c in cols) if (c != null) c.enabled = enabled;
    }
}