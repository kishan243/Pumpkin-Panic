using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(AudioSource))] // Ensures we have an AudioSource
public class GameManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string winSceneName = "win";
    [SerializeField] private string loseSceneName = "lose";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ghostCountText;

    [Header("Audio")] // --- NEW SECTION ---
    [SerializeField] private AudioClip backgroundMusic;
    private AudioSource audioSource;
    // ---------------------------------

    private int activeGhostCount = 0;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        // --- NEW: Get the AudioSource component ---
        audioSource = GetComponent<AudioSource>();
        // ---------------------------------------
    }

    private void Start()
    {
        // --- NEW: Start the background music ---
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        // -------------------------------------
    }

    // --- Ghost Counting ---
    public void RegisterGhost()
    {
        activeGhostCount++;
        UpdateGhostUI();
    }

    public void OnGhostKilled()
    {
        activeGhostCount--;
        UpdateGhostUI();

        if (activeGhostCount <= 0)
        {
            TriggerWin();
        }
    }

    private void UpdateGhostUI()
    {
        if (ghostCountText != null)
        {
            ghostCountText.text = "Ghosts Left: " + activeGhostCount;
        }
    }

    // --- Win/Lose Logic ---
    public void TriggerWin()
    {
        Debug.Log("PLAYER WINS!");
        SceneManager.LoadScene(winSceneName);
    }

    public void TriggerLose()
    {
        Debug.Log("PLAYER LOSES!");
        SceneManager.LoadScene(loseSceneName);
    }
}