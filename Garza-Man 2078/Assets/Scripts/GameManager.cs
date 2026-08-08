using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.IO;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Game Over Video")]
    public VideoPlayer gameOverVideoPlayer;
    public string gameOverVideoFileName = "gameOver.mp4";
    public GameObject gameOverVideoScreen;

    [Header("Victory Video")]
    public VideoPlayer victoryVideoPlayer;
    public string victoryVideoFileName = "victory.mp4";
    public GameObject victoryVideoScreen;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        if (gameOverVideoScreen != null)
        {
            gameOverVideoScreen.SetActive(false);
        }

        if (victoryVideoScreen != null)
        {
            victoryVideoScreen.SetActive(false);
        }

        if (gameOverVideoPlayer != null)
        {
            gameOverVideoPlayer.Stop();
            gameOverVideoPlayer.playOnAwake = false;
            gameOverVideoPlayer.loopPointReached += OnGameOverVideoFinished;
        }

        if (victoryVideoPlayer != null)
        {
            victoryVideoPlayer.Stop();
            victoryVideoPlayer.playOnAwake = false;
            victoryVideoPlayer.loopPointReached += OnVictoryVideoFinished;
        }

        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void TriggerGameOver()
    {
        Debug.Log("Game Over Triggered");

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameOverVideoScreen != null) gameOverVideoScreen.SetActive(true);

        if (gameOverVideoPlayer != null)
        {
            gameOverVideoPlayer.url = Path.Combine(Application.streamingAssetsPath, gameOverVideoFileName);
            gameOverVideoPlayer.Play();
        }

        EndGame();
    }

    public void TriggerVictory()
    {
        Debug.Log("Victory Triggered");

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (victoryVideoScreen != null) victoryVideoScreen.SetActive(true);

        if (victoryVideoPlayer != null)
        {
            victoryVideoPlayer.url = Path.Combine(Application.streamingAssetsPath, victoryVideoFileName);
            victoryVideoPlayer.Play();
        }

        EndGame();
    }

    private IEnumerator ReturnToMenuAfterSeconds(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        QuitToMenu();
    }

    private void EndGame()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnGameOverVideoFinished(VideoPlayer videoPlayer)
    {
        QuitToMenu();
    }

    private void OnVictoryVideoFinished(VideoPlayer videoPlayer)
    {
        QuitToMenu();
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1;

        if (gameOverVideoPlayer != null)
        {
            gameOverVideoPlayer.Stop();
        }

        if (victoryVideoPlayer != null)
        {
            victoryVideoPlayer.Stop();
        }

        if (gameOverVideoScreen != null)
        {
            gameOverVideoScreen.SetActive(false);
        }

        if (victoryVideoScreen != null)
        {
            victoryVideoScreen.SetActive(false);
        }

        SceneManager.LoadScene("Menu");
    }
}