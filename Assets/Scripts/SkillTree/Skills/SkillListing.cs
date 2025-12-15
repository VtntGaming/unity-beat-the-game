using System.Collections.Generic;
using UnityEngine;

public class SkillListing
{
    public Dictionary<string, List<Skills>> list = new Dictionary<string, List<Skills>>();

    private Skills previousSkill;

    public Skills[] GetUnlockedSkills() {
        List<Skills> unlockedSkills = new List<Skills>();
        foreach (KeyValuePair<string, List<Skills>> category in list)
        {
            foreach (Skills skill in category.Value)
            {
                if (!skill.IsLocked())
                {
                    unlockedSkills.Add(skill);
                }
            }
        }
        return unlockedSkills.ToArray();
    }
    private void add(string name, float buffAmount, string type = "Default", bool setPreviousAsRequirement = true)
    {
        Skills skill;
        switch (type)
        {
            default: skill = new Skills(); break;
        }

        skill.buffAmount = buffAmount;
        skill.Name = name;
        if (setPreviousAsRequirement && previousSkill != null)
        {
            skill.requirementSkill = previousSkill;
        }
        previousSkill = skill;
        skill.Category = type;
        List<Skills> category = list.GetValueOrDefault(type);
        if (category == null)
        {
            category = new List<Skills>();
            list.Add(type, category);
        }

        category.Add(skill);
    }

    public SkillListing() {
        add("HP I", 5,"HPBuff");
        add("HP II", 10, "HPBuff");
        add("HP III", 20, "HPBuff");
        add("HP IV", 40, "HPBuff");
        add("HP V", 75, "HPBuff");
        add("HP VI", 100, "HPBuff");
        add("HP VII", 150, "HPBuff");
        add("HP IIX", 200, "HPBuff");
        add("HP IX", 250, "HPBuff");
        add("HP X", 300, "HPBuff");
        add("Atk I", 2, "ATKBuff", false);
        add("Atk II", 4, "ATKBuff");
        add("Speed I", 1, "SpeedBuff", false);
        add("Speed II", 2, "SpeedBuff");
    }
}
