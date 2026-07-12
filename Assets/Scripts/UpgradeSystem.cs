using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    public int gunLevel = 1;
    public int swordLevel = 1;
    public int utilityLevel = 1;
    public int maxGunLevel = 5;
    public int maxSwordLevel = 5;
    public int maxUtilityLevel = 5;

    // Kỹ năng súng
    public bool hasTripleShot = false;
    public bool hasChargeShot = false;
    public bool hasRicochet = false;
    public bool hasExplosiveBullet = false;

    // Kỹ năng kiếm
    public bool hasSpinAttack = false;
    public bool hasAirSlash = false;
    public bool hasDashSlash = false;
    public bool hasCriticalCombo = false;

    // Kỹ năng di chuyển
    public bool hasDoubleJump = false;
    public bool hasWallJump = false;
    public bool hasGrappleHook = false;
    public bool hasAirDash = false;

    public void UpgradeGun()
    {
        if (gunLevel < maxGunLevel)
        {
            gunLevel++;
            Debug.Log("Nâng cấp súng lên cấp " + gunLevel);
            
            // Mở khóa kỹ năng theo cấp
            switch (gunLevel)
            {
                case 2:
                    hasTripleShot = true;
                    Debug.Log("Mở khóa: Triple Shot");
                    break;
                case 3:
                    hasChargeShot = true;
                    Debug.Log("Mở khóa: Charge Shot");
                    break;
                case 4:
                    hasRicochet = true;
                    Debug.Log("Mở khóa: Ricochet");
                    break;
                case 5:
                    hasExplosiveBullet = true;
                    Debug.Log("Mở khóa: Explosive Bullet");
                    break;
            }
        }
        else
        {
            Debug.Log("Súng đã đạt cấp tối đa!");
        }
    }

    public void UpgradeSword()
    {
        if (swordLevel < maxSwordLevel)
        {
            swordLevel++;
            Debug.Log("Nâng cấp kiếm lên cấp " + swordLevel);
            
            // Mở khóa kỹ năng theo cấp
            switch (swordLevel)
            {
                case 2:
                    hasSpinAttack = true;
                    Debug.Log("Mở khóa: Spin Attack");
                    break;
                case 3:
                    hasAirSlash = true;
                    Debug.Log("Mở khóa: Air Slash");
                    break;
                case 4:
                    hasDashSlash = true;
                    Debug.Log("Mở khóa: Dash Slash");
                    break;
                case 5:
                    hasCriticalCombo = true;
                    Debug.Log("Mở khóa: Critical Combo");
                    break;
            }
        }
        else
        {
            Debug.Log("Kiếm đã đạt cấp tối đa!");
        }
    }

    public void UpgradeUtility()
    {
        if (utilityLevel < maxUtilityLevel)
        {
            utilityLevel++;
            Debug.Log("Nâng cấp di chuyển lên cấp " + utilityLevel);
            
            // Mở khóa kỹ năng theo cấp
            switch (utilityLevel)
            {
                case 2:
                    hasDoubleJump = true;
                    Debug.Log("Mở khóa: Double Jump");
                    break;
                case 3:
                    hasWallJump = true;
                    Debug.Log("Mở khóa: Wall Jump");
                    break;
                case 4:
                    hasGrappleHook = true;
                    Debug.Log("Mở khóa: Grapple Hook");
                    break;
                case 5:
                    hasAirDash = true;
                    Debug.Log("Mở khóa: Air Dash");
                    break;
            }
        }
        else
        {
            Debug.Log("Di chuyển đã đạt cấp tối đa!");
        }
    }
}