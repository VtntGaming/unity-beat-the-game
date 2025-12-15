using UnityEngine;
using System.Collections.Generic;

public class BackgroundFollow : MonoBehaviour
{
    private Transform player; // Tham chiếu đến player
    public float moveSpeed = 1f; // Tốc độ di chuyển của background (0.5 = chậm hơn player, 1 = đồng bộ)
    public float smoothFactor = 0.5f; // Hệ số làm mượt chuyển động (nếu muốn mượt hơn)
    private Vector3 initialOffset; // Khoảng cách ban đầu giữa background và player
    private Vector3 playerInitialPosition;
    public Vector3 offset = new Vector3(-5f, 1f, 0);
    public float defaultCameraScale = 5f;
    public float positionMovementMultiplier = 0.1f;
    Camera mainCamera;
    Dictionary<Transform, Vector3> childInitialScaling = new Dictionary<Transform, Vector3>();
    void Start()
    {
        // Lưu khoảng cách ban đầu giữa background và player
        if (player != null)
        {
            playerInitialPosition = player.position;
            initialOffset = transform.position - playerInitialPosition;
        }
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        foreach (Transform child in transform)
        {
            childInitialScaling[child] = child.localScale;
        }
    }

    void setScale(float scale)
    {
        foreach (var kvp in childInitialScaling)
        {
            Transform child = kvp.Key;
            Vector3 initialScale = kvp.Value;
            child.localScale = new Vector3(initialScale.x * scale, initialScale.y * scale, initialScale.z);
        }
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
            if (player == null)
                return;
        }

        // Tính vị trí mục tiêu cho background
        float size = mainCamera.orthographicSize;
        Vector3 cameraOffset = player.position - playerInitialPosition;
        Vector3 targetPosition = mainCamera.transform.position + initialOffset + offset - cameraOffset * positionMovementMultiplier;
        // Áp dụng tốc độ di chuyển cho cả X và Y
        targetPosition.x *= moveSpeed;
        targetPosition.y *= moveSpeed;
        targetPosition.z = transform.position.z; // Giữ nguyên Z để đảm bảo thứ tự vẽ

        // Di chuyển background liên tục (mượt mà với Lerp)
        transform.position = targetPosition;
        setScale(size / defaultCameraScale);
        //transform.localScale = new Vector3(size / defaultCameraScale, size / defaultCameraScale, 1f); // Đặt kích thước cố định cho background
    }
}