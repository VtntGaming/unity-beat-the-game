using UnityEngine;
// using UnityEngine.UI;  // Không còn dùng Button

public class TriggerTutorial : MonoBehaviour
{
    [Header("Tutorial Panel to Show")]
    public GameObject tutorialPanel; // ví dụ MovePanel, JumpPanel...

    [Header("Player Reference")]
    // public BasicMovement playerMovement; // tạm comment, không khóa player

    private bool shown = false;

    void Start()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        // Tìm nút OK trong panel (không dùng nữa)
        // Button ok = tutorialPanel.GetComponentInChildren<Button>();
        // if (ok != null)
        //     ok.onClick.AddListener(HideTutorial);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (shown) return;

        if (collision.CompareTag("Player"))
            ShowTutorial();
    }

    void ShowTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            shown = true;

            // ❌ Freeze player tạm comment
            // if (playerMovement != null)
            //     playerMovement.canMove = false;
        }
    }

    void HideTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);

            // ✅ Unfreeze player tạm comment
            // if (playerMovement != null)
            //     playerMovement.canMove = true;
        }
    }
}
