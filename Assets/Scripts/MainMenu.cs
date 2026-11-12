using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Ana menu: rekoru gosterir, Start ile oyun sahnesini yukler.
public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private Text highScoreText;

    private void Start()
    {
        Time.timeScale = 1f;
        if (highScoreText != null) highScoreText.text = "BEST SCORE: " + GameManager.HighScore;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
