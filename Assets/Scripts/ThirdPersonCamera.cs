
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("追従する対象")]
    public Transform target;

    [Header("カメラ設定")]
    public float distance = 4f;
    public float height = 1.3f;
    public float sensitivity = 0.15f;

    [Header("ズーム設定")]
    public float zoomSpeed = 1f;
    public float minDistance = 1.5f;
    public float maxDistance = 8f;

    [Header("カメラの滑らかさ")]
    public float smoothSpeed = 12f;

    [Header("上下の回転制限")]
    public float minPitch = -20f;
    public float maxPitch = 65f;

    private float yaw;
    private float pitch = 15f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // マウスホイールでズーム
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;

            distance -= scroll * zoomSpeed * 0.01f;

            distance = Mathf.Clamp(
                distance,
                minDistance,
                maxDistance
            );
        }

        // マウス操作
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                yaw += mouseDelta.x * sensitivity;
                pitch -= mouseDelta.y * sensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            // ESCでカーソル解除
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // 左クリックで再ロック
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // 注視する位置
        Vector3 focusPosition =
            target.position + Vector3.up * height;

        // カメラの回転
        Quaternion cameraRotation =
            Quaternion.Euler(pitch, yaw, 0f);

        // カメラの目標位置
        Vector3 desiredPosition =
            focusPosition -
            cameraRotation * Vector3.forward * distance;

        // 滑らかに追従
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Time.deltaTime * smoothSpeed
        );

        transform.rotation = Quaternion.LookRotation(
            focusPosition - transform.position
        );
    }
}
