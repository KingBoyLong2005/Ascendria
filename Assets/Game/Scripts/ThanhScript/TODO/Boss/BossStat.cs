using UnityEngine;

public class BossStats : EnemyStats
{
    public BossSkill uniqueSkill;
    
    private float skillAtkMultiplier;
    private float nextSkillTime = 0f;
    private Transform player;

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        skillAtkMultiplier = uniqueSkill.skillAtkMultiplier;
    }

    private void Update()
    {
        //base.Update();

        if (uniqueSkill != null && Time.time >= nextSkillTime)
        {
            uniqueSkill.Cast(this.gameObject, player); // pass boss + player
            nextSkillTime = Time.time + uniqueSkill.cooldown;
        }
    }

    public void HandleSkillDamage()
    {
        Debug.Log($"Handle Skill Damage: multi: {skillAtkMultiplier}, Atk: {Attack}");
        DamageManager.Instance.CalculateBossSkillDamage(skillAtkMultiplier, Attack);
    }
}
