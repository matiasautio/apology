using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Castle : MonoBehaviour
{
    [SerializeField] int neutralEndingThreshold = 5;
    [SerializeField] int goodEndingThreshold = 10;

    void OnTriggerEnter2D (Collider2D other)
    {

        if (other.tag == "Player") {

            // Check player mental state
            Player player = other.GetComponent<Player>();
            if (player.MentalState <= neutralEndingThreshold)
            {
                SceneManager.LoadScene("BadEnding");
            }
            else if (player.MentalState >= neutralEndingThreshold && player.MentalState < goodEndingThreshold)
            {
                SceneManager.LoadScene("NeutralEnding");
            }
            else if (player.MentalState >= goodEndingThreshold)
            {
                SceneManager.LoadScene("GoodEnding");
            }
            //StartCoroutine(WinWait());
        }
    }

    IEnumerator WinWait() {

        yield return new WaitForSeconds (3);
        //Application.LoadLevel("Win");
    }
}
