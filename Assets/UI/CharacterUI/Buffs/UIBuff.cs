using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuff : MonoBehaviour
{
    BuffManager playerBuff;
    private GameObject buffFileObj;
    private Transform baseOverlay;
    void Start()
    {
        playerBuff = GameObject.Find("Player").GetComponent<BuffManager>();
        buffFileObj = Resources.Load<GameObject>("UI/Buff/BuffItem");
        baseOverlay = transform.Find("Overlay");
    }

    void Update()
    {

        // xoá overlay cũ
        foreach (Transform child in baseOverlay)
        {
            GameObject.Destroy(child.gameObject);
        }

        Dictionary<BuffType, bool> activeBuffs = playerBuff.GetActiveBuff();
        int index = 0;

        void bind(Sprite sprite, Image icon)
        {
            if (sprite != null)
            {
                icon.sprite = sprite;

            }
            else
                Debug.Log("No sprite found");
        }

        foreach (KeyValuePair<BuffType, bool> buff in activeBuffs)
        {
            index++;
            GameObject obj = Instantiate(buffFileObj);
            obj.transform.SetParent(baseOverlay);
            RectTransform rect = obj.transform.GetComponent<RectTransform>();
            // Fix lỗi UI khởi tạo không đúng kích cỡ
            rect.localScale = new Vector2(1,1);
            rect.localPosition = new Vector3(index * 75, 0, 0);
            Image icon = obj.transform.Find("Icon").GetComponent<Image>();
            switch (buff.Key)
            {
                case BuffType.DoubleJump:
                    bind(Resources.Load<Sprite>("UI/Buff/DoubleJump"), icon);
                    break;
                case BuffType.FireElement:
                    bind(Resources.Load<Sprite>("UI/Buff/FireElement"), icon);                    
                    break;
                case BuffType.DashCooldownReduce:
                    bind(Resources.Load<Sprite>("UI/Buff/DashCooldown"), icon);
                    break;
                default:
                    break;
            }
        }
    }
}
