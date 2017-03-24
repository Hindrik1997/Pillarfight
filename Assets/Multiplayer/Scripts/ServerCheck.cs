using UnityEngine;
using System.Collections;

public class ServerCheck : MonoBehaviour {
    public Camera ServerCam;
    public GameObject Speler;
	void Start () {
        if (Network.isServer) {
            Destroy(Speler);
            ServerCam.gameObject.SetActive(true);
        }
	}

}
