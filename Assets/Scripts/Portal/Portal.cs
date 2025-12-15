using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Portal : MonoBehaviour
{
    float t = 0;    // Tiến độ quá trình hiển thị [0-1]
    readonly float animateTime = 0.25f; // Thời gian hoạt ảnh
    readonly float distanceChecking = 2f; // Khoảng cách từ portal đến người chơi
    Transform display;
    bool onTeleport = false;
    public int targetDestination = 0;
    void Start()
    {
        display = transform.Find("Interactable");
    }

    // Update is called once per frame
    void Update()
    {
        GameObject player = GameObject.Find("Player");

        Vector2 distance = player.transform.position - transform.position;

        if (distance.magnitude <= distanceChecking && !onTeleport)
        {
            t += Time.deltaTime / animateTime;
            if (Input.GetKeyDown(KeyCode.F))
                Teleport();

        }
        else
            t -= Time.deltaTime / animateTime;

        t = Mathf.Clamp(t, 0, 1);
        float t2 = Mathf.Lerp(0.75f, 1, t);
        display.GetComponent<CanvasGroup>().alpha = t;
        display.Find("Base").transform.localScale = new Vector2(t2, t2);
    }

    void Teleport()
    {
        onTeleport = true;
        GameObject menuTransition = Instantiate(Resources.Load<GameObject>("UI/SceneTransition"));

        menuTransition.GetComponent<MenuTransition>().switchScene(targetDestination);
    }
}
