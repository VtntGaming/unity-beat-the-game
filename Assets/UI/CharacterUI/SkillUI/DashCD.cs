using UnityEngine;
using TMPro;

public class DashCD : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private BasicMovement movement;

    private TMP_Text display;
    private Transform overlayBase;
    private RectTransform overlayFrame;
    private RectTransform valueFrame;

    bool Load()
    {
        GameObject plr = GameObject.Find("Player");
        if (plr == null) return false;

        movement = plr.transform.GetComponent<BasicMovement>();
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
        if (movement == null)
            if (!Load()) return;
        // dash cooldown
        float size = overlayFrame.localScale.y;

        float CDTime = movement.dashCooldown;
        display.text = (CDTime > 0)?string.Format("{0:F1}", CDTime):"Ready!";
        float sizeRatio = (1 - (CDTime / movement.dashCooldownTime));
        valueFrame.localScale = new Vector2(size, size * sizeRatio);
    }
}
