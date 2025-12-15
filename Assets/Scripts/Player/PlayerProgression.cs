using System;
using UnityEngine;

public class PlayerProgression
{
    private float TotalXP { get; set; } = 0f;

    // Events
    // Invoked once per level crossed with the new level number.
    public event Action<int> LevelUp;
    // Invoked whenever XP is added (amount > 0).
    public event Action<float> XPAdded;

    // XP formula parameters
    private const float BaseXP = 10f;   // base xp needed for the first level-up (level 1 -> 2)
    private const float Growth = 5f;    // growth factor that is doubled each level (multiplied by 2^(level-1))

    public void AddXP(float amount)
    {
        if (amount <= 0f)
            return;

        // Fire XP added event
        XPAdded?.Invoke(amount);

        float before = TotalXP;
        TotalXP += amount;
        Debug.Log("Added XP: " + amount + ", Total XP: " + TotalXP);

        // Detect level ups and fire LevelUp for each crossed level (pass the new level number)
        int oldLevel = CalculateLevelFromXP(before);
        int newLevel = CalculateLevelFromXP(TotalXP);
        for (int lvl = oldLevel + 1; lvl <= newLevel; lvl++)
            LevelUp?.Invoke(lvl);
    }

    public void SetXP(float amount)
    {
        if (amount < 0f)
            amount = 0f;

        if (amount > TotalXP)
        {
            // If increasing, reuse AddXP logic so events are consistent
            AddXP(amount - TotalXP);
        }
        else
        {
            // If setting to same or lower XP, just set silently
            TotalXP = amount;
        }
    }

    // Adds the given number of whole levels to the player. Uses SetLevel to ensure events fire.
    public void AddLevel(int levels)
    {
        if (levels <= 0)
            return;
        int target = GetLevel() + levels;
        SetLevel(target);
    }

    // Sets the player's level to `level`. If increasing, LevelUp/X P events will be fired via AddXP.
    public void SetLevel(int level)
    {
        if (level < 1)
            level = 1;

        float targetXP = CumulativeXPForLevel(level);
        if (targetXP > TotalXP)
        {
            AddXP(targetXP - TotalXP);
        }
        else
        {
            TotalXP = targetXP;
        }
    }

    public float GetTotalXP()
    {
        return TotalXP;
    }

    // Returns the current integer level (starting at 1).
    public int GetLevel()
    {
        Debug.Log("Total XP: " + TotalXP);
        return CalculateLevelFromXP(TotalXP);
    }

    // Returns progress toward next level as 0..1
    public float GetLevelProgress()
    {
        int level = CalculateLevelFromXP(TotalXP);
        float remainder = GetRemainderForLevel(TotalXP, level);
        float needed = XPToNext(level);
        if (needed <= 0f) return 0f;
        return Mathf.Clamp01(remainder / needed);
    }

    // XP required to go from `level` to `level + 1`
    private float XPToNext(int level)
    {
        // level is 1-based. growth doubles each level: Growth * 2^(level-1)
        return BaseXP + Growth * Mathf.Pow(2f, level - 1);
    }

    // Calculate level from total XP by subtracting level requirements until remainder < next requirement
    private int CalculateLevelFromXP(float totalXP)
    {
        if (totalXP < 0f) return 1;

        float remaining = totalXP;
        int level = 1;
        while (true)
        {
            float need = XPToNext(level);
            if (remaining < need)
                break;
            remaining -= need;
            level++;
        }
        return level;
    }

    // Returns remainder XP into the current level (XP into current level, not cumulative)
    private float GetRemainderForLevel(float totalXP, int level)
    {
        if (totalXP <= 0f) return 0f;

        float remaining = totalXP;
        int l = 1;
        while (l < level)
        {
            remaining -= XPToNext(l);
            l++;
        }

        // remaining is the XP into `level`
        return Mathf.Max(0f, remaining);
    }

    // Total cumulative XP required to reach the start of `level` (level 1 -> 0 XP)
    private float CumulativeXPForLevel(int level)
    {
        if (level <= 1) return 0f;
        float sum = 0f;
        for (int i = 1; i < level; i++)
            sum += XPToNext(i);
        return sum;
    }
}
