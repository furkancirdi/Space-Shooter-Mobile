using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Oyun ici ek arayuz: canlar, aktif gucler, asama etiketi, uyari yazisi,
// boss can cubugu ve ucusan puan/zincir yazilari. Hepsi kodla kurulur.
public class Hud : MonoBehaviour
{
    private RectTransform root;
    private Canvas canvas;
    private Font font;

    private Image[] hearts;
    private Text statusText;
    private Text stageText;
    private Text bannerText;
    private Coroutine bannerRoutine;
    private GameObject bossBar;
    private RectTransform bossFill;
    private const float BossBarWidth = 700f;

    private int shownLives = -1;
    private Text coinText;
    private Sprite buttonSprite;

    // insertBefore: bu nesnenin arkasinda kalacak sekilde eklenir (Game Over paneli ustte kalsin)
    public static Hud Create(Canvas canvas, Transform insertBefore)
    {
        GameObject go = new GameObject("RuntimeHud", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        if (insertBefore != null && insertBefore.parent == canvas.transform)
            go.transform.SetSiblingIndex(insertBefore.GetSiblingIndex());

        Hud hud = go.AddComponent<Hud>();
        hud.canvas = canvas;
        hud.root = go.GetComponent<RectTransform>();
        hud.root.anchorMin = Vector2.zero;
        hud.root.anchorMax = Vector2.one;
        hud.root.offsetMin = Vector2.zero;
        hud.root.offsetMax = Vector2.zero;
        if (insertBefore != null)
        {
            Button existing = insertBefore.GetComponentInChildren<Button>(true);
            if (existing != null && existing.image != null) hud.buttonSprite = existing.image.sprite;
        }
        hud.font = GameAssets.I.font != null ? GameAssets.I.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hud.Build();
        return hud;
    }

    private void Build()
    {
        // Canlar: sol ustte, skorun altinda
        hearts = new Image[PlayerStatus.MaxLives];
        for (int i = 0; i < hearts.Length; i++)
        {
            GameObject h = new GameObject("Heart" + i, typeof(RectTransform), typeof(Image));
            h.transform.SetParent(root, false);
            Place(h.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(40 + i * 66, -135), new Vector2(56, 56));
            hearts[i] = h.GetComponent<Image>();
            hearts[i].sprite = GameAssets.I.heartSprite;
            hearts[i].raycastTarget = false;
        }

        // Coin sayaci: canlarin altinda
        GameObject coinIcon = new GameObject("CoinIcon", typeof(RectTransform), typeof(Image));
        coinIcon.transform.SetParent(root, false);
        Place(coinIcon.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(42, -206), new Vector2(46, 46));
        coinIcon.GetComponent<Image>().sprite = GameAssets.I.coinSprite;
        coinIcon.GetComponent<Image>().raycastTarget = false;

        coinText = MakeText("CoinText", 36, TextAnchor.MiddleLeft, new Color(1f, 0.85f, 0.25f));
        Place(coinText.rectTransform, new Vector2(0, 1), new Vector2(100, -206), new Vector2(300, 46));
        RefreshCoins();

        // Aktif gucler: sag ustte, rekorun altinda
        statusText = MakeText("PowerUpStatus", 34, TextAnchor.UpperRight, new Color(0.5f, 1f, 0.6f));
        Place(statusText.rectTransform, new Vector2(1, 1), new Vector2(-40, -135), new Vector2(600, 120));

        // Asama etiketi: ust ortada
        stageText = MakeText("StageLabel", 30, TextAnchor.UpperCenter, new Color(0.7f, 0.85f, 1f));
        Place(stageText.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(400, 50));

        // Buyuk uyari yazisi: ekranin ortasinin biraz ustunde
        bannerText = MakeText("Banner", 84, TextAnchor.MiddleCenter, Color.white);
        Place(bannerText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 280), new Vector2(1000, 220));
        bannerText.gameObject.SetActive(false);

        // Boss can cubugu
        bossBar = new GameObject("BossBar", typeof(RectTransform), typeof(Image));
        bossBar.transform.SetParent(root, false);
        Place(bossBar.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0, -270), new Vector2(BossBarWidth + 8, 30));
        bossBar.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        bossBar.GetComponent<Image>().raycastTarget = false;

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(bossBar.transform, false);
        bossFill = fill.GetComponent<RectTransform>();
        bossFill.anchorMin = new Vector2(0, 0.5f);
        bossFill.anchorMax = new Vector2(0, 0.5f);
        bossFill.pivot = new Vector2(0, 0.5f);
        bossFill.anchoredPosition = new Vector2(4, 0);
        bossFill.sizeDelta = new Vector2(BossBarWidth, 22);
        fill.GetComponent<Image>().color = new Color(0.95f, 0.25f, 0.45f);
        fill.GetComponent<Image>().raycastTarget = false;
        bossBar.SetActive(false);
    }

    private void Update()
    {
        PlayerStatus p = PlayerStatus.Instance;
        int lives = p != null ? p.Lives : 0;
        if (lives != shownLives)
        {
            shownLives = lives;
            for (int i = 0; i < hearts.Length; i++) hearts[i].enabled = i < lives;
        }

        string status = "";
        if (p != null)
        {
            if (p.ShieldTime > 0f) status += "SHIELD " + Mathf.CeilToInt(p.ShieldTime) + "\n";
            if (p.TripleShotTime > 0f) status += "TRIPLE " + Mathf.CeilToInt(p.TripleShotTime);
        }
        if (statusText.text != status) statusText.text = status;
    }

    public void SetLevel(int level)
    {
        stageText.text = "LEVEL " + level;
    }

    public void RefreshCoins()
    {
        coinText.text = Progress.Coins.ToString();
    }

    // Bolum sonu ekrani: skor, toplanan coin, sonraki bolum ve menu butonlari
    public void ShowLevelComplete(int level, int score, int coins, UnityEngine.Events.UnityAction onNext, UnityEngine.Events.UnityAction onMenu)
    {
        GameObject dim = new GameObject("LevelCompletePanel", typeof(RectTransform), typeof(Image));
        dim.transform.SetParent(canvas.transform, false);
        RectTransform dimRt = dim.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = dimRt.offsetMax = Vector2.zero;
        dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

        RectTransform panel = MakeBox(dim.transform, "Panel", new Color(0.08f, 0.1f, 0.25f, 0.95f));
        Place(panel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850, 1000));

        Text title = MakeText("Title", 64, TextAnchor.MiddleCenter, new Color(0.5f, 1f, 0.6f), panel);
        title.text = "LEVEL " + level + "\nCOMPLETE";
        Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(800, 200));

        Text scoreLine = MakeText("Score", 50, TextAnchor.MiddleCenter, Color.white, panel);
        scoreLine.text = "SCORE: " + score;
        Place(scoreLine.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -290), new Vector2(800, 80));

        Text coinLine = MakeText("Coins", 44, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.25f), panel);
        coinLine.text = "+" + coins + " COINS   (" + Progress.Coins + ")";
        Place(coinLine.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -380), new Vector2(800, 70));

        string nextLabel = "NEXT LEVEL";
        if (Levels.IsFirstOfWorld(level + 1))
        {
            Text sector = MakeText("Sector", 30, TextAnchor.MiddleCenter, new Color(0.7f, 0.85f, 1f), panel);
            sector.text = "NEW SECTOR UNLOCKED";
            Place(sector.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -455), new Vector2(800, 50));
            nextLabel = "NEW SECTOR";
        }

        MakeButton(panel, "NextButton", nextLabel, new Vector2(0, -80), onNext);
        MakeButton(panel, "MenuButton", "SHOP / MENU", new Vector2(0, -260), onMenu);
    }

    private RectTransform MakeBox(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.sprite = buttonSprite;
        img.type = Image.Type.Sliced;
        img.color = color;
        return go.GetComponent<RectTransform>();
    }

    private void MakeButton(Transform parent, string name, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform rt = MakeBox(parent, name, new Color(0.25f, 0.5f, 0.95f));
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(620, 150);

        Button button = rt.gameObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        Text text = MakeText("Label", 48, TextAnchor.MiddleCenter, Color.white, rt);
        text.text = label;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
    }

    public void ShowBanner(string message, Color color, float seconds)
    {
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerRoutine = StartCoroutine(BannerRoutine(message, color, seconds));
    }

    private IEnumerator BannerRoutine(string message, Color color, float seconds)
    {
        bannerText.text = message;
        bannerText.gameObject.SetActive(true);
        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            // Baslangicta ve sonda yumusak gecis, arada hafif nabiz
            float fade = Mathf.Clamp01(t / 0.25f) * Mathf.Clamp01((seconds - t) / 0.4f);
            color.a = fade * (0.75f + 0.25f * Mathf.Sin(t * 10f));
            bannerText.color = color;
            yield return null;
        }
        bannerText.gameObject.SetActive(false);
        bannerRoutine = null;
    }

    public void ShowBossBar(bool show)
    {
        bossBar.SetActive(show);
        if (show) SetBossHealth(1f);
    }

    public void SetBossHealth(float fraction)
    {
        bossFill.sizeDelta = new Vector2(BossBarWidth * Mathf.Clamp01(fraction), 22);
    }

    // Dunya konumunda beliren, yukari suzulup solan kisa yazi (zincir carpani, guc adi)
    public void Popup(string message, Vector3 worldPosition, Color color, int size)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Text text = MakeText("Popup", size, TextAnchor.MiddleCenter, color);
        text.text = message;
        RectTransform rt = text.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(600, 80);

        Vector2 screen = cam.WorldToScreenPoint(worldPosition);
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, uiCam, out Vector2 local);
        // Ekran kenarindan tasmasin
        float halfWidth = root.rect.width * 0.5f - 160f;
        local.x = Mathf.Clamp(local.x, -halfWidth, halfWidth);
        // Ust uste binmesin diye hafif rastgele kaydir
        rt.anchoredPosition = local + new Vector2(Random.Range(-50f, 50f), Random.Range(-30f, 40f));

        StartCoroutine(PopupRoutine(text, color));
    }

    private IEnumerator PopupRoutine(Text text, Color color)
    {
        const float life = 0.8f;
        RectTransform rt = text.rectTransform;
        Vector2 start = rt.anchoredPosition;
        float t = 0f;
        while (t < life)
        {
            t += Time.deltaTime;
            float f = t / life;
            rt.anchoredPosition = start + Vector2.up * (90f * f);
            rt.localScale = Vector3.one * (1f + 0.3f * Mathf.Sin(Mathf.Clamp01(f * 4f) * Mathf.PI));
            color.a = 1f - f * f;
            text.color = color;
            yield return null;
        }
        Destroy(text.gameObject);
    }

    private Text MakeText(string name, int size, TextAnchor anchor, Color color, Transform parent = null)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent != null ? parent : root, false);
        Text t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    // Kose/kenar capasina gore yerlestirir (pivot = capa)
    private static void Place(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }
}
