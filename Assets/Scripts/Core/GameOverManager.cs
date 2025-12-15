using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    // Hàm gọi khi bấm nút
    public void BackToMenu()
    {
        Time.timeScale = 1f; // Trả lại thời gian bình thường
        SceneManager.LoadScene("MainMenu"); // Nhớ check tên Scene
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}