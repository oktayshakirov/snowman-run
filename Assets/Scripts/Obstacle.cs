using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour {

    PlayerMovement playerMovement;

	private void Start () {
    playerMovement = GameObject.FindFirstObjectByType<PlayerMovement>();
	}

    private void OnCollisionEnter (Collision collision)
    {
        if (collision.gameObject.name == "Player") {
            playerMovement.Die();
        }
    }

    private void Update () {
	
	}
}