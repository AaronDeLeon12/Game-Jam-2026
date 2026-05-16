using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float spellManaCost = 20f;
    [SerializeField] private float manaRegenDelay = 5f;
    [SerializeField] private float manaRegenPerSecond = 25f;

    private float health;
    private float mana;
    private float lastCastTime = -999f;
    private bool isDead;

    public float Health => health;
    public float Mana => mana;
    public float MaxHealth => maxHealth;
    public float MaxMana => maxMana;
    public float SpellManaCost => spellManaCost;
    public float SpellHealthCost => spellManaCost * 0.5f;
    public float ManaRegenDelayRemaining => Mathf.Max(0f, manaRegenDelay - (Time.time - lastCastTime));
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

        if (Time.time - lastCastTime >= manaRegenDelay && mana < maxMana)
        {
            mana = Mathf.Min(maxMana, mana + manaRegenPerSecond * Time.deltaTime);
        }
    }

    public bool TryPaySpellCost()
    {
        return TryPayAbilityCost(spellManaCost, SpellHealthCost);
    }

    public bool TryPayAbilityCost(float manaCost, float healthCost)
    {
        if (isDead)
        {
            return false;
        }

        if (mana >= manaCost)
        {
            mana -= manaCost;
            lastCastTime = Time.time;
            return true;
        }

        health = Mathf.Max(0f, health - healthCost);
        lastCastTime = Time.time;

        if (health <= 0f)
        {
            isDead = true;
        }

        return true;
    }

    public bool TryPayCombinedCost(float manaCost, float healthCost)
    {
        if (isDead || mana < manaCost)
        {
            return false;
        }

        mana -= manaCost;
        health = Mathf.Max(0f, health - healthCost);
        lastCastTime = Time.time;

        if (health <= 0f)
        {
            isDead = true;
        }

        return true;
    }
}
