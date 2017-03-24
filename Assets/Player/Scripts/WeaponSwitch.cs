using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Weapons;

public class WeaponSwitch : MonoBehaviour
{
    bool isWaiting = false;
    public GameObject WeaponHandler;
    static public int CurrentWeapon = 0;
    static public List<Weapon> Wapens = new List<Weapon>();
    public static bool CantShoot = false;
    public static bool CantSwitch = false;
    public bool FullAutoFiring = false;
    public bool BurstFiring = false;
    public static bool PlayedSound = false;
    int OldWeapon = 0;
    public static MultiplayerBase MB;

    void Awake()
    {
        MB = GameObject.Find("MultiplayerBase").GetComponent<MultiplayerBase>();
    }

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        SetLoadOut(new Loadouts.B());
        SwitchWeapon(1);
        ToggleScope();
    }

    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetLoadOut(new Loadouts.A());
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            SetLoadOut(new Loadouts.B());
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            SetLoadOut(new Loadouts.C());
        }

        if (FullAutoFiring)
        {
            Wapens[CurrentWeapon].Shoot();
            try
            {
                StartCoroutine(PreventShoot(Wapens[CurrentWeapon].FireRate));
            }
            catch { Destroy(gameObject); }

        }

        if (Input.GetMouseButtonUp(0))
        {
            FullAutoFiring = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            switch (Wapens[CurrentWeapon].FireMode)
            {
                case 1:
                    Wapens[CurrentWeapon].Shoot();
                    try
                    {
                        StartCoroutine(PreventShoot(Wapens[CurrentWeapon].FireRate));
                    }
                    catch { Destroy(gameObject); }
                    break;
                case 2:
                    FullAutoFiring = true;
                    //Full auto
                    break;
                case 3:
                    if (!BurstFiring && !CantShoot)
                    {
                        try
                        {
                            StartCoroutine(FireBurst(Wapens[CurrentWeapon].FireRate, 3));
                        }
                        catch { Destroy(gameObject); }
                        //3 round burst
                    }
                    break;
                case 4:
                    if (!BurstFiring && !CantShoot)
                    {
                        try
                        {
                            StartCoroutine(FireBurst(Wapens[CurrentWeapon].FireRate, 5));
                        }
                        catch { Destroy(gameObject); }
                        //5 round burst
                    }
                    break;
                case 5:
                    ShootShotgun(Wapens[CurrentWeapon].BulletsInShell);
                    break;
                default:
                    Debug.Log("Rampzalige error: Vuurmodus " + Wapens[CurrentWeapon].FireMode + " bestaat niet!");
                    break;
            }
        }

        if (!isWaiting)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                SwitchWeapon(1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                SwitchWeapon(0);

            }
        }

        if (Input.GetKeyDown(KeyCode.X))
            ToggleFireMode();
        if (Input.GetKeyDown(KeyCode.C))
            ToggleScope();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int TempWeapon = CurrentWeapon;
            CurrentWeapon = OldWeapon;
            OldWeapon = TempWeapon;
            try
            {
                StartCoroutine(SwitchWeapons(OldWeapon));
            }
            catch { Destroy(gameObject); }
        }
    }

    public void ShootShotgun(int BulletsInShell)
    {
        if (!CantShoot)
        {
            Wapens[CurrentWeapon].AmmoInClip += BulletsInShell;

            for (int i = 0; i <= BulletsInShell; i++)
            {
                Wapens[CurrentWeapon].Shoot();

                if (!PlayedSound)
                {
                    PlayedSound = true;
                }
            }
            PlayedSound = false;
            try
            {
                StartCoroutine(PreventShoot(Wapens[CurrentWeapon].FireRate));
            }
            catch { Destroy(gameObject); }
        }
    }

    public void ShootFullAuto()
    {
        while (Input.GetMouseButton(0))
        {
            Wapens[CurrentWeapon].Shoot();
            try
            {
                StartCoroutine(PreventShoot(Wapens[CurrentWeapon].FireRate));
            }
            catch { Destroy(gameObject); }
        }
    }

    public void SwitchWeapon(int Direction)
    {
        if (!CantSwitch)
        {
            OldWeapon = CurrentWeapon;
            if (Direction == 1)
            {
                if (CurrentWeapon != (Wapens.Count - 1))
                {
                    CurrentWeapon++;
                    try
                    {
                        StartCoroutine(SwitchWeapons(OldWeapon));
                    }
                    catch { Destroy(gameObject); }
                }
                else
                {
                    CurrentWeapon = 0;
                    try
                    {
                        StartCoroutine(SwitchWeapons(OldWeapon));
                    }
                    catch { Destroy(gameObject); }
                }
            }
            else
            {
                if (Direction == 0)
                {
                    if (CurrentWeapon != 0)
                    {
                        CurrentWeapon--;
                        try
                        {
                            StartCoroutine(SwitchWeapons(OldWeapon));
                        }
                        catch { Destroy(gameObject); }
                    }
                    else
                    {
                        CurrentWeapon = (Wapens.Count - 1);
                        try
                        {
                            StartCoroutine(SwitchWeapons(OldWeapon));
                        }
                        catch { Destroy(gameObject); }
                    }
                }
            }
            CantShoot = false;
            BurstFiring = false;
            FullAutoFiring = false;
            ADS.AimingDownSights = false;
            ADS.Zoomed = false;
            GameObject ScopeCamera = GameObject.FindGameObjectWithTag("ScopeCamera");
            if (ScopeCamera != null)
            {
                ScopeCamera.GetComponent<Camera>().fieldOfView = ADS.ScopeFOV;
            }
        }
    }

    void ToggleFireMode()
    {
        Wapens[CurrentWeapon].FireModeIndex++;
        if (Wapens[CurrentWeapon].FireModeIndex > Wapens[CurrentWeapon].AllowedFireModes.Length - 1)
            Wapens[CurrentWeapon].FireModeIndex = 0;
        Wapens[CurrentWeapon].FireMode = Wapens[CurrentWeapon].AllowedFireModes[Wapens[CurrentWeapon].FireModeIndex];
    }

    void ToggleScope()
    {
        try
        {
            Wapens[CurrentWeapon].AttachedSights[Wapens[CurrentWeapon].CurrentSight].SetActive(false);
        }
        catch { Destroy(gameObject); }
        Wapens[CurrentWeapon].CurrentSight++;
        if (Wapens[CurrentWeapon].CurrentSight > Wapens[CurrentWeapon].AttachedSights.Length - 1)
            Wapens[CurrentWeapon].CurrentSight = 0;
        try
        {
            Wapens[CurrentWeapon].AttachedSights[Wapens[CurrentWeapon].CurrentSight].SetActive(true);
        }
        catch { Destroy(gameObject); }
        ADS.Zoomed = false;
        GameObject ScopeCamera = GameObject.FindGameObjectWithTag("ScopeCamera");
        if (ScopeCamera != null)
        {
            ScopeCamera.GetComponent<Camera>().fieldOfView = ADS.ScopeFOV;
        }
    }

    IEnumerator FireBurst(float FireRate, int RoundsPerBurst)
    {
        float BetweenBurstMultiplier;
        if (RoundsPerBurst == 3)
            BetweenBurstMultiplier = .6f;
        else if (RoundsPerBurst == 5)
            BetweenBurstMultiplier = .75f;
        else
            BetweenBurstMultiplier = 1;
        BurstFiring = true;
        for (int i = 0; i < RoundsPerBurst; i++)
        {
            Wapens[CurrentWeapon].Shoot();
            CantShoot = true;
            yield return new WaitForSeconds(BetweenBurstMultiplier * FireRate);
            CantShoot = false;
        }
        try
        {
            StartCoroutine(PreventShoot(1.2f * FireRate));
        }
        catch { Destroy(gameObject); }
        BurstFiring = false;
    }

    IEnumerator SwitchWeapons(int OldWeapon)
    {
        if (!isWaiting)
        {
            //ToggleScope();
            isWaiting = true;
            GetComponent<Animation>().CrossFade("UnloadWeapon");
            yield return new WaitForSeconds(0.1f);
            Wapens[OldWeapon].WeaponObject.SetActive(false);
            GetComponent<Animation>().CrossFade("LoadWeapon");
            yield return new WaitForSeconds(0.1f);
            Wapens[CurrentWeapon].WeaponObject.SetActive(true);
            isWaiting = false;
            if (Network.isClient)
            {
                MB.N.RPC("RPCSetWeaponOnServer", RPCMode.Server, MB.PlayerStats.NetworkPlayerInstance, Wapens[CurrentWeapon].Signature);
            }
            GameObject ScopeCamera = GameObject.FindGameObjectWithTag("ScopeCamera");
            if (ScopeCamera != null)
                ScopeCamera.GetComponent<Camera>().fieldOfView = ADS.ScopeFOV;
            Wapens[CurrentWeapon].WeaponObject.transform.localPosition = ADS.HipFirePosition;
        }
    }


    IEnumerator PreventShoot(float TimeToWait)
    {
        if (!CantShoot)
        {
            CantShoot = true;
            yield return new WaitForSeconds(TimeToWait);
            CantShoot = false;
        }
    }

    public void SetLoadOut(Loadouts ChosenLoadout)
    {
        try
        {
            Wapens[CurrentWeapon].WeaponObject.SetActive(false);
        }
        catch
        {

        } 
        Wapens.Clear();
        Wapens.Add(ChosenLoadout.Primary);
        AttachAttachment(ChosenLoadout.Primary.Attachment, ChosenLoadout.Primary);
        Wapens.Add(ChosenLoadout.Secondary);
        AttachAttachment(ChosenLoadout.Secondary.Attachment, ChosenLoadout.Secondary);
        Wapens.Add(ChosenLoadout.Tertiary);
        AttachAttachment(ChosenLoadout.Tertiary.Attachment, ChosenLoadout.Tertiary);
        CurrentWeapon = 2;
        SwitchWeapon(1);
    }

    public void AttachAttachment(string Attachment, Weapon WeaponForAttachment)
    {
        if (Attachment != "" && Attachment != null){
            switch (Attachment)
            {
                case "Extended Mags":
                    WeaponForAttachment.ClipSize = Mathf.RoundToInt(WeaponForAttachment.ClipSize * 1.3f);
                    WeaponForAttachment.AmmoInClip = Mathf.RoundToInt(WeaponForAttachment.AmmoInClip * 1.3f);
                    break;

                case "Foregrip":
                    WeaponForAttachment.RecoilPerShot = Mathf.RoundToInt(WeaponForAttachment.RecoilPerShot * 0.75f);
                    break;

                case "Starburst":
                    Debug.Log("Starburst detected");
                    for (int i = 0; i <= WeaponForAttachment.AllowedFireModes.Length - 1; i++)
                    {
                        if (WeaponForAttachment.AllowedFireModes[i] == 3)
                        {
                            Debug.Log("Burstfire Found");
                            WeaponForAttachment.AllowedFireModes[i] = 4;
                            WeaponForAttachment.FireMode = 4;
                        }
                    }
                    break;

                default:
                    Debug.Log("Grote paniek; Attachment bestaat niet!");
                    break;
            }
        }
    }


}