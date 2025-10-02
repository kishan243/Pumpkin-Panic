using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Transform player;
    public TextMeshProUGUI ghostsCapturedText;
    public GameObject winPanel;

    private int ghostsCaptured = 0;
    private int totalGhostsInScene;

    void Start()
    {
        totalGhostsInScene = GameObject.FindGameObjectsWithTag("Collectible").Length;
        winPanel.SetActive(false);
        Time.timeScale = 1f;
        UpdateGhostCounter();
    }

    void Update()
    {
        if (player.position.y < -10f)
        {
            RestartGame();
        }
    }

    public void OnGhostCaptured()
    {
        ghostsCaptured++;
        UpdateGhostCounter();

        if (ghostsCaptured >= totalGhostsInScene)
        {
            WinGame();
        }
    }

    void UpdateGhostCounter()
    {
        ghostsCapturedText.text = "GHOSTS CAPTURED: " + ghostsCaptured + " / " + totalGhostsInScene;
    }

    void WinGame()
    {
        winPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}