using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingStatusBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] Vector3 offset;

    Camera playerCam;
    Transform parentTf;
    
    private void Awake() {
        playerCam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        parentTf = transform.parent.parent.transform;
    }

    private void Update() {
        transform.parent.rotation = playerCam.transform.rotation;
        transform.position = parentTf.position + offset;
    }

    public void UpdateValue(int value, int maxValue) {
        slider.value = ((float)value) / maxValue;
    }
    public void UpdateValue(float value, float maxValue) {
        slider.value = value / maxValue;
    }
}
