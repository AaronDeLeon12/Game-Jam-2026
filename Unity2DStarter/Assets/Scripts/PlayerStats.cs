using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float spellManaCost = 20f;
    [SerializeField] private float manaRegenDelay = 3f;
    [SerializeField] private float manaRegenPerSecond = 20f;
    [SerializeField] private float healthValueInMana = 2f;

    private float health;
    private float mana;
    private float manaRegenBlockedUntil;
    private bool isDead;

    public float Health => health;
    public float Mana => mana;
    public float MaxHealth => maxHealth;
    public float MaxMana => maxMana;
    public float SpellManaCost => spellManaCost;
    public float SpellHealthCost => ConvertManaDebtToHealth(spellManaCost);
    public float ManaRegenDelayRemaining => Mathf.Max(0f, manaRegenBlockedUntil - Time.time);
    public bool IsDead => isDead;

    private void Awake()
    {
        health = maxHealth;
        mana = maxMana;
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (Time.time >= manaRegenBlockedUntil && mana < maxMana)
        {
            mana = Mathf.Min(maxMana, mana + manaRegenPerSecond * Time.deltaTime);
        }
    }

    public bool TryPaySpellCost()
    {
        return TryPayCost(spellManaCost, manaRegenDelay);
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

    public float ConvertManaDebtToHealth(float manaDebt)
    {
        return Mathf.Ceil(Mathf.Max(0f, manaDebt) / healthValueInMana);
    }
}
