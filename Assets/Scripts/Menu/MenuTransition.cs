using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuTransition : MonoBehaviour
{
    private bool onTransision = false;
    private bool sceneLoadCalled = false;
    private bool sceneLoaded = false;
    private float progress = 0;
    private int targetScene = 0;
    [SerializeField] float TransitionTime = 0.5f;
    Image Dim;
    CanvasGroup group;
    void Start()
    {
        group = transform.GetComponent<CanvasGroup>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void switchScene(int? scene)
    {
        if (!onTransision)
        {
            if (scene != null)
                targetScene = (int)scene;
            DontDestroyOnLoad(transform);
            transform.Find("Overlay").gameObject.SetActive(true);
            onTransision = true;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (onTransision)
        {
            progress = 1.25f; // rollback
            sceneLoaded = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (onTransision) {
            progress += Time.deltaTime / TransitionTime;
            if (progress < 1.25)
            {
                if (progress <= 1)
                {
                    group.alpha = progress;
                }
                else
                {
                    group.alpha = 1;
                }
            }
            else
            {
                if (!sceneLoadCalled)
                {
                    sceneLoadCalled = true;
                    SceneManager.LoadScene(targetScene);
                }
                if (sceneLoaded && progress >= 1.5)
                {
                    if (progress < 2.5)
                    {
                        group.alpha = 1 - (progress - 1.5f);
                    }
                    else Destroy(transform.gameObject);
                }
                else
                {
                    group.alpha = 1;
                }
                
            }
        }
    }
}
