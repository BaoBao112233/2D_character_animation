using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public int health = 100;
    public int maxHealth = 100;
    public int mana = 50;
    public int maxMana = 100;
    public int heat = 0;
    public int maxHeat = 100;
    public float heatCooldownRate = 5f;  // Heat giảm mỗi giây
    public bool isDashing = false;
    public bool isParrying = false;
    public float dashDuration = 0.3f;  // Thời gian dash
    public float parryWindow = 0.2f;   // Thời gian parry hiệu quả
    public float dashCooldown = 1f;     // Thời gian hồi dash
    public float parryCooldown = 1.5f;  // Thời gian hồi parry

    private float lastDashTime = -1f;
    private float lastParryTime = -1f;
    private int slashCombo = 0;
    private float lastSlashTime = 0f;
    private float slashComboResetTime = 1f;  // Thời gian reset combo

    void Update()
    {
        // Giảm heat theo thời gian
        if (heat > 0)
        {
            heat = Mathf.Max(0, heat - Mathf.RoundToInt(heatCooldownRate * Time.deltaTime));
        }

        // Reset combo nếu quá thời gian
        if (slashCombo > 0 && Time.time - lastSlashTime > slashComboResetTime)
        {
            slashCombo = 0;
        }

        // Kiểm tra input
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Slash();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Parry();
        }
    }

    public void Shoot()
    {
        if (heat < maxHeat)
        {
            heat += 10;
            Debug.Log("Bắn! Heat: " + heat);
            // Thêm logic bắn đạn ở đây
        }
        else
        {
            Debug.Log("Overheat! Chờ hồi...");
        }
    }

    public void Slash()
    {
        slashCombo++;
        lastSlashTime = Time.time;

        switch (slashCombo)
        {
            case 1:
                Debug.Log("Slash 1");
                break;
            case 2:
                Debug.Log("Slash 2");
                break;
            case 3:
                Debug.Log("Slash 3 - Heavy Slash!");
                slashCombo = 0;  // Reset combo sau đòn nặng
                break;
        }
    }

    public void Dash()
    {
        if (Time.time - lastDashTime > dashCooldown)
        {
            isDashing = true;
            lastDashTime = Time.time;
            Debug.Log("Dash!");
            Invoke("EndDash", dashDuration);
        }
        else
        {
            Debug.Log("Dash chưa hồi!");
        }
    }

    private void EndDash()
    {
        isDashing = false;
    }

    public void Parry()
    {
        if (Time.time - lastParryTime > parryCooldown)
        {
            isParrying = true;
            lastParryTime = Time.time;
            Debug.Log("Parry!");
            Invoke("EndParry", parryWindow);
        }
        else
        {
            Debug.Log("Parry chưa hồi!");
        }
    }

    private void EndParry()
    {
        isParrying = false;
    }

    public void TakeDamage(int damage)
    {
        if (isParrying)
        {
            Debug.Log("Parry thành công! Enemy stunned.");
            // Thêm logic stunned enemy
            return;
        }

        health -= damage;
        Debug.Log("Nhận " + damage + " sát thương. Health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Ember chết!");
        // Thêm logic chết (reload scene, game over, etc.)
    }
}