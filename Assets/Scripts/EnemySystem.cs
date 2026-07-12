using UnityEngine;

public class EnemySystem : MonoBehaviour
{
    public enum EnemyType { Drone, Spider, Golem, Phantom, BossGolem, BossPhantom }
    public EnemyType enemyType;
    public int health = 50;
    public int damage = 10;
    public float moveSpeed = 2f;
    public bool isStunned = false;
    public float stunDuration = 2f;

    void Start()
    {
        switch (enemyType)
        {
            case EnemyType.Drone:
                health = 30;
                damage = 5;
                moveSpeed = 3f;
                Debug.Log("Steam Drone: Bay lơ lửng và bắn đạn nhỏ.");
                break;
            case EnemyType.Spider:
                health = 40;
                damage = 8;
                moveSpeed = 4f;
                Debug.Log("Mechanical Spider: Di chuyển nhanh trên tường và trần.");
                break;
            case EnemyType.Golem:
                health = 100;
                damage = 15;
                moveSpeed = 1f;
                Debug.Log("Lava Golem: Ném đá lửa và đấm mạnh.");
                break;
            case EnemyType.Phantom:
                health = 60;
                damage = 12;
                moveSpeed = 3.5f;
                Debug.Log("Mana Phantom: Bay lơ lửng và bắn tia năng lượng.");
                break;
            case EnemyType.BossGolem:
                health = 300;
                damage = 20;
                moveSpeed = 1.5f;
                Debug.Log("Boss: Steam Golem - Phase 1: Đấm và bắn rocket.");
                break;
            case EnemyType.BossPhantom:
                health = 250;
                damage = 18;
                moveSpeed = 4f;
                Debug.Log("Boss: Mana Phantom - Bay lơ lửng và bắn tia năng lượng.");
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log(enemyType + " nhận " + damage + " sát thương. Health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    public void Stun()
    {
        isStunned = true;
        Debug.Log(enemyType + " bị stunned!");
        Invoke("EndStun", stunDuration);
    }

    private void EndStun()
    {
        isStunned = false;
        Debug.Log(enemyType + " hết stunned.");
    }

    private void Die()
    {
        Debug.Log(enemyType + " chết!");
        // Thêm logic chết (drop item, biến mất, etc.)
        Destroy(gameObject);
    }

    public void Attack()
    {
        if (!isStunned)
        {
            Debug.Log(enemyType + " tấn công! Sát thương: " + damage);
            // Thêm logic tấn công
        }
    }
}