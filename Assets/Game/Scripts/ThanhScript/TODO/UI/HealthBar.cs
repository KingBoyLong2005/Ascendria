using UnityEngine;
using UnityEngine.UI;

//Health Bar follows player
public class HealthBar : MonoBehaviour
{
    public Slider slider;
    //change to eventhandler subscription
    public void Awake() 
    {
        PlayerStatManager.Instance.OnPlayerHealthChange += PlayerStatManager_OnPlayerHealthChange;
    }

    private void PlayerStatManager_OnPlayerHealthChange(object sender, PlayerStatManager.OnPlayerHealthChangeEventArgs e)
    {
        SetHealth(e.currentHealth, e.maxHealth);
    }

    public void SetHealth(float currHealth, float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = currHealth;
    }
}
