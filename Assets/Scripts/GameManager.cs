using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Oyun sahnesindeki skor, rekor, zorluk ve oyun sonu akisini yonetir.
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
    private int coinsThisLevel;
    private float elapsed;
    private bool isGameOver;
    private bool levelCleared;
    private bool playerDead;
    private Text gameOverInfoText;

    // Oyun durdu mu (oyuncu oldu ya da bolum bitti)
    public bool IsGameOver => isGameOver;
    public bool IsLevelCleared => levelCleared;
    public float Elapsed => elapsed;
    public int Score => score;
    public Hud Hud { get; private set; }

    public static int HighScore => Progress.HighScore;

    private static int Level => WaveDirector.Instance != null ? WaveDirector.Instance.Level : Progress.Level;

    // Dusmanlarin gelme sikligi. Bolume gore belirlenir (Levels.cs), bolum icinde de
    // dalga asamasi ilerledikce yuzde 85'ten yuzde 115'e cikar.
    public static float Difficulty =>
        Levels.SpawnRate(Level) * Mathf.Lerp(0.85f, 1.15f, WaveDirector.Instance != null ? WaveDirector.Instance.WaveProgress : 0f);

    // Dusman, mermi ve arka plan hiz carpani. Bolume gore belirlenir.
    public static float SpeedMultiplier => Levels.Speed(Level);

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        score = Progress.RunScore;

        // Can, HUD ve asama yoneticisi kodla eklenir; sahnede ek kurulum gerekmez
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && player.GetComponent<PlayerStatus>() == null)
            player.AddComponent<PlayerStatus>();

        if (scoreText != null)
            Hud = Hud.Create(scoreText.canvas.rootCanvas, gameOverPanel != null ? gameOverPanel.transform : null);

        if (GetComponent<WaveDirector>() == null) gameObject.AddComponent<WaveDirector>();
    }

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        RefreshUI();
        Sfx.PlayMusic();
    }

    private void Update()
    {
        if (isGameOver) return;
        elapsed += Time.deltaTime;
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

    // Bir dusman olduruldu. chainDepth 0 = lazerle, 1+ = baska bir patlamanin parcasiyla.
    // Zincirin her halkasi puani bir kat daha artirir.
    public void RegisterKill(int baseScore, int chainDepth, Vector3 position)
    {
        int multiplier = chainDepth + 1;
        AddScore(baseScore * multiplier);

        GameAssets a = GameAssets.I;
        Sfx.Play(a.explosion, 0.5f, Random.Range(0.9f, 1.1f));

        if (chainDepth >= 1)
        {
            Sfx.Play(a.chain, 0.7f, Mathf.Min(2.2f, 1f + 0.15f * (chainDepth - 1)));
            ScreenShake.Shake(Mathf.Min(0.22f, 0.05f + 0.035f * chainDepth), 0.18f);
            if (Hud != null)
            {
                Color color = Color.Lerp(new Color(1f, 0.9f, 0.3f), new Color(1f, 0.35f, 0.2f), Mathf.Clamp01((chainDepth - 1) / 4f));
                Hud.Popup("CHAIN x" + multiplier, position, color, Mathf.Min(64, 36 + 6 * chainDepth));
            }
        }
        else
        {
            ScreenShake.Shake(0.04f, 0.1f);
        }
    }

    public void CoinCollected(int amount)
    {
        coinsThisLevel += amount;
        if (Hud != null) Hud.RefreshCoins();
    }

    // Boss yenildi: coinler toplansin diye kisa bir bekleme, sonra bolum sonu ekrani
    public void LevelCleared()
    {
        if (isGameOver || levelCleared || playerDead) return;
        levelCleared = true;

        Coin.AttractAll = true;
        if (PlayerStatus.Instance != null) PlayerStatus.Instance.MakeSafe();
        foreach (EnemyBullet bullet in FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None))
            Destroy(bullet.gameObject);

        // Ilerleme hemen kaydedilir; oyuncu bu arada uygulamayi kapatsa bile bolum gecilmis sayilir
        int level = Level;
        Progress.Level = level + 1;
        Progress.RunScore = score;
        SaveHighScore();
        Progress.Save();

        StartCoroutine(LevelCompleteAfter(2.8f, level));
    }

    private IEnumerator LevelCompleteAfter(float delay, int level)
    {
        yield return new WaitForSeconds(delay);

        Progress.RunScore = score;
        SaveHighScore();
        Progress.Save();
        RefreshUI();

        isGameOver = true;
        Sfx.Play(GameAssets.I.levelComplete, 0.8f);
        Sfx.DuckMusic(0.5f);
        if (Hud != null) Hud.ShowLevelComplete(level, score, coinsThisLevel, NextLevel, GoToMenu);
        Time.timeScale = 0f;
    }

    private bool SaveHighScore()
    {
        if (score <= Progress.HighScore) return false;
        Progress.HighScore = score;
        return true;
    }

    // Oyuncunun son cani gitti: patlama gorunsun diye kisa bir bekleme, sonra oyun sonu
    public void PlayerDied()
    {
        if (isGameOver || levelCleared) return;
        playerDead = true;
        StartCoroutine(GameOverAfter(0.9f));
    }

    private IEnumerator GameOverAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameOver();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        bool isNewHighScore = SaveHighScore();
        Progress.RunScore = 0;
        Progress.Save();

        RefreshUI();

        if (finalScoreText != null) finalScoreText.text = "SCORE: " + score;
        if (newHighScoreText != null) newHighScoreText.gameObject.SetActive(isNewHighScore);
        ShowGameOverInfo();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Sfx.Play(GameAssets.I.gameOver, 0.8f);
        Sfx.DuckMusic(0.35f);

        Time.timeScale = 0f;
    }

    // Oyun sonu panelinde hangi bolumde olundugunu ve bu denemede toplanan coini gosterir
    private void ShowGameOverInfo()
    {
        if (finalScoreText == null) return;
        if (gameOverInfoText == null)
        {
            gameOverInfoText = Instantiate(finalScoreText, finalScoreText.transform.parent);
            gameOverInfoText.name = "LevelInfoText";
            gameOverInfoText.fontSize = 34;
            gameOverInfoText.color = new Color(0.7f, 0.85f, 1f);
            // "NEW HIGH SCORE" yazisi ile Restart butonu arasindaki bosluga sigar
            gameOverInfoText.rectTransform.sizeDelta = new Vector2(800f, 40f);
            gameOverInfoText.rectTransform.anchoredPosition = new Vector2(0f, -464f);
        }
        gameOverInfoText.text = "LEVEL " + Level + "    +" + coinsThisLevel + " COINS";
    }

    // UI butonlari icin. Restart ayni bolumu bastan baslatir.
    public void Restart()
    {
        Progress.RunScore = 0;
        Sfx.Play(GameAssets.I.uiClick, 0.7f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Sfx.Play(GameAssets.I.uiClick, 0.7f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Progress.RunScore = 0;
        Sfx.Play(GameAssets.I.uiClick, 0.7f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(MenuSceneName);
    }

    private void RefreshUI()
    {
        if (scoreText != null) scoreText.text = "SCORE: " + score;
        if (highScoreText != null) highScoreText.text = "BEST: " + Mathf.Max(HighScore, score);
    }
}
