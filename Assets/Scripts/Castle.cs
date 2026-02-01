using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Castle : MonoBehaviour
{

    [SerializeField] int badEndingThreshold = 0;
    [SerializeField] int neutralEndingThreshold = 5;
    [SerializeField] int goodEndingThreshold = 10;

    void OnTriggerEnter2D (Collider2D other)
    {

        if (other.tag == "Player") {

            // Check player mental state
            Player player = other.GetComponent<Player>();
            if (player.MentalState <= neutralEndingThreshold)
            {
                Debug.Log("Bad ending!");
            }
            else if (player.MentalState >= neutralEndingThreshold && player.MentalState < goodEndingThreshold)
            {
                Debug.Log("Neutral ending!");
            }
            else if (player.MentalState >= goodEndingThreshold)
            {
                Debug.Log("Good ending!");
            }
            //StartCoroutine(WinWait());
        }
    }

    IEnumerator WinWait() {

        yield return new WaitForSeconds (3);
        //Application.LoadLevel("Win");
    }
}
