
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillCategory : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject SkillItemPrefab;
    private GameObject listing;
    private string Name;
    private void Awake()
    {
        //listing = transform.Find("Listing").gameObject;
    }
    void Start()
    {
        
    }

    public void Setup(List<Skills> skills, string categoryName)
    {
        TMP_Text text = transform.Find("CategoryName").GetComponent<TMP_Text>();
        text.SetText(categoryName);
        Name = categoryName;

        listing = transform.Find("Listing").gameObject;

        foreach (Skills skill in skills) {
            GameObject skillItem = Instantiate(SkillItemPrefab, listing.transform);
            SkillItem item = skillItem.GetComponent<SkillItem>();
            item.Setup(skill);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
