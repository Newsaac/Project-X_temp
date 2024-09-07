using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] WeaponSpecs weaponSpecs;
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform gunEnd;
    [SerializeField] Animator playerAnimator;
    [SerializeField] Animator pistolAnimator;
    [SerializeField] GameObject muzzleFlash;
    [SerializeField] ReloadSlider reloadLine;

    [Header("Audio")]
    [SerializeField] AudioSource shotAudio;
    [SerializeField] AudioSource reloadAudio;
    [SerializeField] AudioSource punchAudio;
    [SerializeField] AudioSource punchAirAudio;

    private bool isRecoiling = false;
    private float timer = 0;
    private Vector3 jointOriginalPos;
    [Header("Recoil")]
    [SerializeField] float recoilSpeed = 10f;
    [SerializeField] Vector3 recoilAmount = new Vector3(.15f, .2f, 0f);
    [SerializeField] Transform joint;

    private bool isPunching;
    [Header("Punch")]
    [SerializeField] float actionLockDuration = 0.5f;
    [SerializeField] float punchForce = 1000f;
    [SerializeField] Transform punchPoint;


    private WaitForSeconds shotDuration = new WaitForSeconds(0.1f);
    private float nextFire;

    private bool isReloading = false;

    private GameManager gameManager;
    private GameSettings controls;

    private void Awake() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        controls = gameManager.settings;

        jointOriginalPos = joint.localPosition;
    }

    void Start() {
        gameManager.ammoCnt = weaponSpecs.magazine;
    }


    void Update() {
        if (gameManager.gameOver)
            return;
        bool isFiring = weaponSpecs.isAutomatic ? Input.GetKey(controls.shoot) : Input.GetKeyDown(controls.shoot);
        if (isFiring && Time.time > nextFire && gameManager.ammoCnt > 0 && !isReloading && !isPunching) {
            nextFire = Time.time + weaponSpecs.fireRate;
            gameManager.ammoCnt--;

            playerAnimator.SetTrigger("fire");
            pistolAnimator.SetTrigger("fire");
            timer = 0;
            isRecoiling = true;

            StartCoroutine(ShotEffect());

            Vector3 rayOrigin = playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
            RaycastHit hit;

            if (Physics.Raycast(rayOrigin, playerCamera.transform.forward, out hit, weaponSpecs.weaponRange)) {

                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null) {
                    enemy.TakeDamage(weaponSpecs.gunDamage);
                }

                if (hit.rigidbody != null) {
                    hit.rigidbody.AddForce(-hit.normal * weaponSpecs.hitForce);
                }
            }
        }
        Recoil();

        if(Input.GetKeyDown(controls.reload)) {
            if (gameManager.ammoCnt < weaponSpecs.magazine && !isReloading)
                StartCoroutine(Reload());
        }

        if (Input.GetKeyDown(controls.punch)) {
            if (!isReloading)
                StartCoroutine(Punch());
        }
    }

    private IEnumerator Punch() {
        playerAnimator.SetTrigger("punch");
        isPunching = true;

        Collider[] colliders = Physics.OverlapSphere(punchPoint.position, 1.5f);

        yield return new WaitForSeconds(actionLockDuration * 0.5f);

        int hitCnt = 0;
        foreach (Collider collider in colliders) {
            if (collider.gameObject.CompareTag("Wall") || collider.gameObject.CompareTag("Enemy")) {
                hitCnt++;
                Vector3 direction = punchPoint.TransformDirection(Vector3.forward);
                try { collider.attachedRigidbody.AddForce(direction * punchForce, ForceMode.Impulse); }
                catch { }
            }
        }
        if (hitCnt > 0) {
            punchAudio.volume = gameManager.settings.effectsVolume;
            punchAudio.Play();
        }
        else {
            punchAirAudio.volume = gameManager.settings.effectsVolume;
            punchAirAudio.Play();

        }

        yield return new WaitForSeconds(actionLockDuration * 0.5f);

        isPunching = false;
    }

    private IEnumerator ShotEffect() {

        shotAudio.volume = gameManager.settings.effectsVolume;
        shotAudio.Play();

        GameObject flash = Instantiate(muzzleFlash, gunEnd);
        yield return shotDuration;
        Destroy(flash);
    }

    private IEnumerator Reload() {
        playerAnimator.SetTrigger("reload");
        pistolAnimator.SetTrigger("reload");
        reloadAudio.volume = gameManager.settings.effectsVolume;
        reloadAudio.Play();
        isReloading = true;
        reloadLine.StartReloadAnimation(weaponSpecs.reloadDuration);

        yield return new WaitForSeconds(weaponSpecs.reloadDuration);

        isReloading = false;
        gameManager.ammoCnt = weaponSpecs.magazine;
    }

    private void Recoil() {
        if (isRecoiling) {
            timer += Time.deltaTime * recoilSpeed;
            Vector3 distance = joint.localPosition - jointOriginalPos;
            if (timer > 0.1f && distance.y <= 0) {
                isRecoiling = false;
                return;
            }
            joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * recoilAmount.x, jointOriginalPos.y + Mathf.Sin(timer) * recoilAmount.y, jointOriginalPos.z + Mathf.Sin(timer) * recoilAmount.z);
        }
        else {
            timer = 0;
            joint.localPosition = new Vector3(Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * recoilSpeed), Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * recoilSpeed), Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * recoilSpeed));
        }
    }
}
