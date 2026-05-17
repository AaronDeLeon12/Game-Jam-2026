using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float spellManaCost = 20f;
    [SerializeField] private float manaRegenDelay = 3f;
    [SerializeField] private float manaRegenPerSecond = 20f;
    [SerializeField] private float healthValueInMana = 2f;
    [SerializeField] private float parryWindowDuration = 0.2f;
    [SerializeField] private float parryCooldown = 1f;
    [SerializeField] private float parryHealthRestore = 10f;

    private float health;
    private float mana;
    private float manaRegenBlockedUntil;
    private float parryWindowEndTime;
    private float nextParryReadyTime;
    private bool isDead;
    private bool isBlocking;
    private bool wasBlocking;
    private PlayerActionCounter actionCounter;

    public float Health => health;
    public float Mana => mana;
    public float MaxHealth => maxHealth;
    public float MaxMana => maxMana;
    public float SpellManaCost => spellManaCost;
    public float SpellHealthCost => ConvertManaDebtToHealth(spellManaCost);
    public float ManaRegenDelayRemaining => Mathf.Max(0f, manaRegenBlockedUntil - Time.time);
    public bool IsDead => isDead;
    public bool IsBlocking => isBlocking && !isDead;

    private void Awake()
    {
        health = maxHealth;
        mana = maxMana;
        actionCounter = GetComponent<PlayerActionCounter>();
    }

    private void Update()
    {
        if (PauseMenu.IsPaused)
        {
            return;
        }

        if (isDead)
        {
            isBlocking = false;
            return;
        }

        isBlocking = Input.GetMouseButton(1);
        if (isBlocking && !wasBlocking && Time.time >= nextParryReadyTime)
        {
            parryWindowEndTime = Time.time + parryWindowDuration;
            nextParryReadyTime = Time.time + parryCooldown;
            RecordAction("block_parry_attempt");
        }

        wasBlocking = isBlocking;

        if (Time.time >= manaRegenBlockedUntil && mana < maxMana)
        {
            mana = Mathf.Min(maxMana, mana + manaRegenPerSecond * Time.deltaTime);
        }
    }

    public bool TryPaySpellCost()
    {
        return TryPayCost(spellManaCost, manaRegenDelay);
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        float finalDamage = IsBlocking ? damage * 0.5f : damage;
        health = Mathf.Max(0f, health - Mathf.Max(0f, finalDamage));
        HitFlash2D.Play(gameObject, IsBlocking ? new Color(0.4f, 0.75f, 1f) : new Color(1f, 0.2f, 0.15f));
        GameAudio.PlaySfx("playerDamagedSF2", transform.position, 0.8f);

        if (IsBlocking && Time.time <= parryWindowEndTime)
        {
            RestoreHealth(parryHealthRestore);
            parryWindowEndTime = 0f;
        }

        if (health <= 0f)
        {
            isDead = true;
        }
    }

    public bool TryPayCost(float manaCost, float regenDelay)
    {
        if (isDead)
        {
            return false;
        }

        float manaPaid = Mathf.Min(mana, manaCost);
        float remainingManaCost = manaCost - manaPaid;
        float healthCost = ConvertManaDebtToHealth(remainingManaCost);

        mana -= manaPaid;

        if (healthCost > 0f)
        {
            health = Mathf.Max(0f, health - healthCost);
        }

        manaRegenBlockedUntil = Mathf.Max(manaRegenBlockedUntil, Time.time + regenDelay);

        if (health <= 0f)
        {
            isDead = true;
        }

        return true;
    }

    public void RestoreResourceValue(float manaValue)
    {
        if (isDead)
        {
            return;
        }

        float manaRestored = Mathf.Min(maxMana - mana, Mathf.Max(0f, manaValue));
        mana += manaRestored;

        float leftoverValue = manaValue - manaRestored;
        if (leftoverValue > 0f)
        {
            RestoreHealth(ConvertManaDebtToHealth(leftoverValue));
        }
    }

    public void RestoreHealth(float amount)
    {
        if (isDead)
        {
            return;
        }

        health = Mathf.Min(maxHealth, health + Mathf.Max(0f, amount));
    }

    public float ConvertManaDebtToHealth(float manaDebt)
    {
        return Mathf.Ceil(Mathf.Max(0f, manaDebt) / healthValueInMana);
    }

    private void RecordAction(string actionName)
    {
        if (actionCounter == null)
        {
            actionCounter = GetComponent<PlayerActionCounter>();
        }

        if (actionCounter != null)
        {
            actionCounter.Record(actionName);
        }
    }
}
