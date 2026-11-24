
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class RagdollController : MonoBehaviour
{
    [Header("操作する腰のRigidbody")]
    public Rigidbody pelvis;

    [Header("移動設定")]
    public float moveForce = 25f;
    public float maxSpeed = 4f;

    [Header("カメラ")]
    public Transform cameraTransform;

    [Header("ジャンプ設定")]
    public float jumpForce = 5f;
    public float jumpCooldown = 1f;

    [Header("接地判定")]
    public LayerMask groundLayer = ~0;

    [Header("リスポーン設定")]
    public Transform respawnPoint;

    private HashSet<GroundContactRelay> groundedParts =
        new HashSet<GroundContactRelay>();

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float lastJumpTime = -999f;

    // 最初の身体の状態を保存
    private Rigidbody[] allBodies;
    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private Vector3 initialPelvisPosition;

    private bool IsGrounded
    {
        get { return groundedParts.Count > 0; }
    }

    private void Start()
    {
        if (pelvis != null)
        {
            startPosition = pelvis.position;
            startRotation = pelvis.rotation;
        }

        // 全身のRigidbodyを取得
        allBodies = GetComponentsInChildren<Rigidbody>(true);

        initialPositions = new Vector3[allBodies.Length];
        initialRotations = new Quaternion[allBodies.Length];

        // 全身の初期位置・回転を保存
        for (int i = 0; i < allBodies.Length; i++)
        {
            initialPositions[i] = allBodies[i].position;
            initialRotations[i] = allBodies[i].rotation;
        }

        initialPelvisPosition = pelvis.position;

        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            if (col.isTrigger) continue;

            GroundContactRelay relay =
                col.GetComponent<GroundContactRelay>();

            if (relay == null)
            {
                relay = col.gameObject.AddComponent<GroundContactRelay>();
            }

            relay.Initialize(this);
        }
    }

    private void Update()
    {
        if (pelvis == null || Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (IsGrounded &&
                Time.time >= lastJumpTime + jumpCooldown)
            {
                pelvis.AddForce(
                    Vector3.up * jumpForce,
                    ForceMode.Impulse
                );

                lastJumpTime = Time.time;
                groundedParts.Clear();
            }
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Respawn();
        }
    }

    private void FixedUpdate()
    {
        if (pelvis == null) return;

        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
            if (Keyboard.current.sKey.isPressed) z -= 1f;
            if (Keyboard.current.wKey.isPressed) z += 1f;
        }

        // カメラの向きを取得
        Transform cam = cameraTransform != null
            ? cameraTransform
            : Camera.main.transform;

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        // 上下方向の傾きをなくす
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // カメラ基準の移動方向
        Vector3 direction =
            (forward * z + right * x).normalized;

        Vector3 velocity = pelvis.linearVelocity;

        Vector3 horizontalVelocity =
            new Vector3(velocity.x, 0f, velocity.z);

        if (direction.sqrMagnitude > 0.01f &&
            horizontalVelocity.magnitude < maxSpeed)
        {
            pelvis.AddForce(
                direction * moveForce,
                ForceMode.Acceleration
            );
        }
    }

    public bool IsGroundLayer(int layer)
    {
        return (groundLayer.value & (1 << layer)) != 0;
    }

    public void SetPartGrounded(
        GroundContactRelay part, bool grounded)
    {
        if (grounded)
            groundedParts.Add(part);
        else
            groundedParts.Remove(part);
    }

    public void Respawn()
    {
        if (pelvis == null || allBodies == null) return;

        Vector3 targetPosition = respawnPoint != null
            ? respawnPoint.position
            : initialPelvisPosition;

        Vector3 offset = targetPosition - initialPelvisPosition;

        // 全身の物理演算を一時停止
        foreach (Rigidbody body in allBodies)
        {
            body.isKinematic = true;
        }

        // 全身の位置と回転を戻す
        for (int i = 0; i < allBodies.Length; i++)
        {
            Rigidbody body = allBodies[i];

            body.position = initialPositions[i] + offset;
            body.rotation = initialRotations[i];

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        Physics.SyncTransforms();

        // 物理演算を再開
        foreach (Rigidbody body in allBodies)
        {
            body.isKinematic = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.WakeUp();
        }

        Physics.SyncTransforms();

        groundedParts.Clear();
        lastJumpTime = -999f;
    }
}
