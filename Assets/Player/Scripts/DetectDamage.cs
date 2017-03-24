using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Weapons;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DetectDamage : MonoBehaviour {

    public static MultiplayerBase MB;
    public bool BHatShot = false;
    public NetworkPlayer Owner = new NetworkPlayer();

    public void Awake()
    {
        MB = GameObject.Find("MultiplayerBase").GetComponent<MultiplayerBase>();
    }

    [RPC]
    void SetOwner(NetworkPlayer NP)
    {
        Owner = NP;
        Debug.Log("OwnerID Player: " + Owner);

    }

    
	// Update is called once per frame
	void Update () {

	}

    public void ReceiveDamage(List<System.Object> MPList)
    {
        int Damage = 0;
        NetworkPlayer Killer;
        string WeaponRef = null;

        Damage = (int)MPList[0];
        if (BHatShot)
        {
            Damage = Damage * 2;
        }
        Killer = (NetworkPlayer)MPList[1];
        WeaponRef = (string)MPList[2];
        Debug.Log(WeaponRef);

        NetworkPlayer Victimpje = Owner;

        Debug.Log(Damage +"-"+Killer+"-"+WeaponRef+"-"+Victimpje);


        MB.N.RPC("FetchPlayerStats", RPCMode.Server, Victimpje , Damage, Killer, WeaponRef);
    }

    public void ReceiveDamage(int Damage)
    {
        MB.PlayerStats.Stability = -Damage;
    }

    [RPC]
    public void InterpretData(byte[] SerializedData, NetworkPlayer Victim, int Damage, NetworkPlayer Killer, string WeaponRef)
    {
        BinaryFormatter BinForm = new BinaryFormatter();
        MemoryStream MemStrm = new MemoryStream();
        MemStrm.Write(SerializedData, 0, SerializedData.Length);
        MemStrm.Seek(0, SeekOrigin.Begin);
        String[] Data = (String[])BinForm.Deserialize(MemStrm);

        int Stability = Convert.ToInt32(Data[1]);
        int AdditionalStability = Convert.ToInt32(Data[2]);
        int ExtraDamage = 0;
        
        if (AdditionalStability != 0)
        {
            AdditionalStability =- Damage;
            MB.SetAdditionalStabilityOnServer(Victim, AdditionalStability);
            if (AdditionalStability < 0)
            {
                ExtraDamage = AdditionalStability * -1;
                Stability =- ExtraDamage;
                MB.SetStabilityOnServer(Victim, Stability);
            }
        }
        else
        {
            Stability = -Damage;
            MB.SetStabilityOnServer(Victim, Stability);
        }
        if (Stability <= 0)  
        {
        MB.RegisterKill(Killer,Victim,WeaponRef, BHatShot);
        }

    }
}

