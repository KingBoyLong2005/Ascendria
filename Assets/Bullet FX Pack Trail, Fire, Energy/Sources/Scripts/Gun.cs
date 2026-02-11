using UnityEngine;

namespace bullet.fx.pack {
    public sealed class Gun : MonoBehaviour
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float bulletSpeed;
        [SerializeField] private float gravityForce;

        public void Shoot(Vector3 forward) {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(forward - firePoint.position) * Quaternion.Euler(90, 0, 0));
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            rb.linearVelocity = (forward - firePoint.position) * bulletSpeed;
        
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        }
    }
}