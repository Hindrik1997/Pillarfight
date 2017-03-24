using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InterfaceScript : MonoBehaviour {

    public GameObject AmmoInClipDisplay;
    public GameObject MagsDisplay;
    public GameObject FireModeDisplay;
    public GameObject Crosshair;
    static GameObject SCrosshair;
    public GameObject Stability;
    string FireMode;
    public static MultiplayerBase MB;

    void Awake()
    {
        if (Network.isClient)
        {
            MB = GameObject.Find("MultiplayerBase").GetComponent<MultiplayerBase>();

        }
    }

    // Use this for initialization
	void Start () {
         SCrosshair = Crosshair;
    }
	
	// Update is called once per frame
	void Update () {
        AmmoInClipDisplay.GetComponent<Text>().text = WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].AmmoInClip.ToString();
        MagsDisplay.GetComponent<Text>().text = (WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].CurrentClips * WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].ClipSize).ToString();
        switch (WeaponSwitch.Wapens[WeaponSwitch.CurrentWeapon].FireMode)
        {
            case 1:
                FireMode = "Semi-Auto";
                break;
            case 2:
                FireMode = "Full-Auto";
                break;
            case 3:
                FireMode = "3-Burst";
                break;
            case 4:
                FireMode = "5-Burst";
                break;
            case 5:
                FireMode = "Shotgun";
                break;
            default:
                FireMode = "Invalid Firemode";
                break;
        }
        FireModeDisplay.GetComponent<Text>().text = FireMode;
        if (Network.isClient)
        {
            try
            {
                Stability.GetComponent<Text>().text = MB.PlayerStats.Stability.ToString();
            }
            catch { }
        }
        else
        {
            Stability.GetComponent<Text>().text = "Disabled";
        }
    }

     public static void DisableCrosshair() {
         SCrosshair.SetActive(false);
     }

     public static void EnableCrosshair()
     {
         SCrosshair.SetActive(true);
     }
}
