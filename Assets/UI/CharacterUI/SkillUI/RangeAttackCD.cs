using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class RangeAttackCD : MonoBehaviour
{
    private BasicAttack attack;

    private TMP_Text display;
    private Transform overlayBase;
    private RectTransform overlayFrame;
    private RectTransform valueFrame;

    bool Load()
    {
        GameObject plr = GameObject.Find("Player");
        if (plr == null) return false;
        attack = plr.transform.GetComponent<BasicAttack>();

        return true;
    }
    void Start()
    {
        Load();
        display = transform.Find("Display").GetComponent<TMP_Text>();
        overlayBase = transform.Find("Overlay");
        overlayFrame = overlayBase.GetComponent<RectTransform>();
        valueFrame = overlayBase.Find("ValueDisplay").GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (attack == null)
            if (!Load()) return;
        float size = overlayFrame.localScale.y;

        float CDTime = Mathf.Min(attack.rangeAttackCooldown, attack.rangeCooldownTimer);
        CDTime = attack.rangeAttackCooldown - CDTime;


        display.text = (CDTime > 0) ? string.Format("{0:F1}", CDTime) : "Ready!";
        float sizeRatio = 1 - (CDTime / attack.rangeAttackCooldown);
        valueFrame.localScale = new Vector2(size, size * sizeRatio);
    }
}
