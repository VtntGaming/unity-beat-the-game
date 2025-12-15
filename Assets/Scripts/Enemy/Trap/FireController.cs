using UnityEngine;
using System.Collections.Generic;

public class FireController : MonoBehaviour
{
    // Cài đặt trên Inspector
    public Animator fireBaseAnimator;
    public GameObject fireExtensionPrefab;
    public Transform firePoint1;

    // ====================================================================
    // CÁC BIẾN CÓ THỂ CHỈNH SỬA TRONG INSPECTOR (Đã bỏ const)
    // ====================================================================

    [Header("Physics Constants & Timing")]
    [Tooltip("Chiều dài X chính xác của Prefab mở rộng.")]
    [SerializeField] private float extensionLength = 0.74f;

    [Tooltip("Thời gian fire_start chạy.")]
    [SerializeField] private float durationStart = 0.5f;

    [Tooltip("Thời gian fire_mid chạy trước khi mở rộng.")]
    [SerializeField] private float durationMidBeforeGrow = 0.5f;

    // **ĐÃ CHUYỂN SANG PUBLIC/SERIALIZED FIELD**
    [Tooltip("Thời gian lửa mở rộng cháy (3.5s).")]
    public float durationExtension = 3.5f;

    // **ĐÃ CHUYỂN SANG PUBLIC/SERIALIZED FIELD**
    [Tooltip("Thời gian nghỉ (Cool-down) (10s).")]
    public float restartDelay = 10f;

    [Tooltip("Dịch chuyển Z để hiển thị phía trước.")]
    [SerializeField] private float cloneZOffset = -0.5f;

    // Tên các hàm Invoke (Giữ nguyên const)
    private const string GROW_METHOD = "AutoGrowFire";
    private const string STOP_METHOD = "AutoStopFire";
    private const string RESTART_METHOD = "StartFireCycle";

    // Logic quản lý
    private List<GameObject> activeExtensions = new List<GameObject>();
    private int currentFireLevel = 0;

    private void Start()
    {
        // Kiểm tra tham chiếu an toàn
        if (fireBaseAnimator == null) Debug.LogError("ERROR: Animator (FireBase) is NOT assigned!");
        if (fireExtensionPrefab == null) Debug.LogError("ERROR: Prefab (Extension) is NOT assigned!");
        if (firePoint1 == null) Debug.LogError("ERROR: FirePoint1 is NOT assigned!");

        StartFireCycle();
    }

    /// <summary>
    /// Bắt đầu chu kỳ hoạt động mới.
    /// </summary>
    private void StartFireCycle()
    {
        Debug.Log("Restarting Fire Cycle...");

        if (fireBaseAnimator != null && fireBaseAnimator.gameObject != null)
        {
            fireBaseAnimator.gameObject.SetActive(true);
        }

        StartFire();
    }

    /// <summary>
    /// Kích hoạt animation cháy và hẹn giờ mở rộng.
    /// </summary>
    public void StartFire()
    {
        if (currentFireLevel > 0) return;
        if (fireBaseAnimator == null) { return; }

        currentFireLevel = 1;

        CancelInvoke(GROW_METHOD);
        CancelInvoke(STOP_METHOD);

        // Sử dụng biến mới (durationStart, durationMidBeforeGrow)
        float growDelay = durationStart + durationMidBeforeGrow;
        Invoke(GROW_METHOD, growDelay);

        Debug.Log($"Fire starting. Will call AutoGrowFire in {growDelay}s.");
    }

    /// <summary>
    /// Tạo đối tượng mở rộng và hẹn giờ tắt.
    /// </summary>
    private void AutoGrowFire()
    {
        if (currentFireLevel != 1) return;

        currentFireLevel = 2;
        Debug.Log("AutoGrowFire triggered. Growing to Level 2 (MAX).");

        // Tạo clone tại firePoint1
        if (firePoint1 != null)
        {
            InstantiateExtension(firePoint1.position, firePoint1.rotation);
        }

        // Sử dụng biến mới durationExtension
        CancelInvoke(STOP_METHOD);
        Invoke(STOP_METHOD, durationExtension);
    }

    /// <summary>
    /// Xử lý việc tạo Prefab, đặt Z-Depth và bù trừ Pivot.
    /// </summary>
    private void InstantiateExtension(Vector3 position, Quaternion rotation)
    {
        if (fireExtensionPrefab == null) { return; }

        GameObject extension = Instantiate(fireExtensionPrefab, position, rotation, transform);
        activeExtensions.Add(extension);

        // THAY ĐỔI VỊ TRÍ Z và BÙ TRỪ PIVOT
        Vector3 newPos = extension.transform.position;
        newPos.z += cloneZOffset; // Sử dụng biến cloneZOffset

        // Bù trừ Pivot
        Vector3 offset = Vector3.right * (extensionLength / 2f); // Sử dụng biến extensionLength
        newPos += offset;

        extension.transform.position = newPos;

        Debug.Log($"Extension created at compensated X={extension.transform.position.x}, Z={newPos.z}.");
    }

    /// <summary>
    /// Dừng toàn bộ hệ thống lửa, hủy clone và ẩn lửa gốc, sau đó hẹn giờ khởi động lại.
    /// </summary>
    private void AutoStopFire()
    {
        if (currentFireLevel == 0) return;

        Debug.Log("Fire cycle completed. Starting cool-down phase.");

        if (fireBaseAnimator != null)
        {
            fireBaseAnimator.SetTrigger("StopFire");
            Invoke("HideBaseFire", 0.5f);
        }

        foreach (GameObject extension in activeExtensions)
        {
            if (extension != null) Destroy(extension, 0.01f);
        }

        activeExtensions.Clear();
        currentFireLevel = 0;

        // Sử dụng biến mới restartDelay
        CancelInvoke(RESTART_METHOD);
        Invoke(RESTART_METHOD, restartDelay);
    }

    private void HideBaseFire()
    {
        if (fireBaseAnimator != null && fireBaseAnimator.gameObject != null)
        {
            fireBaseAnimator.gameObject.SetActive(false);
            Debug.Log($"Base fire hidden. Will restart in {restartDelay}s."); // Sử dụng biến mới
        }
    }
}