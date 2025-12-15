using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.Experimental;
using UnityEditor;
using UnityEngine.Rendering;

class DamageTxt
{
    public GameObject obj;
    public float t = 0;
    Vector2 moveVector;
    Vector2 basePosition;
    float moveDistance = 100f;

    public DamageTxt(GameObject obj, Vector2 moveVector, Vector2 basePosition)
    {
        this.obj = obj;
        this.moveVector = moveVector;
        this.basePosition = basePosition;
    }

    float sineOut(float t)
    {
        t = Mathf.Clamp01(t);
        return Mathf.Sin(t * Mathf.PI * 0.5f);
    }
    public void move(float deltaTime)
    {
        t += deltaTime;
        float t2 = sineOut(t);
        float t3 = Mathf.Clamp((1f - t) / 0.25f, 0, 1);
        obj.GetComponent<RectTransform>().localPosition = basePosition + moveVector * t2 * moveDistance;
        obj.GetComponent<CanvasGroup>().alpha = t3;
    }
}
public class DamageDisplay : MonoBehaviour
{
    GameObject DamageTxtOverlay;
    GameObject cam;

    List<DamageTxt> list = new List<DamageTxt>();
    void Start()
    {
        DamageTxtOverlay = Resources.Load<GameObject>("UI/DamageDisplay/DamageTxt");
        cam = GameObject.Find("Main Camera");
    }

    Vector2 gameScreen = new Vector2(1280, 720);
    public void DisplayDamage(float Damage, Vector3 pos, bool? isHeal)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform, screenPos, cam.GetComponent<Camera>(), out Vector2 localPos
        );
        GameObject newOverlay = Instantiate(DamageTxtOverlay);
        newOverlay.transform.SetParent(transform);
        TMP_Text text = newOverlay.transform.GetComponent<TMP_Text>();
        RectTransform rect = newOverlay.GetComponent<RectTransform>();
        if (isHeal == true) {
            text.color = new Color32(142, 255, 82, 255);
        }
        text.text = string.Format("{0:F0}", Damage);

        rect.localPosition = localPos;
        rect.localScale = new Vector2(1, 1);



        Vector2 moveVector = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        DamageTxt newTxt = new DamageTxt(newOverlay, moveVector, localPos);
        list.Add(newTxt);
    }
    void Update()
    {
        List<DamageTxt> toRemove = new List<DamageTxt>();

        foreach (DamageTxt t in list)
        {
            t.move(Time.deltaTime);
            if (t.t > 1)
            {
                Destroy(t.obj);
                toRemove.Add(t);
            }
        }

        foreach (DamageTxt t in toRemove)
        {
            list.Remove(t);
        }
    }
}
