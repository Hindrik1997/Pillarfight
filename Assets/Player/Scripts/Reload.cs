using UnityEngine;
using System.Collections;

public class Reload : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            StartCoroutine(ReloadWeapon());
    }
    IEnumerator ReloadWeapon()
    {
        if (WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].CurrentClips > 0 && WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].AmmoInClip != WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].ClipSize)
        {
            WeaponSwitch.CantSwitch = true;
            WeaponSwitch.CantShoot = true;
            ADS.AimingDownSights = false;
            try
            {
                WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].WeaponObject.transform.Find("Magazijn").gameObject.GetComponent<Animation>().Play("Reload");
            }
            catch {}

            yield return new WaitForSeconds(1.5f);
            WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].CurrentClips--;
            WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].AmmoInClip = WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].ClipSize;
            WeaponSwitch.CantShoot = false;
            WeaponSwitch.CantSwitch = false;
        }
    }
}