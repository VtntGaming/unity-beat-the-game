using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    [SerializeField] private float xpValue = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().AddCoin(coinValue);
            // Thêm xp khi chạm vào coin
            PlayerProgression plrProgression = GameObject.FindFirstObjectByType<PlayerStats>()?.plrProgression;

            if (plrProgression != null)
            {
                float xpAmount = xpValue;
                plrProgression.AddXP(xpAmount);
            }
            AudioManager.Sfx(Sound.PickUpCoin);
            Destroy(gameObject);
        }
    }
}
