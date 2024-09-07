using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class EnemyExplosion : Enemy
{

    private new void Awake() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private new void Start() {
        
    }

    protected override void Idle() {
    }
    protected override void TrackPlayer() {    }
    protected override void Attack() {
    }
    public override void TakeDamage(int value) {
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.name == "Player") {
            gameManager.DamagePlayer(stats.collideDamage);
        }
    }
}
