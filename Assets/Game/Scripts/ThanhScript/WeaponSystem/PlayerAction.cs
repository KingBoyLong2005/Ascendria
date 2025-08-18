using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerAction : MonoBehaviour
{
    [SerializeField] private PlayerWeaponSelector WeaponSelector;

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.isPressed && WeaponSelector.ActiveWeapon != null)
        {
            WeaponSelector.ActiveWeapon.Shoot();
        }
    }
}
