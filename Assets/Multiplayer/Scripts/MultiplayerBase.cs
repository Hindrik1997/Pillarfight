using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomNetworkCode;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Weapons;

public class MultiplayerBase : MonoBehaviour
{
    public string Name = "A dumbass without a name";
    public string ConnectionIP = "192.168.1.38";
    public int PortNumber = 2500;
    public bool isIngame = false;
    public bool isConnected = false;
    public int MaxClients = 48;
    public List<Player> Players = new List<Player>();
    public Player PlayerStats;
    public bool Errormessage = false;
    public bool MainMenu = true;
    public GameObject PillarModel;
    public GameObject PillarToSpawn;
    public GameObject RefObject;
    public bool SyncPillarModel = false;
    public Transform FPSPos;
    public NetworkView N;
    public GUISkin ChatBox;
    public bool Chat = false;
    string CurrentMessage = "";
    public Vector2 ScrollVector1 = new Vector2();
    public Vector2 ScrollVector2 = new Vector2();

    public List<string> KillHistory = new List<string>();
    public List<string> KillFeed = new List<string>();
    public List<string> ChatHistory = new List<string>();


    public void Awake() {
        N = gameObject.GetComponent<NetworkView>();
        DontDestroyOnLoad(gameObject);
    }
    
    //Kill/Death/Stability/AddStability - ClientMethods
    [RPC]
    public void SetKillsOnClient()
    {
        PlayerStats.Kills = +1;
    }
    [RPC]
    public void SetDeathsOnClient()
    {
        PlayerStats.Kills = +1;
    }
    [RPC]
    public void SetStabilityOnClient(int Stab)
    {
        PlayerStats.Stability =+ Stab;
    }
    [RPC]
    public void SetAddStabilityOnClient(int Stab)
    {
        PlayerStats.AdditionalStability = +Stab;
    }
    [RPC]
    public void SetWeapon(string SelectedWeapon)
    {
        PlayerStats.CurrentWeapon = SelectedWeapon;
    }
    [RPC]
    public void SetClass(int SelectedClass)
    {
        PlayerStats.Class = SelectedClass;
    }

    //Server methods callen de client varianten
    [RPC]
    public void RPCRegisterKill(NetworkPlayer Killer, NetworkPlayer Victim, string KillWeapon, bool HatShot)
    {
        string KillName = "";
        string VictimName = "";
        foreach (Player TKiller in Players) {
            if (TKiller.NetworkPlayerInstance == Killer)
            {
                KillName = TKiller.Name;
                TKiller.Kills += 1;
                N.RPC("SetKillsOnClient", TKiller.NetworkPlayerInstance);
            }
            else
            {
                if(TKiller.NetworkPlayerInstance == Victim)
                {
                    VictimName = TKiller.Name;
                    TKiller.Deaths += 1;
                    N.RPC("SetDeathsOnClient", TKiller.NetworkPlayerInstance);
                }
            }
        }
        ProcessKill(KillName, VictimName,KillWeapon,HatShot);
    }
    [RPC]
    public void RPCSetStabilityOnServer(NetworkPlayer NPlayer, int Mod)
    {
        foreach (Player T in Players)
        {
            if(T.NetworkPlayerInstance == NPlayer)
            {
                T.Stability =+ Mod;
                N.RPC("SetStabilityOnClient", T.NetworkPlayerInstance);
            }
        }
    }
    [RPC]
    public void RPCSetAdditionalStabilityOnServer(NetworkPlayer NPlayer, int Mod)
    {
        foreach (Player T in Players)
        {
            if (T.NetworkPlayerInstance == NPlayer)
            {
                T.AdditionalStability =+ Mod;
                N.RPC("SetAdditionalStabilityOnClient", T.NetworkPlayerInstance);
            }
        }
    }
    [RPC]
    public void RPCSetDeathsOnServer(NetworkPlayer NPlayer)
    {
        foreach (Player T in Players)
        {
            if (T.NetworkPlayerInstance == NPlayer)
            {
                T.Deaths=+1;
                N.RPC("SetDeathsOnClient", T.NetworkPlayerInstance);
            }
        }
    }
    [RPC]
    public void RPCSetClassOnServer(NetworkPlayer NPlayer, int Class)
    {
        foreach (Player T in Players)
        {
            if (T.NetworkPlayerInstance == NPlayer)
            {
                T.Class = Class;
                N.RPC("SetClass", T.NetworkPlayerInstance,Class);
            }
        }
    }
    [RPC]
    public void RPCSetWeaponOnServer(NetworkPlayer NPlayer, string TWeapon)
    {
        foreach (Player T in Players)
        {
            if (T.NetworkPlayerInstance == NPlayer)
            {
                T.CurrentWeapon = TWeapon;
                N.RPC("SetWeapon", T.NetworkPlayerInstance, TWeapon);
            }
        }
    }

    //CallMethods -- Deze moeten dus gecalled worden
    public void RegisterKill(NetworkPlayer Killer, NetworkPlayer Victim, string KillWeapon, bool Hatshot)
    {
        N.RPC("RPCRegisterKill", RPCMode.All, Killer, Victim, KillWeapon, Hatshot);
    }
    public void SetStabilityOnServer(NetworkPlayer Victim, int DamageDone)
    {
        N.RPC("RPCSetStabilityOnServer", RPCMode.Server, Victim, DamageDone );
    }
    public void SetAdditionalStabilityOnServer(NetworkPlayer NPlayer, int Mod)
    {
        N.RPC("RPCSetAdditionalStabilityOnServer", RPCMode.All, NPlayer, Mod);
    }
    public void SetDeathOnServer(NetworkPlayer NPlayer) 
    {
        N.RPC("RPCSetDeathOnServer", RPCMode.Server, NPlayer);
    }
    public void SetClassOnServer(NetworkPlayer NPlayer, int Class)
    {
        N.RPC("RPCSetClassOnServer", RPCMode.Server, NPlayer, Class);
    }
    public void SetWeaponOnServer(NetworkPlayer NPlayer, string Weapon)
    {
        N.RPC("RPCSetWeaponOnServer", RPCMode.Server, NPlayer, Weapon);
    }


    //RPC calls voor initialization met server
    [RPC]
    public void SetPlayerStats(byte[] SerializedEndData, NetworkPlayer PlayerData)
    {
        BinaryFormatter BinForm = new BinaryFormatter();
        MemoryStream MemStrm = new MemoryStream();
        MemStrm.Write(SerializedEndData, 0, SerializedEndData.Length);
        MemStrm.Seek(0, SeekOrigin.Begin);
        String[] TempData = (String[])BinForm.Deserialize(MemStrm);
        Player TempPlayerVar = new Player { Name = TempData[0], IP = TempData[1], NetworkPlayerInstance = PlayerData, Kills = 0, Deaths = 0 };
        Players.Add(TempPlayerVar);
        Debug.Log("Succesfully set the data from player: " + TempPlayerVar.Name + "");
        SendPlayerStatsToClient(TempPlayerVar.NetworkPlayerInstance, TempPlayerVar.Name, TempPlayerVar.IP, TempPlayerVar.Kills, TempPlayerVar.Deaths, 100, 0);
        //Bevestiging door terug te zenden naar client
        TempPlayerVar.Dispose();
        //Doordat ik de IDisposable Interface geimplementeerd heb in deze class, 
        //kan ik Dispose() gebruiken om hem te markeren voor verwijdering door de Garbage Collector
    }
    [RPC]
    public void SetPlayerStatsOnClient(NetworkPlayer RPlayer, string RName, int RKills, int RDeaths, string RIP, int RStability, int RAdditionalStability)
    {
        try
        {
            PlayerStats = new Player();
            PlayerStats.Name = RName;
            PlayerStats.IP = RIP;
            PlayerStats.Kills = RKills;
            PlayerStats.Deaths = RDeaths;
            PlayerStats.Stability = RStability;
            PlayerStats.AdditionalStability = RAdditionalStability;
            PlayerStats.NetworkPlayerInstance = RPlayer;
            Debug.Log("ClientCode: " + PlayerStats.NetworkPlayerInstance);
        }
        catch { Debug.Log("ERROR WHILE SETTING DATA"); }
    } 

    public bool SendPlayerStats(NetworkPlayer PlayerRef)
    {
        String TempName = Name;
        String TempIP = Network.player.ipAddress.ToString();
        String[] Data = new string[2]{TempName, TempIP};
        BinaryFormatter BinForm = new BinaryFormatter();
        MemoryStream MemStrm = new MemoryStream();
        BinForm.Serialize(MemStrm, Data);
        byte[] SerializedData = MemStrm.ToArray();
        MemStrm.Close();
        this.GetComponent<NetworkView>().RPC("SetPlayerStats", RPCMode.Server, SerializedData, PlayerRef);
        return true;
    }
    public void SendPlayerStatsToClient(NetworkPlayer SPlayerRef, string SName, string SIP, int SKills, int SDeaths, int SStability, int SAdditionalStability)
    {
        Debug.Log(SPlayerRef + "-" + SName + "-" + SIP + "-" + SKills + "-" + SDeaths + "-" + SStability + "-" + SAdditionalStability);
        this.GetComponent<NetworkView>().RPC("SetPlayerStatsOnClient", SPlayerRef, SPlayerRef,SName,SKills,SDeaths,SIP,SStability, SAdditionalStability);
    }

    [RPC]
    public void FetchPlayerStats(NetworkPlayer NP,int Damage, NetworkPlayer Killer, string WeaponRef, NetworkMessageInfo Info)
    {
        Player FetchedPlayer = new Player();
        foreach(Player P in Players)
        {
            if(P.NetworkPlayerInstance == NP)
            {
                Debug.Log("RUN");
                FetchedPlayer.Name = P.Name;
                FetchedPlayer.Stability = P.Stability;
                FetchedPlayer.AdditionalStability = P.AdditionalStability;
                FetchedPlayer.Kills = P.Kills;
                FetchedPlayer.Deaths = P.Deaths;
                FetchedPlayer.Class = P.Class;
                FetchedPlayer.CurrentWeapon = P.CurrentWeapon;
                FetchedPlayer.IP = P.IP;
                Debug.Log(P.Name);
                Debug.Log(P.Stability);
                Debug.Log(P.AdditionalStability);
                Debug.Log(P.Kills);
                Debug.Log(P.Deaths);
                Debug.Log(P.Class);
                Debug.Log(P.CurrentWeapon);
                Debug.Log(P.IP);

                String[] Data = new string[7] { FetchedPlayer.Name.ToString(), FetchedPlayer.Stability.ToString(), FetchedPlayer.AdditionalStability.ToString(), FetchedPlayer.Kills.ToString(), FetchedPlayer.Deaths.ToString(), FetchedPlayer.CurrentWeapon, FetchedPlayer.IP };
                BinaryFormatter BinForm = new BinaryFormatter();
                MemoryStream MemStrm = new MemoryStream();
                BinForm.Serialize(MemStrm, Data);
                byte[] SerializedData = MemStrm.ToArray();
                MemStrm.Close();

                Info.networkView.RPC("InterpretData", NP, SerializedData, NP, Damage, Killer, WeaponRef);

            }
            Debug.Log("TIK");
        }
        

    }



    //Reguliere methods
    public void Update()
    {
        if (SyncPillarModel)
        {
            PillarModel.transform.position = new Vector3(FPSPos.position.x, FPSPos.position.y, FPSPos.position.z);
            PillarModel.transform.rotation = FPSPos.transform.rotation;
            
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Chat = !Chat;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log(PlayerStats.Name);
        }
    }
    private void OnGUI()
    {
        if (Errormessage == true)
        {
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "You have lost connection to the server...");
            if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 50, 100, 100), "Quit"))
            {
                Application.Quit();
            }
        }


        if (MainMenu == true)
        {
            Name = GUI.TextField(new Rect(10, 150, 150, 150), Name);
            GUI.Label(new Rect(10, 10, 150, 50), "Not connected to server");
            if (GUI.Button(new Rect(10, 50, 150, 50), "Connect to server"))
            {
                ConnectToServer();
            }
            if (GUI.Button(new Rect(10, 100, 150, 50), "Start server"))
            {
                StartServer();
            }
        }

        if (Network.isClient)
        {
            MainMenu = false;

            if (Network.peerType == NetworkPeerType.Connecting)
            {
                GUI.Label(new Rect(10, 10, 100, 100), "Connecting to server...");
            }
            else
            {
                if (GUI.Button(new Rect(10, 70, 100, 100), "Disconnect"))
                {
                    DisconnectClient(Network.player);
                }
            }
        }
        if (Network.isServer)
        {
            MainMenu = false;
            if (GUI.Button(new Rect(10, 10, 100, 100), "Stop server"))
            {
                StopServer();
            }
            GUI.Label(new Rect(Screen.width - 300, 0, 100, 300), "Server IP: " + Network.player.ipAddress);
            GUI.Label(new Rect(Screen.width - 300, 50, 100, 300), "Server Port: " + Network.player.port);
        }

        //CHATBOX
        if (Chat && (Network.isClient || Network.isServer))
        {
            GUI.skin = ChatBox;
            GUI.Box(new Rect(10,Screen.height - 100, 400,100), "");
            CurrentMessage = GUI.TextField(new Rect(15, Screen.height-25,290,20), CurrentMessage);
            if (GUI.Button(new Rect(310, Screen.height-25,95,20), "Send"))
            {
                if (!String.IsNullOrEmpty(CurrentMessage.Trim()))
                {
                    string Mess = CurrentMessage;
                    CurrentMessage = PlayerStats.Name + ": " + Mess;
                    N.RPC("ChatMessage", RPCMode.All, CurrentMessage);
                    CurrentMessage = string.Empty;
                }
            }
            ScrollVector1 = GUI.BeginScrollView(new Rect(10, Screen.height - 90, 395, 60), ScrollVector1, new Rect(10, 10, ChatHistory.Count * 20, 400));
            int i = 1;
            foreach (string C in ChatHistory)
            {
                    GUI.Label(new Rect(20, i*20, 290, 20), C);
                    i++;
            }
            GUI.EndScrollView();
            GUI.skin = null;
        }
        //KILLFEED
        if ((Network.isClient || Network.isServer) && KillFeed.Count > 0)
        {
            GUI.skin = ChatBox;
            int j = 1;
            foreach(string Kill in KillFeed)
            {
                j =+1;
                GUI.Box(new Rect(Screen.width - 155, j*30,150,25),Kill);
            }
            GUI.skin = null;
        }
    }

    //Chatbox/killfeed
    public void ProcessKill(string Killer, string Victim, string KillWeapon, bool HatShot)
    {
        string Text = string.Empty;
        if (HatShot)
        {
            Text = "[HATSHOT!!!] " + Killer + " [" + KillWeapon + "] " + Victim;
        }
        else
        {
            Text = Killer + " [" + KillWeapon + "] " + Victim;
        }
        N.RPC("KillFeedMessage",RPCMode.All, Text);
    }
    public IEnumerator WaitForStopShow(string Text)
    {
        if (KillFeed.Count <= 2)
        {
            foreach (string Message in KillFeed)
            {
                if (Message == Text)
                {
                    yield return new WaitForSeconds(1);
                    KillFeed.Remove(Message);
                    N.RPC("KillMessage", RPCMode.All, Text);
                }
            }
        }
        else
        {
            KillFeed.RemoveAt(0);
            KillFeed.Add(Text);
            yield return new WaitForSeconds(1);
            KillFeed.Remove(Text);
            N.RPC("KillMessage", RPCMode.All, Text);
        }
    }
    [RPC]
    public void ChatMessage(string Message)
    {
        ChatHistory.Add(Message);
    }
    [RPC]
    public void KillMessage(string Message)
    {
        KillHistory.Add(Message);
    }
    [RPC]
    public void KillFeedMessage(string Message)
    {
        KillFeed.Add(Message);
        StartCoroutine(WaitForStopShow(Message));
    }

    //Client methods
    public void ConnectToServer()
    {
        Network.Connect(ConnectionIP, PortNumber);
    }
    public void OnConnectedToServer()
    {
        Debug.Log("Connection succeeded with: " + ConnectionIP + PortNumber);
        isConnected = true;
        isIngame = true;
        LoadPlayLevel();
    }
    public void OnFailedToConnect()
    {
        Debug.Log("Error while connecting!");
    }
    public void DisconnectClient(NetworkPlayer player)
    {
        Errormessage = true;
    }
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        if (Network.isClient)
        {
            Errormessage = true;
        }
    }

    //Als het level geladen is...
    public void OnLevelWasLoaded(int Level){
        if (Network.isClient && Level == 1)
        {
                RefObject = GameObject.Find("/Speler/FPSController");
                if (RefObject != null)
                {
                    PillarModel = PillarToSpawn;
                    PillarModel = Network.Instantiate(PillarModel, Vector3.zero, Quaternion.identity, 0) as GameObject;
                    FPSPos = RefObject.transform.Find("Pillar").transform;
                    SyncPillarModel = true;
                    SendPlayerStats(Network.player);
                    N.RPC("SetPillarOwners", RPCMode.Server, Network.player);
                }
                else
                {
                    Debug.Log("Desastreuze error! -> RefObject = null");
                }
        }
    }

    //PillarSpawnRPC
    [RPC]
    public void SetPillarOwners(NetworkPlayer NP)
    {
        foreach (Player P in Players)
        { 
            if(P.NetworkPlayerInstance == NP)
            {   
                foreach(GameObject gameObj in GameObject.FindObjectsOfType<GameObject>()){
                    if(gameObj.name == "Pillar(clone)")
                    {
                        if(gameObj.GetComponent<NetworkView>().owner == NP)
                        {
                            gameObj.GetComponent<NetworkView>().RPC("SetOwner", RPCMode.AllBuffered, NP);
                            Debug.Log("Succes");
                        }
                    }
                }

            }
        }
    }




    //Server methods
    public void StartServer()
    {
        Network.InitializeServer(MaxClients, PortNumber, true);
        Debug.Log("--Server started---");
        Debug.Log("IP Address: " + ConnectionIP);
        Debug.Log("Port Number: " + PortNumber);
        PlayerStats = new Player();
        PlayerStats.Name = "SERVER";
        LoadPlayLevel();
    }
    public void StopServer()
    {
        Network.Disconnect();
        Debug.Log("--Server stopped--");
        Application.LoadLevel(0);
        Destroy(gameObject);
    }
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Running cleanup method....");
        List<Player> TempList = new List<Player>();
        foreach (Player TempPlayer in Players)
        {
            if (TempPlayer.NetworkPlayerInstance == player)
            {
                Debug.Log(TempPlayer.Name + " disconnected from server. Cleaning up his mess...");
                Network.RemoveRPCs(TempPlayer.NetworkPlayerInstance);
                Network.DestroyPlayerObjects(TempPlayer.NetworkPlayerInstance);
                TempList.Add(TempPlayer);
            }
        }    
        foreach (Player ToBeRemoved in TempList)
        {
            Players.Remove(ToBeRemoved);
        }
    }

    //Server And Client Methods
    public bool LoadPlayLevel()
    {
        Application.LoadLevel(1);
        if (Network.isClient)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}