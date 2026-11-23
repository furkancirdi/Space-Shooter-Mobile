using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Ana menu: rekoru, coin bakiyesini ve siradaki bolumu gosterir.
// Start oyunu kaldigi bolumden baslatir, Shop marketi acar.
// Sahnedeki Start/Quit butonlarinin yanina gelen ogeler kodla eklenir.
public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private Text highScoreText;

    private Text coinText;
    private Shop shop;

    private void Start()
    {
        Time.timeScale = 1f;
        Progress.RunScore = 0;
        if (highScoreText != null)
        {
            highScoreText.text = "BEST SCORE: " + Progress.HighScore;
            BuildExtras(highScoreText.canvas.rootCanvas);
        }
        Sfx.PlayMusic();
    }

    private void BuildExtras(Canvas canvas)
    {
        Transform root = canvas.transform;
        Font font = highScoreText.font;
        Button startButton = Find<Button>(root, "StartButton");
        Button quitButton = Find<Button>(root, "QuitButton");
        Sprite buttonSprite = startButton != null ? startButton.image.sprite : null;

        // Siradaki bolum: rekorun altinda
        int level = Progress.Level;
        Text levelText = UiKit.Text(root, "LevelText", font, 44, TextAnchor.MiddleCenter, new Color(0.7f, 0.85f, 1f));
        levelText.text = "LEVEL " + level + (Levels.World(level) > 1 ? "  -  " + WorldTheme.Name : "");
        UiKit.Place(levelText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(1000, 70));

        // Coin bakiyesi: sag ustte
        Image coinIcon = UiKit.Image(root, "CoinIcon", GameAssets.I.coinSprite, Color.white);
        UiKit.Place(coinIcon.rectTransform, new Vector2(1, 1), new Vector2(-40, -60), new Vector2(64, 64));
        coinText = UiKit.Text(root, "CoinText", font, 48, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.25f));
        UiKit.Place(coinText.rectTransform, new Vector2(1, 1), new Vector2(-120, -60), new Vector2(500, 64));
        RefreshCoins();

        // Shop butonu Start ile Quit arasina girer
        if (quitButton != null)
            ((RectTransform)quitButton.transform).anchoredPosition = new Vector2(0, -550);
        Button shopButton = UiKit.Button(root, "ShopButton", "SHOP", font, buttonSprite, new Color(0.95f, 0.65f, 0.15f), OpenShop);
        UiKit.Place((RectTransform)shopButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -350), new Vector2(560, 150));

        shop = Shop.Create(canvas, font, buttonSprite, RefreshCoins);
    }

    private void RefreshCoins()
    {
        if (coinText != null) coinText.text = Progress.Coins.ToString();
    }

    private void OpenShop()
    {
        Sfx.Play(GameAssets.I.uiClick, 0.7f);
        if (shop != null) shop.Open();
    }

    public void StartGame()
    {
        Sfx.Play(GameAssets.I.uiClick, 0.7f);
        Progress.RunScore = 0;
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private static T Find<T>(Transform root, string name) where T : Component
    {
        foreach (T c in root.GetComponentsInChildren<T>(true))
            if (c.name == name) return c;
        return null;
    }
}
