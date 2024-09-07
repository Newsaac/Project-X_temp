using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAmplifier : MonoBehaviour
{
    [SerializeField] float amplifier = 1f;

    private Rigidbody rb;
    void FixedUpdate() {
        rb.AddForce(Physics.gravity * GetComponent<Rigidbody>().mass * amplifier);
    }
}
