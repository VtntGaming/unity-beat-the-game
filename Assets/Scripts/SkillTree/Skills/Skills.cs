using UnityEngine;

public class Skills
{
    public string Name;
    public string Category = "Default";
    public string Description = "";
    public bool Locked = true;
    public Skills requirementSkill;
    public float buffAmount = 0f;
    public Skills() {

    }

    private bool checkRequirement(Skills skill)
    {
        if (skill != null) {
            if (skill.Locked)
                return true;
            else
                return checkRequirement(skill.requirementSkill);
        }
        return false;
    }

    public bool IsLocked()
    {
        if (Locked) return true;
        return checkRequirement(requirementSkill);
    }

    public bool CanUnlock()
    {
        return !checkRequirement(requirementSkill);
    }
}
