using UnityEngine;

public class BasicBlocking : MonoBehaviour
{
    [Header("Block Settings")]
    [SerializeField] private float blockCooldown = 5f; // Cooldown for the special block
    [SerializeField] private float halfDamageMultiplier = 0.5f; // Damage multiplier for idle block (normal block)
    [SerializeField] private KeyCode blockKey = KeyCode.H; // Key to block
    [SerializeField] private string blockTrigger = "Block"; // Animation trigger for block
    [SerializeField] private string idleBlockBool = "IdleBlock"; // Bool for idle block animation

    private Animator playerAnimator;
    private Entity playerHealth;
    private BasicMovement basicMovement;

    private float blockCooldownTimer = Mathf.Infinity; // Timer for block cooldown
    private bool isBlocking = false; // Is the player blocking?
    private bool canPerformPerfectBlock = false; // Can the player perform a perfect block

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        playerHealth = GetComponent<Entity>();
        basicMovement = GetComponent<BasicMovement>();
    }

    void Update()
    {
        if (playerHealth.dead) return;

        blockCooldownTimer += Time.deltaTime;

        // Handle block input
        if (Input.GetKeyDown(blockKey) && basicMovement.canBlock())
        {
            StartBlocking();
            AudioManager.Sfx(Sound.Block);
        }

        // Handle while holding the block key
        if (Input.GetKey(blockKey))
        {
            ContinueBlocking();
        }
        else
        {
            StopBlocking();
        }
    }

    private void StartBlocking()
    {
        if (isBlocking) return; // Avoid re-blocking if already blocking

        isBlocking = true;
        canPerformPerfectBlock = blockCooldownTimer >= blockCooldown; // Can perform perfect block only when cooldown is finished
        blockCooldownTimer = 0f; // Reset cooldown for the next block
        playerAnimator.SetBool(idleBlockBool, true); // Start idle block animation
        playerAnimator.SetTrigger(blockTrigger); // Trigger block animation
    }

    private void ContinueBlocking()
    {
        if (!isBlocking) return;

        if (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Block"))
        {
            playerAnimator.SetBool(idleBlockBool, true);  // Keep idle block animation active
        }
    }

    private void StopBlocking()
    {
        if (!isBlocking) return;

        playerAnimator.SetBool(idleBlockBool, false);  // Stop idle block animation
        isBlocking = false;
    }

    // Called when the player takes damage
    public float ProcessIncomingDamage(float damage, float attackerDirection, GameObject attacker, bool isProjectile)
    {
        if (isBlocking)
        {
            if (canPerformPerfectBlock)
            {
                playerAnimator.SetTrigger(blockTrigger);
                blockCooldownTimer = 0f;
                canPerformPerfectBlock = false;

                if (!isProjectile) KnockbackAttacker(attacker, attackerDirection);
                else HandleProjectile(attacker); // NEW LINE

                return 0f; // No damage
            }
            else
            {
                if (isProjectile) HandleProjectile(attacker); // NEW LINE
                return damage * halfDamageMultiplier; // Normal block
            }
        }

        return damage; // Took full hit
    }


    private void KnockbackAttacker(GameObject attacker, float attackerDirection)
    {
        if (attacker == null) return;

        // Skip knockback if attacker is a projectile
        if (attacker.GetComponent<EnemyProjectile>() != null) return;

        KnockbackController kb = attacker.GetComponent<KnockbackController>();
        if (kb != null)
        {
            kb.ApplyKnockback(-attackerDirection, false);
        }
    }



    private void HandleProjectile(GameObject attacker)
    {
        EnemyProjectile proj = attacker.GetComponent<EnemyProjectile>();
        if (proj != null)
        {
            proj.Explode();
        }
        else
        {
            Destroy(attacker); // fallback if no component found
        }
    }


    // Method to check if the player is blocking
    public bool IsBlocking()
    {
        return isBlocking && Input.GetKey(blockKey);
    }
    public bool BlockingMovementLock => isBlocking;

    // Property to check if the player is performing a perfect block
    public bool IsPerfectBlock()
    {
        return isBlocking && canPerformPerfectBlock;
    }

    // Property to check if the player is performing an idle block (normal block during cooldown)
    public bool IsIdleBlock()
    {
        return isBlocking && !canPerformPerfectBlock;
    }
}
