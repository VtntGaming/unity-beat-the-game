using UnityEngine;

public class GameComponentLoad : MonoBehaviour
{
    void Start()
    {
        // Nếu có UI cũ, xóa nó
        GameObject OldUI = GameObject.Find("GameUI");
        if (OldUI != null)
            Destroy(OldUI);

        // Load UI mới
        GameObject MainUI = Instantiate(Resources.Load<GameObject>("UI/GameUI"));
        MainUI.SetActive(true);
        MainUI.transform.SetParent(null);
        MainUI.name = "GameUI";

        // Gán camera cho canvas
        Camera mainCamera = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
        if (mainCamera != null && MainUI.TryGetComponent(out Canvas canvas))
        {
            canvas.worldCamera = mainCamera;
        }
    }

    void Update()
    {
        // Không cần gì ở đây hiện tại
    }
}