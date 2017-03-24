using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponClasses : MonoBehaviour
{
    public GameObject PubPistolModel;
    public GameObject PubSMGModel;
    public GameObject PubSniperModel;
    public GameObject PubBoltActionSniperModel;
    public GameObject PubShotgunModel;
    public GameObject PubMeleeModel;
    public GameObject PubBAM16A4Model;
    public GameObject PubPlayer;
    public GameObject PubBulletPrefab;

    public GameObject[] PubPistolSights;
    public GameObject[] PubSMGSights;
    public GameObject[] PubSniperSights;
    public GameObject[] PubBoltActionSniperSights;
    public GameObject[] PubShotgunSights;
    public GameObject[] PubBAM16A4Sights;

    public GameObject RaySource;
    public Recoil RecoilScript;
    public static MultiplayerBase MB;

    //Deze functie stelt de statische variabelen in.
    void Awake()
    {
        if (Network.isClient)
        {
            MB = GameObject.Find("MultiplayerBase").GetComponent<MultiplayerBase>();
        }
        PistolModel = PubPistolModel;
        SMGModel = PubSMGModel;
        SniperModel = PubSniperModel;
        BoltActionSniperModel = PubBoltActionSniperModel;
        ShotgunModel = PubShotgunModel;
        MeleeModel = PubMeleeModel;
        BAM16A4Model = PubBAM16A4Model;
        BulletPrefab = PubBulletPrefab;

        PistolSights = PubPistolSights;
        SMGSights = PubSMGSights;
        SniperSights = PubSniperSights;
        BoltActionSniperSights = PubBoltActionSniperSights;
        ShotgunSights = PubShotgunSights;
        BAM16A4Sights = PubBAM16A4Sights;
        StaticRaySource = RaySource;
        StaticRecoilScript = RecoilScript;
        }

    public static GameObject PistolModel;
    public static GameObject SMGModel;
    public static GameObject SniperModel;
    public static GameObject BoltActionSniperModel;
    public static GameObject ShotgunModel;
    public static GameObject MeleeModel;
    public static GameObject BAM16A4Model;
    public static GameObject BulletPrefab;

    public static GameObject[] PistolSights;
    public static GameObject[] SMGSights;
    public static GameObject[] SniperSights;
    public static GameObject[] BoltActionSniperSights;
    public static GameObject[] ShotgunSights;
    public static GameObject[] BAM16A4Sights;

    public static GameObject StaticRaySource;
    public static Recoil StaticRecoilScript;
}

namespace Weapons
{

   
    public abstract class Weapon
    {
        public GameObject WeaponObject { get; set; }
        public int ClipSize { get; set; }
        public int AmmoInClip { get; set; }
        public int MaxClips { get; set; }
        public int BulletAmount { get { return ClipSize * MaxClips; } }
        public float Spread { get; set; }
        public float HipSpread { get; set; }
        public int DamagePerBullet { get; set; }
        public float FireRate { get; set; }
        public int MaxDistance { get; set; }
        public float MaxEffectiveDistance { get; set; }
        public float Recoil { get; set; }
        public float RecoilPerShot { get; set; }
        public float RecoilSpeed { get; set; }
        public float SetMaxRecoil { get; set; }
        public int CurrentClips { get; set; }
        public int FireMode { get; set; }//1 semi, 2 is full auto, 3 is burst fire, 4 is 5 round burst en 5 is shotgun
        public int BulletsInShell { get; set; }
        public int FireModeIndex { get; set; }
        public int[] AllowedFireModes { get; set; }
        public GameObject[] AttachedSights { get; set; }
        public int CurrentSight { get; set; }
        public string[] PossibleAttachments { get; set; }
        public string Attachment { get; set; }
        public string Signature { get; set; }

        GameObject BulletSource;
        GameObject RaySource = WeaponClasses.StaticRaySource;
        int WeaponLayerMask = ~(1 << 9);
                
        virtual public void Shoot() 
        {
            if (!WeaponSwitch.CantShoot)
            {
                if (AmmoInClip > 0)
                {
                   BulletSource = this.WeaponObject.transform.Find("BulletSource").gameObject;
                   
                    RaycastHit hit;
                    if (Physics.Raycast(RaySource.transform.position, RaySource.transform.forward, out hit, 500, WeaponLayerMask))
                    {
                        Vector3 LookPoint = hit.point;
                        BulletSource.transform.LookAt(LookPoint);
                    }

                    this.AmmoInClip--;

                    if (!WeaponSwitch.PlayedSound)
                    {
                        this.WeaponObject.GetComponent<AudioSource>().Play();
                    }
                    
                    float OffsetX;
                    float OffsetY;
                    float OffsetZ;

                    if (ADS.ADSPositionReached)
                    {
                        OffsetX = Random.Range(-this.Spread, this.Spread);
                        OffsetY = Random.Range(-this.Spread, this.Spread);
                        OffsetZ = Random.Range(-this.Spread, this.Spread);
                    }

                    else
                    {
                        OffsetX = Random.Range(-this.HipSpread, this.HipSpread);
                        OffsetY = Random.Range(-this.HipSpread, this.HipSpread);
                        OffsetZ = Random.Range(-this.HipSpread, this.HipSpread);                    
                    }

                    Vector3 RayDirection = RaySource.transform.forward;
                    RayDirection.x += OffsetX;
                    RayDirection.y += OffsetY;
                    RayDirection.z += OffsetZ;

                    Vector3 BulletDirection = BulletSource.transform.forward;
                    BulletDirection.x += OffsetX;
                    BulletDirection.y += OffsetY;
                    BulletDirection.z += OffsetZ;

                    if (Network.connections.Length > 0)
                    {
                        GameObject clone = Network.Instantiate(WeaponClasses.BulletPrefab, BulletSource.transform.position, BulletSource.transform.rotation,0) as GameObject;
                        clone.GetComponent<Rigidbody>().AddForce(BulletDirection * 2000);
                    }
                    else
                    {
                        GameObject clone = MonoBehaviour.Instantiate(WeaponClasses.BulletPrefab, BulletSource.transform.position, BulletSource.transform.rotation) as GameObject;
                        clone.GetComponent<Rigidbody>().AddForce(BulletDirection * 2000);
                    }

                    //Debug.DrawRay(RaySource.transform.position, RayDirection, Color.red);
                    //Debug.Log(hit.point);
                    if (Physics.Raycast(Camera.main.transform.position, RayDirection, out hit, this.MaxDistance, WeaponLayerMask))
                    {
                        int Damage;

                        if (hit.distance > MaxEffectiveDistance)
                        {
                            Damage = this.DamagePerBullet / 2;
                        }
                        else
                        {
                            Damage = this.DamagePerBullet;
                        }
                        if (Network.isClient)
                        {
                            List<System.Object> StuffToSend = new List<System.Object>();
                            StuffToSend.Add(Damage);
                            StuffToSend.Add(WeaponClasses.MB.PlayerStats.NetworkPlayerInstance);
                            StuffToSend.Add(WeaponClasses.MB.PlayerStats.CurrentWeapon);
                            hit.transform.SendMessage("ReceiveDamage", StuffToSend, SendMessageOptions.DontRequireReceiver);
                            //HIER GAAT IETS MIS
                        }
                        else
                        {
                            hit.transform.SendMessage("ReceiveDamage", Damage, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    this.Recoil += this.RecoilPerShot;
                    WeaponClasses.StaticRecoilScript.StartRecoiling(this);

                }
            }
        }

        //WAPENS--------------------------------------------------------------------------

        public class Pistol : Weapon
        {
            public Pistol()
            {
                this.Signature = "Zapper";
                this.WeaponObject = WeaponClasses.PistolModel;
                this.ClipSize = 9;
                this.AmmoInClip = this.ClipSize;
                this.MaxClips = 16;
                this.CurrentClips = this.MaxClips;
                this.Spread = 0.025f;
                this.HipSpread = 0.05f;
                this.DamagePerBullet = 20;
                this.FireRate = 0.3f;
                this.MaxDistance = 35;
                this.MaxEffectiveDistance = 25;
                this.Recoil = 0;
                this.SetMaxRecoil = -7;
                this.RecoilPerShot = 0.02f;
                this.RecoilSpeed = 10f;
                this.FireMode = 1;
                this.FireModeIndex = 0;
                this.AllowedFireModes = new int[2] { 1, 3 };
                this.AttachedSights = WeaponClasses.PistolSights;
                this.CurrentSight = 0;
            }
        }
        public class SMG : Weapon
        {
            public SMG()
            {
                this.Signature = "P90";
                this.WeaponObject = WeaponClasses.SMGModel;
                this.ClipSize = 60;
                this.AmmoInClip = this.ClipSize;
                this.MaxClips = 12;
                this.CurrentClips = this.MaxClips;
                this.Spread = 0.05f;
                this.HipSpread = 0.1f;
                this.DamagePerBullet = 15;
                this.FireRate = 0.05f;
                this.MaxDistance = 30;
                this.MaxEffectiveDistance = 20;
                this.Recoil = 0;
                this.SetMaxRecoil = -7;
                this.RecoilPerShot = 0.01f;
                this.RecoilSpeed = 10f;
                this.FireMode = 2;
                this.FireModeIndex = 0;
                this.AllowedFireModes = new int[2] { 1, 2 };
                this.AttachedSights = WeaponClasses.SMGSights;
                this.CurrentSight = 0;
            }
        }
        public class Sniper : Weapon
        {
            public Sniper()
            {
                this.Signature = "Mesh Sniper";
                this.WeaponObject = WeaponClasses.SniperModel;
                this.ClipSize = 8;
                this.AmmoInClip = this.ClipSize;
                this.MaxClips = 16;
                this.CurrentClips = this.MaxClips;
                this.Spread = 0.0f;
                this.HipSpread = 0.15f;
                this.DamagePerBullet = 75;
                this.FireRate = 1f;
                this.MaxDistance = 200;
                this.MaxEffectiveDistance = 175;
                this.Recoil = 0;
                this.SetMaxRecoil = -10;
                this.RecoilPerShot = 0.03f;
                this.RecoilSpeed = 10f;
                this.FireMode = 1;
                this.FireModeIndex = 0;
                this.AllowedFireModes = new int[1] { 1 };
                this.AttachedSights = WeaponClasses.SniperSights;
                this.CurrentSight = 0;
            }
        }
        public class BoltActionSniper : Weapon
        {
            public BoltActionSniper()
            {
                this.Signature = "Krabiimov";
                this.WeaponObject = WeaponClasses.BoltActionSniperModel;
                this.ClipSize = 12;
                this.AmmoInClip = this.ClipSize;
                this.MaxClips = 14;
                this.CurrentClips = this.MaxClips;
                this.Spread = 0.0f;
                this.HipSpread = 0.15f;
                this.DamagePerBullet = 175;
                this.FireRate = 2f;
                this.MaxDistance = 200;
                this.MaxEffectiveDistance = 150;
                this.Recoil = 0;
                this.SetMaxRecoil = -10;
                this.RecoilPerShot = 0.06f;
                this.RecoilSpeed = 10f;
                this.FireMode = 1;
                this.FireModeIndex = 0;
                this.AllowedFireModes = new int[1] { 1 };
                this.AttachedSights = WeaponClasses.BoltActionSniperSights;
                this.CurrentSight = 0;
                }
        }
        public class Shotgun : Weapon
        {
            public Shotgun()
            {
                this.Signature = "Leafblower";
                this.WeaponObject = WeaponClasses.ShotgunModel;
                this.ClipSize = 8;
                this.AmmoInClip = this.ClipSize;
                this.MaxClips = 16;
                this.CurrentClips = this.MaxClips;
                this.Spread = 0.1f;
                this.HipSpread = 0.1f;
                this.DamagePerBullet = 4;
                this.FireRate = 0.6f;
                this.MaxDistance = 35;
                this.MaxEffectiveDistance = 25;
                this.Recoil = 0;
                this.SetMaxRecoil = -12;
                this.RecoilPerShot = 0.001f;
                this.RecoilSpeed = 10f; 
                this.BulletsInShell = 30;
                this.FireMode = 5;
                this.FireModeIndex = 0;
                this.AllowedFireModes = new int[1] { 5 };
                this.AttachedSights = WeaponClasses.ShotgunSights;
                this.CurrentSight = 0;
            }
        }
        public class Melee : Weapon
        {
            public Melee()
            {
                this.Signature = "Knife";
                this.WeaponObject = WeaponClasses.MeleeModel;
                this.ClipSize = 9;
                this.AmmoInClip = this.ClipSize;
                this.MaxClips = 16;
                this.CurrentClips = this.MaxClips;
                this.Spread = 0.025f;
                this.DamagePerBullet = 120;
                this.FireRate = 0.2f;
                this.MaxDistance = 35;
                this.MaxEffectiveDistance = 25;
                this.Recoil = 0;
                this.SetMaxRecoil = -10;
                this.RecoilPerShot = 1;
                this.RecoilSpeed = 10f;
                this.FireMode = 1;
                this.FireModeIndex = 0;
                this.AllowedFireModes = new int[1] { 1 };
                this.AttachedSights = new GameObject[0];
                this.CurrentSight = 0;
                }
        }
        public class BAM16A4 : Weapon
        {
            public BAM16A4()
            {
                this.Signature = "BAM16A4";
                this.WeaponObject = WeaponClasses.BAM16A4Model;
                this.ClipSize = 30;
                this.AmmoInClip = this.ClipSize;
                this.MaxClips = 8;
                this.CurrentClips = this.MaxClips;
                this.Spread = 0.01f;
                this.HipSpread = 0.02f;
                this.DamagePerBullet = 16;
                this.FireRate = 0.15f;
                this.MaxDistance = 200;
                this.MaxEffectiveDistance = 175;
                this.Recoil = 0;
                this.SetMaxRecoil = -5;
                this.RecoilPerShot = 0.01f;
                this.RecoilSpeed = 10f;
                this.FireMode = 3;
                this.FireModeIndex = 2;
                this.AllowedFireModes = new int[3] { 1, 2, 3 };
                this.AttachedSights = WeaponClasses.BAM16A4Sights;
                this.CurrentSight = 0;
                this.PossibleAttachments = new string[3] { "Extended Mags", "Starburst", "Foregrip" };
            }
        }
    }
}
