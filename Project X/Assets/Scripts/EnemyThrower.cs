using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyThrower : Enemy
{
    private Transform playerTf;
    [SerializeField] Animator anim;

    private bool isRotating = false;
    private float rotationProgress = -1;
    private bool isRotateOnCooldown = false;
    private Quaternion targetRotation;
    private Quaternion initialRotation;
    [Header("Idle Rotation")]
    [SerializeField] float idleRotatePause = 0.7f;
    [SerializeField] float idleRotateSpeed = 2f;

    private GameObject throwable;
    private bool isAttacking;
    private Vector3 throwPoint;
    [Header("Throw Attack")]
    [SerializeField] Transform throwableSpawnPoint;
    [SerializeField] GameObject throwablePrefab;
    [SerializeField] float throwPreparation = 0.3f;
    [SerializeField] float maxVelocity = 30f;

    private new void Awake() {
        base.Awake();
        playerTf = GameObject.Find("Player").GetComponent<Transform>();
    }

    private new void Start() {
        base.Start();
    }

    void Update() {
        if (!gameManager.gameOver) {
            float distance = (playerTf.position - transform.position).magnitude;

            if (distance > stats.detectRange) {
                Idle();
            }
            else if (distance <= stats.detectRange && distance > stats.attackRange) {
                anim.SetBool("isIdle", false);
                anim.SetBool("isThrowing", false);
                TrackPlayer();
            }
            else {
                if(hp > 0) 
                    Attack();
            }
        }
    }

    protected override void Attack() {
        anim.SetBool("isThrowing", true);

        TrackPlayer();
        moveDirection = Vector3.zero;

        if (!isAttacking) {
            isAttacking = true;
            throwPoint = playerTf.position;
            Invoke(nameof(Throw), throwPreparation);
        }
    }
    private void Throw() {

        throwable = Instantiate(throwablePrefab, throwableSpawnPoint);
        throwable.transform.SetParent(null);
        throwable.transform.rotation = Quaternion.Euler(0, 0, 0);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();

        Vector3 toTarget = throwPoint - throwableSpawnPoint.position;

        float gSquared = Physics.gravity.sqrMagnitude;
        float b = maxVelocity * maxVelocity + Vector3.Dot(toTarget, Physics.gravity);
        float discriminant = b * b - gSquared * toTarget.sqrMagnitude;

        // Check whether the target is reachable at max speed or less.
        if (discriminant < 0) {
            
        }

        float discRoot = Mathf.Sqrt(discriminant);

        // Highest shot with the given max speed:
        float T_max = Mathf.Sqrt((b + discRoot) * 2f / gSquared);

        // Most direct shot with the given max speed:
        float T_min = Mathf.Sqrt((b - discRoot) * 2f / gSquared);

        // Lowest-speed arc available:
        float T_lowEnergy = Mathf.Sqrt(Mathf.Sqrt(toTarget.sqrMagnitude * 4f / gSquared));

        float T = T_lowEnergy;

        Vector3 velocity = toTarget / T - Physics.gravity * T / 2f;

        rb.AddForce(velocity, ForceMode.VelocityChange);

        StartCoroutine(nameof(AttackCooldown));
    }

    private IEnumerator AttackCooldown() {
        yield return new WaitForSeconds(stats.attackCooldown);
        isAttacking = false;
    }

    protected override void Idle() {
        anim.SetBool("isIdle", true);
        anim.SetBool("isThrowing", false);
        moveDirection = Vector3.zero;
        if (!isRotateOnCooldown) {
            if (isRotating) {
                if (rotationProgress < 1 && rotationProgress >= 0) {
                    rotationProgress += Time.deltaTime * idleRotateSpeed;
                    rotation = Quaternion.Lerp(initialRotation, targetRotation, rotationProgress);
                }
                else {
                    isRotating = false;
                    rotationProgress = -1;
                    StartCoroutine(nameof(IdleRotateCooldown));
                }
            }
            else {
                initialRotation = transform.rotation;
                targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, UnityEngine.Random.Range(-180f, 180f), transform.rotation.eulerAngles.z);
                anim.SetFloat("turnValue", targetRotation.eulerAngles.y - initialRotation.eulerAngles.y);
                isRotating = true;
                rotationProgress = 0;
            }
        }
    }

    private IEnumerator IdleRotateCooldown() {
        if (!isRotateOnCooldown) {
            anim.SetFloat("turnValue", 0);
            anim.SetTrigger("stopTurning");
        }
        isRotateOnCooldown = true;

        yield return new WaitForSeconds(idleRotatePause);

        isRotateOnCooldown = false;
    }

    protected override void TrackPlayer() {
        moveDirection = playerTf.position - transform.position;
        moveDirection.y = 0;

        Vector3 lookDirection = moveDirection;
        lookDirection = new Vector3(lookDirection.x, 0, lookDirection.z);
        rotation = Quaternion.LookRotation(lookDirection);
    }

    protected override void OnDeath() {
        moveDirection = Vector3.zero;

        healthBar.gameObject.SetActive(false);
        Destroy(gameObject.GetComponent<Rigidbody>());
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        anim.SetTrigger("death");
        gameManager.EnemyKilled();
        Invoke(nameof(Die), gameManager.settings.deleteCorpsesIn);
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Player")) {
            anim.SetBool("isPunching", true);
            InvokeRepeating(nameof(PerformCollide), stats.collideDmgCooldown, stats.collideDmgCooldown);
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.gameObject.CompareTag("Player")) {
            anim.SetBool("isPunching", false);
            CancelInvoke(nameof(PerformCollide));
        }
    }

    private void PerformCollide() {
        gameManager.DamagePlayer(stats.collideDamage);
    }
}
