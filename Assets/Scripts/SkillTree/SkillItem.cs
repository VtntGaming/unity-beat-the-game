using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Color lockedColor = new Color(0.5f, 0.5f, 0.5f);
    Color locked_CanUnlockColor = new Color(0.75f, 0.75f, 0.75f);
    Color unlockedColor = new Color(0.5f, 1f, 1f);

    Skills skill;
    TMP_Text txt;
    TMP_Text desc;
    Image img;

    Dictionary<string, string> TypeData = new Dictionary<string, string>{
        {"HPBuff", "HP"},
        {"ATKBuff", "Atk"},
        {"SpeedBuff", "Speed"},
    };


    void Awake()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(OnBtnPress);
    }

    void Start()
    {
        
    }

    void OnBtnPress()
    {
        //Debug.Log("Player choosed the buff: " + skill.Name + " | Category: " + skill.Category);
        if (skill.IsLocked())
        {
            if (skill.CanUnlock())
            {
                skill.Locked = false;
            }
            else
                Debug.Log("The previous skill need to be unlocked first");
        }
        else
            Debug.Log("The skill is already unlocked");
        
    }

    public void Setup(Skills currentSkill)
    {
        txt = transform.Find("SkillName").GetComponent<TMP_Text>();
        desc = transform.Find("Desc").GetComponent<TMP_Text>();
        skill = currentSkill;
        txt.SetText(skill.Name);
        img = GetComponent<Image>();

        desc.SetText(string.Format("{0} +{1}", TypeData[skill.Category], skill.buffAmount));
    }

    // Update is called once per frame
    void Update()
    {
        img.color = (skill.IsLocked()) ? (skill.CanUnlock() ? locked_CanUnlockColor : lockedColor) : unlockedColor;
    }
}
