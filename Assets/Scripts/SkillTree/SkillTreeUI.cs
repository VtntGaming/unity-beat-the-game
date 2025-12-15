using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private SkillListing skillListing;
    [SerializeField] private GameObject skillCategoryPrefab;
    private GameObject overlay;
    private GameObject content;

    private Button TriggerButton;
    void Awake()
    {
        skillListing = GameObject.FindFirstObjectByType<PlayerStats>().skillListing;
        overlay = transform.Find("Overlay").gameObject;
    }
    void Start()
    {
        TriggerButton = transform.Find("Skills_Button").Find("Button").GetComponent<Button>();

        content = overlay.transform.Find("Scroll View").Find("Viewport").Find("Content").gameObject;

        foreach (KeyValuePair<string, List<Skills>> data in skillListing.list)
        {
            string categoryName = data.Key;
            List<Skills> listing = data.Value;
            GameObject newCategory = Instantiate(skillCategoryPrefab, content.transform);
            newCategory.transform.localScale = Vector3.one;
            SkillCategory skillCategory = newCategory.GetComponent<SkillCategory>();
            skillCategory.Setup(listing, categoryName);
        }
        TriggerButton.onClick.AddListener(ButtonTrigger);
    }

    void ButtonTrigger()
    {
        overlay.SetActive(!overlay.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ButtonTrigger();
        }
    }
}
