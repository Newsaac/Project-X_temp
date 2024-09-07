using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlyer : Enemy
{
    private Transform playerTf;
    private AudioSource audioSource;
    [SerializeField] Animator anim;


    private float initialAltitude;
    private bool isIdle = false;
    private int floatDirection = 1;
    private float floatProgress = -1;
    private bool floatPause = false;
    [Header("Idle")]
    [SerializeField] float floatSpeed = 5f;
    [SerializeField] float floatTime = 1f;
    [SerializeField] float floatPeakPause = 0.25f;

    private GameObject explosion;
    private bool explosionOnCooldown = false;
    [Header("Explosion")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionDuration = 0.25f;
    [SerializeField] float explosionDelay = 0.3f;

    private new void Awake() {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        playerTf = GameObject.Find("Player").GetComponent<Transform>();
    }

    private new void Start() {
        base.Start();
    }

    void Update() {
        if (!gameManager.gameOver && hp > 0) {
            float distance = (playerTf.position - transform.position).magnitude;

            if (distance > stats.detectRange)
                Idle();
            else if (distance <= stats.detectRange && distance > stats.attackRange)
                TrackPlayer();
            else {
                isIdle = false;
                if(!explosionOnCooldown)
                    Attack();
            }
        }
    }

    #region Attack
    protected override void Attack() {
        moveDirection = Vector3.zero;
        anim.SetTrigger("Attack");
        explosionOnCooldown = true;
        Invoke(nameof(CreateExplosion), explosionDelay);
    }

    private void CreateExplosion() {
        audioSource.volume = gameManager.settings.effectsVolume;
        audioSource.Play();
        explosion = Instantiate(explosionPrefab, transform);
        Invoke(nameof(EndExplosion), explosionDuration);
    }
    private void EndExplosion() {
        Destroy(explosion);
        StartCoroutine(nameof(ExplosionCooldown));
    }
    private IEnumerator ExplosionCooldown() {
        explosionOnCooldown = true;
        yield return new WaitForSeconds(stats.attackCooldown);
        explosionOnCooldown = false;
    }
    #endregion

    protected override void TrackPlayer() {
        isIdle = false;
        moveDirection = playerTf.position - transform.position;

        Vector3 lookDirection = moveDirection;
        lookDirection = new Vector3(lookDirection.x, 0, lookDirection.z);
        rotation = Quaternion.LookRotation(lookDirection);
    }

    #region Idle
    protected override void Idle() {
        if (!isIdle) {
            moveDirection = Vector3.zero;
            initialAltitude = transform.position.y;
            floatProgress = 0f;
            isIdle = true;
        }
        if (!floatPause) {
            if (floatProgress >= 0 && floatProgress <= floatTime) {
                floatProgress += Time.deltaTime;
                moveDirection = floatDirection * floatSpeed * Vector3.up;
            }
            else {
                floatProgress = 0;
                floatDirection *= -1;
                StartCoroutine(nameof(FloatPause));
            }
        }
    }
    private IEnumerator FloatPause() {
        floatPause = true;
        moveDirection = Vector3.zero;
        yield return new WaitForSeconds(floatPeakPause);
        floatPause = false;
    }
    #endregion

    #region Collisions
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Player")) {
            InvokeRepeating(nameof(CollisionAttack), stats.collideDmgCooldown, stats.collideDmgCooldown);
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.gameObject.CompareTag("Player")) {
            CancelInvoke(nameof(CollisionAttack));
        }
    }

    private void CollisionAttack() {
        gameManager.DamagePlayer(stats.collideDamage);
    }
    #endregion

    protected override void OnDeath() {
        moveDirection = Vector3.zero;
        healthBar.gameObject.SetActive(false);
//        if(!explosionOnCooldown)
        Invoke(nameof(Die), explosionDelay + explosionDuration + 0.05f);
        Attack();
    }

    public override void Die() {
        gameManager.EnemyKilled();
        base.Die();
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, stats.attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.detectRange);
    }
}
