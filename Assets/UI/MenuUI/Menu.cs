using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    Transform returnMenuBtn;
    Transform returnBtn;
    Transform tutorialBtn;
    GameObject gameOverlay;
    GameObject menuButton;
    public GameObject scrollViewTutorial;

    void Start()
    {
        // Tìm các UI elements
        menuButton = transform.Find("MenuButton")?.gameObject;
        gameOverlay = transform.Find("Overlay")?.gameObject;

        Transform options = transform.Find("Overlay")?.Find("Options");
        if (options != null)
        {
            returnMenuBtn = options.Find("ReturnMenu");
            returnBtn = options.Find("Return");
            tutorialBtn = options.Find("Tutorial");
        }

        // Gán listeners
        returnMenuBtn?.GetComponent<Button>().onClick.AddListener(toggleReturnMenu);
        returnBtn?.GetComponent<Button>().onClick.AddListener(toggleReturn);
        tutorialBtn?.GetComponent<Button>().onClick.AddListener(toggleScrollViewTutorial);
        menuButton?.transform.Find("Button")?.GetComponent<Button>().onClick.AddListener(toggleGameMenu);

        // Thiết lập trạng thái ban đầu
        menuButton?.SetActive(true);
        gameOverlay?.SetActive(false);
        scrollViewTutorial?.SetActive(false);
    }

    void toggleReturnMenu()
    {
        GameObject menuTransition = Instantiate(Resources.Load<GameObject>("UI/SceneTransition"));
        menuTransition.GetComponent<MenuTransition>().switchScene(0);
    }

    void toggleReturn()
    {
        gameOverlay?.SetActive(false);
        scrollViewTutorial?.SetActive(false);
        menuButton?.SetActive(true);
    }

    void toggleGameMenu()
    {
        if (gameOverlay != null)
        {
            bool newState = !gameOverlay.activeSelf;
            gameOverlay.SetActive(newState);
            menuButton?.SetActive(!newState);

            if (!newState && scrollViewTutorial != null && scrollViewTutorial.activeSelf)
                scrollViewTutorial.SetActive(false);
        }
    }

    void toggleScrollViewTutorial()
    {
        if (scrollViewTutorial != null)
            scrollViewTutorial.SetActive(!scrollViewTutorial.activeSelf);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            toggleGameMenu();
    }
}