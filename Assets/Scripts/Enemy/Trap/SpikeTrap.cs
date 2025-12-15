using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    private Entity playerHealth;
    [SerializeField] private int damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            playerHealth = other.GetComponent<Entity>();
            DamageObject();
        }
    }
    public void DamageObject()
    {
        if (playerHealth != null)
        {
            float direction = Mathf.Sign(transform.localScale.x);
            playerHealth.TakeDamage(damage, direction, gameObject, false); // false = not a projectile
        }
        else
        {
            Debug.LogError("PlayerHealth is null! Cannot damage player.");
        }
    }
}
