using UnityEngine;
using System.Collections;
using Weapons;

public class Recoil : MonoBehaviour
{
    static Vector3 ReturnLocation;
    public static bool IsRecoiling = false;
    public static Weapon Temp;

    public void Update() {
        if (IsRecoiling == true) { Recoiling(Temp); }
    }

    public void StartRecoiling(Weapon RecoilObject){
        StartCoroutine(StartTRecoiling(RecoilObject));
    }
    public IEnumerator StartTRecoiling(Weapon RecoilObject) {
        Temp = RecoilObject;
        IsRecoiling = true;
        yield return new WaitForSeconds(RecoilObject.FireRate);
        RecoilObject.WeaponObject.transform.localRotation = Quaternion.identity;
        IsRecoiling = false;
    }

    public void Recoiling(Weapon RecoilObject)
    {
        if (RecoilObject.Recoil > 0)
        {
            Quaternion MaxRecoil = Quaternion.Euler(RecoilObject.SetMaxRecoil, 0, 0);
            Vector3 MaxRecoilPosition = RecoilObject.WeaponObject.transform.localPosition;
            MaxRecoilPosition.z -= RecoilObject.SetMaxRecoil / -100;
            RecoilObject.WeaponObject.transform.localRotation = Quaternion.Slerp(RecoilObject.WeaponObject.transform.localRotation, MaxRecoil, Time.deltaTime * RecoilObject.RecoilSpeed);
            RecoilObject.WeaponObject.transform.localPosition = Vector3.Lerp(RecoilObject.WeaponObject.transform.localPosition, MaxRecoilPosition, RecoilObject.RecoilSpeed);
            RecoilObject.Recoil -= Time.deltaTime;
        }
        else
        {
            RecoilObject.Recoil = 0;
            try
            {
                RecoilObject.WeaponObject.transform.localRotation = Quaternion.Slerp(RecoilObject.WeaponObject.transform.localRotation, Quaternion.identity, Time.deltaTime * RecoilObject.RecoilSpeed / 2);
            }
            catch { }
            if (ADS.AimingDownSights)
                ReturnLocation = ADS.ADSPosition;
            else
                ReturnLocation = ADS.HipFirePosition;
            try
            {
                RecoilObject.WeaponObject.transform.localPosition = Vector3.Lerp(RecoilObject.WeaponObject.transform.localPosition, ReturnLocation, Time.deltaTime * RecoilObject.RecoilSpeed / 2);
            }
            catch { }
        }
    }
}