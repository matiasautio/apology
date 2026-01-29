using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Castle : MonoBehaviour {
public GameObject Player;
public GameObject Player2;

    void OnTriggerEnter2D (Collider2D other) {

        if (other.tag == "Player") {

            StartCoroutine(WinWait());
            Player.GetComponent<Animator>().SetBool("isWin", true);
            Player2.GetComponent<Animator>().SetBool("isWin", true);
        }
    }

    IEnumerator WinWait() {

        yield return new WaitForSeconds (3);
        Application.LoadLevel("Win");
    }
}
