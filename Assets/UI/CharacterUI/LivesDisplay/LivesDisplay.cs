using UnityEngine;
using TMPro; // Nhớ dòng này để dùng TextMeshPro

public class LivesDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI livesText;

    // Hàm này sẽ được Player gọi để cập nhật số hiển thị
    public void UpdateLives(int currentLives)
    {
        if (livesText != null)
        {
            livesText.text = "x" + currentLives.ToString();
        }
    }
}