using UnityEngine;

[CreateAssetMenu(menuName = "BossSkills/SummonSpike")]
public class SummonSpikeSkill : BossSkill
{
    public GameObject warningPrefab; // transparent red sphere prefab
    public GameObject spikePrefab;   // spike prefab with AOE damage
    public float warningDuration = 1f;
    public float spikeOffset = 3f;   // distance ahead of player
    public float knockbackStrength = 3f;

    private Vector3 castPos;

    public override void Cast(GameObject boss, Transform player)
    {
        // Predict position ahead of player
        castPos = player.position + player.forward * spikeOffset;

        // Step 1: Spawn warning sign
        GameObject warning = Instantiate(warningPrefab, castPos, Quaternion.identity);
        Destroy(warning, warningDuration);

        // Step 2: Delay spike summon
        boss.GetComponent<MonoBehaviour>().StartCoroutine(SpawnSpikeAfterDelay(castPos));
    }

    private System.Collections.IEnumerator SpawnSpikeAfterDelay(Vector3 pos)
    {
        yield return new WaitForSeconds(warningDuration);

        // Spawn spike at predicted position
        GameObject spike = Instantiate(spikePrefab, pos, Quaternion.identity);

        // Destroy spike after 2 seconds (adjust as needed)
        Destroy(spike, 1f);
    }
}

