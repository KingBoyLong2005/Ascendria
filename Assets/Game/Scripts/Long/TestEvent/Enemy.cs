using UnityEngine;

public class Enemy : MonoBehaviour {
    public int hp = 1;

    public void TakeDamage(int dmg) {
        hp -= dmg;
        if (hp <= 0) Die();
    }

    void Die() {
        GameManager.Instance.AddKill();
        Destroy(gameObject);
    }

    // Tạm test bằng phím chuột trái bắn chết
    void OnMouseDown() {
        TakeDamage(1);
    }
}
