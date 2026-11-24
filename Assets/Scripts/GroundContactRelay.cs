
using System.Collections.Generic;
using UnityEngine;

public class GroundContactRelay : MonoBehaviour
{
    private RagdollController controller;
    private HashSet<Collider> groundColliders = new HashSet<Collider>();

    public void Initialize(RagdollController parent)
    {
        controller = parent;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (controller == null) return;

        Collider other = collision.collider;

        if (controller.IsGroundLayer(other.gameObject.layer))
        {
            groundColliders.Add(other);
            controller.SetPartGrounded(this, true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        groundColliders.Remove(collision.collider);

        if (groundColliders.Count == 0 && controller != null)
        {
            controller.SetPartGrounded(this, false);
        }
    }

    public void ClearContacts()
    {
        groundColliders.Clear();

        if (controller != null)
        {
            controller.SetPartGrounded(this, false);
        }
    }
}
