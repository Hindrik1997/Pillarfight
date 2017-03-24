using UnityEngine;
using System.Collections;

public class ADS : MonoBehaviour
{

    public static bool AimingDownSights = false;
    public static bool ADSPositionReached = false;
    public static bool HipFirePositionReached = false;
    public static Vector3 HipFirePosition = new Vector3(0.01f, 0.73f, -0.19f);
    public static Vector3 ADSPosition = new Vector3(-1f, 1, -1f);
    public static float NormalFOV;
    public static float ADSFOV;
    public static float ScopeFOV;
    public static float IncreasedZoomFOV;
    public static bool Zoomed;

    // Use this for initialization
    void Start()
    {
        HipFirePositionReached = true;
        NormalFOV = Camera.main.fieldOfView;
        ADSFOV = NormalFOV / 1.2f;
        ScopeFOV = NormalFOV / 2.5f;
        IncreasedZoomFOV = NormalFOV / 4.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            AimingDownSights = !AimingDownSights;
        }

        if (AimingDownSights)
            StartADS();
        else
            StopADS();
        try
        {

            if (Vector3.Distance(WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].WeaponObject.transform.localPosition, HipFirePosition) < 0.000001)
            {
                HipFirePositionReached = true;
                WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].WeaponObject.transform.localPosition = HipFirePosition;
            }
            else
                HipFirePositionReached = false;

            if (Vector3.Distance(WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].WeaponObject.transform.localPosition, ADSPosition) < 0.000001)
            {
                ADSPositionReached = true;
                WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].WeaponObject.transform.localPosition = ADSPosition;
            }
            else
                ADSPositionReached = false;
        }
        catch { }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ToggleZoom();
        }
    }

    public static void StartADS()
    {
        if (!ADSPositionReached && WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].Recoil == 0)
        {
            WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].WeaponObject.transform.localPosition = Vector3.Slerp(WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].WeaponObject.transform.localPosition, ADSPosition, Time.deltaTime * 25f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, ADSFOV, Time.deltaTime * 25f);
            try
            {
                InterfaceScript.DisableCrosshair();
            }
            catch { }
        }
    }

    public static void StopADS()
    {
        if (!HipFirePositionReached && WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].Recoil == 0)
        {
            WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].WeaponObject.transform.localPosition = Vector3.Slerp(WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].WeaponObject.transform.localPosition, HipFirePosition, Time.deltaTime * 25f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, NormalFOV, Time.deltaTime * 25f);
            try
            {
                InterfaceScript.EnableCrosshair();
            }
            catch { }
        }
    }

    void ToggleZoom()
    {
        Zoomed = !Zoomed;
        GameObject ScopeCamera = GameObject.FindGameObjectWithTag("ScopeCamera");
        if (Zoomed == true)
        {
            if (ScopeCamera != null)
            {
                ScopeCamera.GetComponent<Camera>().fieldOfView = IncreasedZoomFOV;
            }
        }
        else
        {
            if (ScopeCamera != null)
            {
                ScopeCamera.GetComponent<Camera>().fieldOfView = ScopeFOV;
            }
        }
    }
}
