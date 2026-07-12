using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CombatSystem combatSystem;
    public UpgradeSystem upgradeSystem;

    void Start()
    {
        combatSystem = GetComponent<CombatSystem>();
        upgradeSystem = GetComponent<UpgradeSystem>();
    }

    void Update()
    {
        // Kiểm tra input và gọi các hàm tương ứng
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            upgradeSystem.UpgradeGun();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            upgradeSystem.UpgradeSword();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            upgradeSystem.UpgradeUtility();
        }
    }
}