using UnityEngine;

public class FloatingMotion : MonoBehaviour
{
    public float amplitude = 0.25f;
    public float frequency = 1f;

    private Vector3 startPos;
    private bool isActive = false;

    void Start()
    {
        startPos = transform.position;
    }

    public void ActivateFloating()
    {
        isActive = true;
        startPos = transform.position; // cập nhật vị trí gốc tại thời điểm bắt đầu lơ lửng
    }

    void Update()
    {
        if (!isActive) return;

        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * frequency) * amplitude;
    }
}
