using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Button TriggerButton;
    private Button CloseButtonButton;
    private GameObject mainOverlay;
    void Start()
    {
        mainOverlay = transform.parent.Find("Overlay").gameObject;

        TriggerButton = transform.Find("Button").GetComponent<Button>();
        TriggerButton.onClick.AddListener(ButtonTrigger);


        CloseButtonButton = transform.parent.Find("Overlay").Find("CloseButton").GetComponent<Button>();
        CloseButtonButton.onClick.AddListener(ButtonClose);

        mainOverlay.SetActive(false); // Mặc định nó là false
    }

    void ButtonTrigger()
    {
        mainOverlay.SetActive(!mainOverlay.activeSelf);
    }

    void ButtonClose()
    {
        mainOverlay.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) {
            ButtonTrigger();
        }
    }
}
