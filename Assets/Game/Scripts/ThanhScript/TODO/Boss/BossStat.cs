using UnityEngine;

public class BossStats : EnemyStats
{
    public BossSkill uniqueSkill;
    
    private float skillAtkMulti;
    private float nextSkillTime = 0f;
    private Transform player;

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        if (uniqueSkill != null) 
        { 
            uniqueSkill.OnSkillHitPlayer += HandleSkillDamage; 
        }
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

    public void HandleSkillDamage(float multi)
    {
        //Debug.Log($"Handle Skill Damage: multi: {multi}, Atk: {Attack}");
        DamageManager.Instance.CalculateBossSkillDamage(multi, Attack);
    }
}
