using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HitBox2D : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 15;
    [SerializeField] private GameObject owner;

    [Header("Behaviour")]
    [SerializeField] private bool startDisabled = true;
    [SerializeField] private bool singleHitPerActivation = true;

    [Header("Debug")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0.2f, 0.2f, 0.35f);

    private Collider2D col;
    private readonly HashSet<HurtBox2D> alreadyHit = new HashSet<HurtBox2D>();

    public int Damage => damage;
    public bool IsActive => col != null && col.enabled;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        if (owner ==null) owner = transform.root.gameObject;
        if (startDisabled) col.enabled = false;
    }

    /// On for the active frames. Called by PlayerAttack now,
    /// by an Animation Event later.
    public void Activate()
    {
        alreadyHit.Clear();
        col.enabled = true;
    }

    public void Deactivate()
    {
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var hurtbox = other.GetComponent<HurtBox2D>();
        if (hurtbox == null) return;

        //Safety net in case the layer matrix is misconfigured.
        if (hurtbox.transform.root == owner.transform.root) return;

        // One hit per targert per swing
        if (singleHitPerActivation && !alreadyHit.Add(hurtbox)) return;
        
        Vector2 contact = other.ClosestPoint(transform.position);
        hurtbox.ReceiveHit(new HitInfo(damage, owner, contact));
    }

    private void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = Application.isPlaying && col.enabled
            ? gizmoColor
            : new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.1f);

        Gizmos.matrix = transform.localToWorldMatrix;
        
        if(col is BoxCollider2D box)
            Gizmos.DrawCube(box.offset, box.size);
        else if (col is CircleCollider2D circle)
            Gizmos.DrawSphere(circle.offset, circle.radius);
    }
}
