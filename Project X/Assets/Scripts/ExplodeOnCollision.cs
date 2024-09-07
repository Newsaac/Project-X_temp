using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeOnCollision : MonoBehaviour
{
    private GameObject explosion;
    private AudioSource explosionSound;
    private GameManager gameManager;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionDuration;

    private void Awake() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        explosionSound = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other) {
        explosionSound.volume = gameManager.settings.effectsVolume;
        explosionSound.Play();

        explosion = Instantiate(explosionPrefab, transform);
        Vector3 scale = explosion.transform.localScale;
        explosion.transform.SetParent(null);
        explosion.transform.localScale = scale;

        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<SphereCollider>().enabled = false;
        Invoke(nameof(End), explosionDuration);
    }

    private void End() {
        Destroy(explosion);
        Destroy(gameObject);
    }
}
