using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour
{
    [Header("Health & Mana")]
    [SerializeField] public float startingHealth = 100f;
    [SerializeField] public float startingMana = 50f;
    public float currentHealth { get; private set; }
    public float currentMana { get; private set; }
    public float maxHealth { get; private set; }
    public float maxMana { get; private set; }
    private Animator anim;
    public bool dead;

    [Header("Defense & Healing")]
    // [SerializeField] public float armour = 20f; // ❌ XÓA HOẶC COMMENT DÒNG NÀY
    [SerializeField] public float healingPerSec = 10f;
    [SerializeField] public float manaRegenPerSec = 3f;
    private float healCooldown = 0;
    private BasicBlocking blockSystem;
    [SerializeField] private bool knockbackEvenWhenBlocked = true; // dùng nếu block vẫn cho knockback hay không

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration = 0.8f;
    [SerializeField] private int numberOfFlashes = 4;
    private SpriteRenderer spriteRend;
    private bool invulnerable;

    [Header("Components")]
    [SerializeField] private Behaviour[] components;

    private PlayerStats playerStats;

    // Cache để tránh gọi Find nhiều lần
    private DamageDisplay damageDisplayCache;
    private int layerA = 10; // default layer indices used in IgnoreLayerCollision
    private int layerB = 11;

    private void Awake()
    {
        maxHealth = startingHealth;
        maxMana = startingMana;
        currentHealth = startingHealth;
        currentMana = startingMana;

        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
        blockSystem = GetComponent<BasicBlocking>();
        playerStats = GetComponent<PlayerStats>();

        // Cache DamageDisplay nếu có (tùy cấu trúc UI của bạn)
        var ui = GameObject.Find("GameUI");
        if (ui != null)
        {
            var dd = ui.transform.Find("DamageDisplay");
            if (dd != null)
                damageDisplayCache = dd.GetComponent<DamageDisplay>();
        }

        // Nếu bạn dùng Layer names thay vì indices, đổi ở đây:
        // layerA = LayerMask.NameToLayer("Player");
        // layerB = LayerMask.NameToLayer("Enemy");
    }

    private void Update()
    {
        // Hồi máu nếu không bị đánh trong thời gian hồi healCooldown
        if (currentHealth > 0 && currentHealth < maxHealth && healCooldown <= 0 && !dead)
        {
            currentHealth = Mathf.Clamp(currentHealth + healingPerSec * Time.deltaTime, 0, maxHealth);
        }

        currentMana = Mathf.Clamp(currentMana + manaRegenPerSec * Time.deltaTime, 0, maxMana);
        healCooldown = Mathf.Max(0, healCooldown - Time.deltaTime);

        // Logic tự tử khi rơi xuống void
        if (transform.position.y < -20f && !dead)
        {
            currentHealth -= Time.deltaTime * maxHealth * 2f;
            if (currentHealth < 10f)
            {
                // Gọi TakeDamage với sát thương lớn (canBeBlocked=false để bypass block)
                TakeDamage(1000000f, 1f, null, false, false);
            }
        }
    }

    private bool animatorHasTrigger(Animator animator, string triggerName)
    {
        if (animator == null) return false;
        foreach (var param in animator.parameters)
        {
            if (param.name == triggerName && param.type == AnimatorControllerParameterType.Trigger)
                return true;
        }
        return false;
    }

    private float GetDamageAfterReduced(float damage)
    {
        float Base = 15f;
        // ======== THAY ĐỔI QUAN TRỌNG ========
        // Lấy giáp từ PlayerStats nếu có, nếu không (ví dụ: đây là Enemy) thì dùng 0
        float currentArmour = (playerStats != null) ? playerStats.FinalArmour : 0f;
        // (Nếu Enemy cũng có giáp, bạn cần một logic khác, 
        // nhưng hiện tại file Health.cs của bạn không phân biệt Player/Enemy)

        float reductionFactor = Base / (Base + currentArmour);
        float finalDamage = damage * reductionFactor;
        return finalDamage;
    }

    /// <summary>
    /// TakeDamage:
    /// - canBeBlocked: nếu false thì bypass block system (dùng cho DOT, trap...)
    /// - isProjectile: phân biệt melee vs projectile (để block xử lý khác nhau / knockback khác nhau)
    /// </summary>
    public virtual void TakeDamage(
        float _damage,
        float attackerDirection,
        GameObject attacker = null,
        bool isProjectile = false,
        bool canBeBlocked = true
        )
    {
        bool isDOT = !canBeBlocked;

        if (invulnerable || dead) return;

        // Xử lý Block (chỉ nếu canBeBlocked = true)
        if (blockSystem != null && canBeBlocked)
        {
            // Expect basic blockSystem to handle isProjectile param if needed
            _damage = blockSystem.ProcessIncomingDamage(_damage, attackerDirection, attacker, isProjectile);
        }

        float DamageTaken = GetDamageAfterReduced(_damage);

        // Hiển thị damage (dùng cache nếu có)
        if (DamageTaken > 0f)
        {
            if (damageDisplayCache != null)
                damageDisplayCache.DisplayDamage(DamageTaken, transform.position, false);
            else
            {
                var ui = GameObject.Find("GameUI");
                if (ui != null)
                {
                    var dd = ui.transform.Find("DamageDisplay");
                    if (dd != null)
                    {
                        var ddComp = dd.GetComponent<DamageDisplay>();
                        if (ddComp != null) ddComp.DisplayDamage(DamageTaken, transform.position, false);
                    }
                }
            }
        }

        currentHealth = Mathf.Clamp(currentHealth - DamageTaken, 0f, maxHealth);

        if (currentHealth > 0f)
        {
            if (DamageTaken > 0f)
            {
                if (anim != null && animatorHasTrigger(anim, "Hurt"))
                    anim.SetTrigger("Hurt");

                if (CompareTag("Player"))
                    AudioManager.Sfx(Sound.HurtPlayer);

                // Không kích hoạt iFrames cho DOT
                if (!isDOT)
                    StartCoroutine(Invulnerability());

                // Reset cooldown hồi máu
                healCooldown = 1f;

                // Knockback: chỉ khi không phải DOT và attackerDirection khác 0
                if (!isDOT && attackerDirection != 0f)
                {
                    var kb = GetComponent<KnockbackController>();
                    if (kb != null)
                    {
                        // Nếu KnockbackController hỗ trợ isProjectile, dùng overload; nếu không, fallback
                        var method = kb.GetType().GetMethod("ApplyKnockback", new System.Type[] { typeof(float), typeof(bool) });
                        if (method != null)
                        {
                            // Gọi ApplyKnockback(float dir, bool isProjectile)
                            method.Invoke(kb, new object[] { attackerDirection, isProjectile });
                        }
                        else
                        {
                            // Fallback: gọi ApplyKnockback(float dir)
                            kb.ApplyKnockback(attackerDirection, isProjectile);

                        }
                    }
                }
            }
        }
        else
        {
            if (!dead)
            {
                dead = true;
                Debug.Log($"{gameObject.name} đã hết HP và chết.");

                // 1. Tắt các component điều khiển (để quái đứng yên/không tấn công nữa)
                if (components != null)
                {
                    foreach (Behaviour component in components)
                    {
                        if (component != null) component.enabled = false;
                    }
                }

                // Tắt DOT
                EnemyDamageDot dot = GetComponent<EnemyDamageDot>();
                if (dot != null) dot.enabled = false;

                // 2. Chơi Animation Death
                if (anim != null && animatorHasTrigger(anim, "Death"))
                    anim.SetTrigger("Death");

                AudioManager.Sfx(Sound.Death);

                // --- XỬ LÝ RIÊNG CHO PLAYER ---
                if (CompareTag("Player"))
                {
                    var playerController = GetComponent<PlayerController>();
                    if (playerController != null)
                        playerController.Die();
                }
                // --- XỬ LÝ CHO ENEMY ---
                else
                {
                    // Rớt đồ ngay lập tức (hoặc có thể chuyển vào Animation Event nếu muốn)
                    var dropTable = GetComponent<DropTable>();
                    if (dropTable != null) dropTable.DropLoot();

                    // Xử lý vật lý cho quái bay (Rơi xuống đất)
                    FlyingEnemy fe = GetComponent<FlyingEnemy>();
                    Rigidbody2D rb = GetComponent<Rigidbody2D>();

                    if (fe != null)
                    {
                        fe.enabled = false; // Tắt bay
                        if (rb != null)
                        {
                            rb.bodyType = RigidbodyType2D.Dynamic; // Bật trọng lực để rơi
                            rb.gravityScale = 1f;
                        }
                        BoxCollider2D fallCollider = GetComponent<BoxCollider2D>();
                        if (fallCollider != null) fallCollider.enabled = true;
                    }

                    // ❌ QUAN TRỌNG: Đã xóa dòng Destroy(gameObject, time) ở đây.
                    // Việc Destroy giờ sẽ do Animation Event gọi hàm DestroyEntity() bên dưới.
                }
            }
        }
    }

    public void UseMana(float _amount)
    {
        if (currentMana >= _amount)
            currentMana -= _amount;
    }

    public void AddHealth(float _value)
    {
        float healthHeal = Mathf.Clamp(currentHealth + _value, 0f, maxHealth) - currentHealth;
        if (healthHeal > 0f)
        {
            if (damageDisplayCache != null)
                damageDisplayCache.DisplayDamage(healthHeal, transform.position, true);
            else
            {
                var ui = GameObject.Find("GameUI");
                if (ui != null)
                {
                    var dd = ui.transform.Find("DamageDisplay");
                    if (dd != null)
                    {
                        var ddComp = dd.GetComponent<DamageDisplay>();
                        if (ddComp != null) ddComp.DisplayDamage(healthHeal, transform.position, true);
                    }
                }
            }
        }
        currentHealth = Mathf.Clamp(currentHealth + _value, 0f, maxHealth);
    }

    public void AddMana(float _value)
    {
        currentMana = Mathf.Clamp(currentMana + _value, 0f, maxMana);
    }

    private IEnumerator Invulnerability()
    {
        invulnerable = true;

        // Guard: ensure numberOfFlashes >= 1 and iFramesDuration positive
        int flashes = Mathf.Max(1, numberOfFlashes);
        float duration = Mathf.Max(0.01f, iFramesDuration);
        float halfWait = duration / (flashes * 2f);

        int layerIndexA = layerA;
        int layerIndexB = layerB;

        // try to toggle by name if indices invalid
        if (layerIndexA < 0 || layerIndexB < 0)
        {
            // optionally try to find by name
            layerIndexA = LayerMask.NameToLayer("Player");
            layerIndexB = LayerMask.NameToLayer("Enemy");
        }

        if (layerIndexA >= 0 && layerIndexB >= 0)
            Physics2D.IgnoreLayerCollision(layerIndexA, layerIndexB, true);

        for (int i = 0; i < flashes; i++)
        {
            if (spriteRend != null)
                spriteRend.color = new Color(1f, 0f, 0f, 0.5f);
            yield return new WaitForSeconds(halfWait);

            if (spriteRend != null)
                spriteRend.color = Color.white;
            yield return new WaitForSeconds(halfWait);
        }

        if (layerIndexA >= 0 && layerIndexB >= 0)
            Physics2D.IgnoreLayerCollision(layerIndexA, layerIndexB, false);

        invulnerable = false;
    }

    public void Respawn()
    {
        dead = false;
        if (components != null)
        {
            foreach (Behaviour component in components)
                if (component != null)
                    component.enabled = true;
        }
        AddHealth(startingHealth);

        if (anim != null)
        {
            anim.ResetTrigger("Death");
            anim.Play("Idle");
        }

        StartCoroutine(Invulnerability());
    }

    private void Deactivate()
    {
        // Chỉ destroy nếu không phải là Player
        if (!CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name} thực hiện Despawn sau khi hết animation.");
            Destroy(gameObject);
        }
    }
}