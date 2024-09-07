using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    private struct LevelSpecs {
        public readonly float bestTime;
        public LevelSpecs(float bestTime) : this() {
            this.bestTime = bestTime;
        }
    }

    [HideInInspector] public bool gameOver = true;
    [HideInInspector] public int ammoCnt = 0;
    [HideInInspector] public int enemiesLeft;
    float timer = 0f;

    [Header("Game Vars")]
    [SerializeField] int maxPlayerHp;
    [SerializeField] int playerHp;

    [Header("UI")]
    [SerializeField] Button startButton;
    [SerializeField] Button restartButton;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI bestTimeText;
    [SerializeField] TextMeshProUGUI enemiesLeftText;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] Slider hpSlider;
    [SerializeField] Image crosshair;
    [SerializeField] GameObject menuPanel;
    [SerializeField] Image damageTrim;
    [Space(5)]
    [SerializeField] float trimDuration = 0.5f;
    [SerializeField] float trimFadeInSpeed = 1f;
    [SerializeField] float trimFadeOutSpeed = 1f;
    //[SerializeField] TextMeshProUGUI controlGuideText;

    [Header("Audio")]
    [SerializeField] AudioSource bgMusicAudio;

    [Space(20)]
    public GameSettings settings;

    private LevelSpecs levelSpecs;

    private float startOpacity = 0f;
    private float fadeInProgress = -1;
    private float fadeOutProgress = -1;
    private bool isTrimOn = false;

    private void LoadLevelSpecs() {
        levelSpecs = new LevelSpecs(
            PlayerPrefs.HasKey("Best") ? PlayerPrefs.GetFloat("Best") : 0
        );
    }

    void Start() {
        playerHp = maxPlayerHp;
        hpSlider.value = ((float)playerHp) / maxPlayerHp;

        LoadLevelSpecs();

        gameOver = true;
        Time.timeScale = 0;
    }

    void Update() {
        if (!gameOver)
            timer += Time.deltaTime;
        ammoText.text = "Ammo: " + ammoCnt;
        timerText.text = "Time: " + timer.ToString("0.00");
    }

    public void StartGame() {
        bgMusicAudio.volume = settings.musicVolume;
        bgMusicAudio.Play();

        Time.timeScale = 1;

        timer = 0f;
        bestTimeText.text = "Best: " + (levelSpecs.bestTime == 0f ? "-" : levelSpecs.bestTime.ToString("0.00"));

        enemiesLeftText.text = "Enemies: " + enemiesLeft;

        gameOver = false;
        ToggleMenuState(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartGame() {
        gameOver = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GameOver() {
        bgMusicAudio.Stop();

        if ((timer < levelSpecs.bestTime || levelSpecs.bestTime == 0) && enemiesLeft == 0)
            PlayerPrefs.SetFloat("Best", timer);
        gameOver = true;
        crosshair.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        menuPanel.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }

    void ToggleMenuState(bool mode) {
        startButton.gameObject.SetActive(mode);
        menuPanel.gameObject.SetActive(mode);
        crosshair.gameObject.SetActive(!mode);
        //controlGuideText.gameObject.SetActive(mode);
    }

    public void EnemyKilled() {
        enemiesLeft--;
        enemiesLeftText.text = "Enemies: " + enemiesLeft;
        if (enemiesLeft == 0)
            GameOver();
    }

    public void DamagePlayer(int amount) {
        #region Trim Coroutine
        if (fadeInProgress == -1f) {
            fadeInProgress = 0f;
            isTrimOn = false;
            startOpacity = 0f;
            StartCoroutine(nameof(CreateTrim));
        }
        else if(fadeInProgress < 1 && fadeInProgress >= 0) {
            //Do nothing
        }
        else if(isTrimOn) {
            StopCoroutine(nameof(CreateTrim));
            fadeInProgress = 1f;
            StartCoroutine(nameof(CreateTrim));
        }
        else {
            StopCoroutine(nameof(CreateTrim));
            fadeInProgress = 0f;
            isTrimOn = false;
            startOpacity = damageTrim.color.a;
            StartCoroutine(nameof(CreateTrim));
        }
        #endregion

        playerHp -= amount;
        hpSlider.value = ((float)playerHp) / maxPlayerHp;
        if (playerHp <= 0)
            GameOver();
    }

    private IEnumerator CreateTrim() {
        fadeOutProgress = -1f;
        while (true) {
            if (fadeInProgress < 1 && fadeInProgress >= 0) {
                fadeInProgress += Time.deltaTime * trimFadeInSpeed;
                Color tmp = damageTrim.color;
                tmp.a = Mathf.Lerp(startOpacity, 1f, fadeInProgress);
                damageTrim.color = tmp;
                yield return new WaitForEndOfFrame();
            }
            else if (fadeInProgress >= 1) {
                isTrimOn = true;
                yield return new WaitForSeconds(trimDuration);
                isTrimOn = false;
                fadeOutProgress = 0f;
                fadeInProgress = -2f; //any negative that is not -1
            }
            else if (fadeOutProgress < 1f && fadeOutProgress >= 0f) {
                fadeOutProgress += Time.deltaTime * trimFadeOutSpeed;
                Color tmp = damageTrim.color;
                tmp.a = Mathf.Lerp(1f, 0f, fadeOutProgress);
                damageTrim.color = tmp;
                yield return new WaitForEndOfFrame();
            }
            else {
                fadeInProgress = -1f;
                fadeOutProgress = -1f;
                yield break;
            }
        }
    }

}
