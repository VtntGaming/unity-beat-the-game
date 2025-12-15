using TMPro;
using UnityEngine;

public class RerollDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshPro priceText;
    [SerializeField] private SpriteRenderer iconRenderer;

    public void SetPrice(int price)
    {
        if (priceText != null)
            priceText.text = price.ToString() + "G";
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconRenderer != null)
            iconRenderer.sprite = sprite;
    }
}
