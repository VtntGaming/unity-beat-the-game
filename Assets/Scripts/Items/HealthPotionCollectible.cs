using UnityEngine;

public class HealthPotionCollectible : MonoBehaviour
{
    [SerializeField] private float healthValue;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<Entity>().AddHealth(healthValue);
            AudioManager.Sfx(Sound.PickUpHealthPotion);
            Destroy(gameObject);
        }
    }
}
