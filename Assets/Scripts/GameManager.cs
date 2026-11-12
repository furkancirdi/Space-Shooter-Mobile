using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Oyun sahnesindeki skor, rekor ve oyun sonu akisini yonetir.
public class GameManager : MonoBehaviour
{
    public const string HighScoreKey = "HighScore";
    public const string MenuSceneName = "MainMenu";

    public static GameManager Instance { get; private set; }

    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text newHighScoreText;

    private int score;
    private bool isGameOver;

    public bool IsGameOver => isGameOver;

    public static int HighScore => PlayerPrefs.GetInt(HighScoreKey, 0);

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        RefreshUI();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        bool isNewHighScore = score > HighScore;
        if (isNewHighScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, score);
            PlayerPrefs.Save();
        }

        RefreshUI();

        if (finalScoreText != null) finalScoreText.text = "SCORE: " + score;
        if (newHighScoreText != null) newHighScoreText.gameObject.SetActive(isNewHighScore);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // UI butonlari icin
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MenuSceneName);
    }

    private void RefreshUI()
    {
        if (scoreText != null) scoreText.text = "SCORE: " + score;
        if (highScoreText != null) highScoreText.text = "BEST: " + Mathf.Max(HighScore, score);
    }
}
