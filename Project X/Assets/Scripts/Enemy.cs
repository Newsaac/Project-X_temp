using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    private bool isDead = false;

    [SerializeField] protected EnemyStats stats;

    protected int hp;
    protected FloatingStatusBar healthBar;
    protected GameManager gameManager;

    protected Rigidbody rb;
    protected Vector3 moveDirection = Vector3.zero;
    protected Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, 0));

    protected void Awake() {
        rotation = transform.rotation;

        rb = GetComponent<Rigidbody>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        healthBar = GetComponentInChildren<FloatingStatusBar>();
    }

    protected void Start() {
        gameManager.enemiesLeft++;
        hp = stats.maxHp;
        healthBar.UpdateValue(hp, stats.maxHp);
    }

    protected virtual void FixedUpdate() {
        if (rb != null) {
            rb.MoveRotation(rotation);
            rb.AddForce(moveDirection.normalized * stats.speed);
        }
    }

    protected abstract void Attack();
    protected abstract void Idle();
    protected abstract void TrackPlayer();

    protected virtual void OnDeath() {
        gameManager.EnemyKilled();
        Destroy(this.gameObject);
    }
    public virtual void TakeDamage(int value) {
        hp -= value;
        healthBar.UpdateValue(hp, stats.maxHp);
        if (hp <= 0) {
            if (!isDead) {
                isDead = true;
                OnDeath();
            }
        }
    }

    public virtual void Die() { Destroy(this.gameObject); }
}
