using UnityEngine;
using System.Collections;

public class DestroyBullet : MonoBehaviour {

    IEnumerator Start()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }

    IEnumerator OnTriggerEnter()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

}
