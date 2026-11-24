
using UnityEngine;

public class PendulumBall : MonoBehaviour
{
    [Header("振り子設定")]
    public float swingAngle = 60f;
    public float swingSpeed = 1.5f;

    [Header("吹き飛ばし設定")]
    public float knockbackForce = 15f;
    public float upwardForce = 5f;

    [Header("開始方向")]
    public bool reverseStart = false;

    private Rigidbody rb;
    private Quaternion initialRotation;

    private float timer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        initialRotation = rb.rotation;
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime * swingSpeed;

        // 自然な振り子運動
        float direction = reverseStart ? -1f : 1f;

        float angle = Mathf.Sin(timer) * swingAngle * direction;

        Quaternion targetRotation =
            Quaternion.AngleAxis(angle, Vector3.forward)
            * initialRotation;

        rb.MoveRotation(targetRotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.transform.root.CompareTag("Player"))
            return;

        Rigidbody playerRb =
            collision.collider.GetComponentInParent<Rigidbody>();

        if (playerRb == null || playerRb.isKinematic)
            return;

        Vector3 direction =
            (playerRb.worldCenterOfMass - transform.position).normalized;

        Vector3 force =
            direction * knockbackForce +
            Vector3.up * upwardForce;

        playerRb.AddForce(force, ForceMode.Impulse);
    }
}
