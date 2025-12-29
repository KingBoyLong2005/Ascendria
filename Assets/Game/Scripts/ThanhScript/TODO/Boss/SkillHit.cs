using System;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class SkillHit : MonoBehaviour
{
    private CharacterController controller;

    public float knockbackStrength = 5f;
    private Vector3 knockbackVelocity; 
    public float knockbackDecay = 5f;

    public GameObject boss;

    private void Update()
    {
        if (knockbackVelocity.magnitude > 0.1f)
        { 
            // Apply knockback movement
            controller.Move(knockbackVelocity * Time.deltaTime); 
            // Gradually reduce knockback
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime); 
        } 
    }

    public void ApplyKnockback(Vector3 direction, float strength)
    {
        knockbackVelocity = direction.normalized * strength;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) //only knock back the player
        {
            //Debug.Log("Skill Hit Player");
            var a = boss.GetComponent<BossStats>();
            a.HandleSkillDamage();

            controller = other.GetComponent<CharacterController>();
            if (controller != null)
            {
                //Debug.Log("Apply knockback");
                Vector3 dir = (other.transform.position - transform.position).normalized;
                ApplyKnockback(dir, knockbackStrength);
            }
        }
    }
}
