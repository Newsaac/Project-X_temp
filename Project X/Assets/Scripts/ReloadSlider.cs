using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ReloadSlider : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    private float _timer;

    private void Start() {
        _slider.gameObject.SetActive(false);
        _slider.value = 0;
    }

    public void StartReloadAnimation(float reloadDuration) {
        _slider.gameObject.SetActive(true);
        StartCoroutine(AnimationReload(reloadDuration));
    }
    private IEnumerator AnimationReload(float reloadDuration) {
        for (_timer = 0f; _timer <= reloadDuration; _timer += Time.deltaTime) {
            yield return new WaitForEndOfFrame();
            _slider.value = (_timer / reloadDuration);
        }
        _slider.value = 0f;
        _slider.gameObject.SetActive(false);
    }
}