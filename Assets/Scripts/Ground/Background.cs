using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    public Transform player; // Tham chiếu đến player
    public float moveSpeed = 1f; // Tốc độ di chuyển của background (0.5 = chậm hơn player, 1 = đồng bộ)
    public float smoothFactor = 5f; // Hệ số làm mượt chuyển động (nếu muốn mượt hơn)
    [SerializeField] private Vector3 offset = new Vector3(0,0,0); // Khoảng cách cố định giữa background và player (nếu cần)
    private Vector3 initialOffset; // Khoảng cách ban đầu giữa background và player
    public float defaultCameraScale = 4f;
    Camera mainCamera;

    void Start()
    {
        // Lưu khoảng cách ban đầu giữa background và player
        if (player != null)
        {
            initialOffset = transform.position - player.position;
        }
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        if (player == null) return;

        // Tính vị trí mục tiêu cho background
        Vector3 targetPosition = player.position + initialOffset + offset * (mainCamera.orthographicSize / defaultCameraScale);
        // Áp dụng tốc độ di chuyển cho cả X và Y
        targetPosition.x *= moveSpeed;
        targetPosition.y *= moveSpeed;
        targetPosition.z = transform.position.z; // Giữ nguyên Z để đảm bảo thứ tự vẽ

        // Di chuyển background liên tục (mượt mà với Lerp)
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothFactor * Time.deltaTime);
        transform.localScale = new Vector3(mainCamera.orthographicSize/ defaultCameraScale, mainCamera.orthographicSize/ defaultCameraScale, 1f); // Đặt kích thước cố định cho background
    }
}