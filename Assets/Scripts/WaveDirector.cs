using System.Collections;
using UnityEngine;

// Bir bolumun akisini yonetir: once dalga asamasi (meteorlar, gemiler), sonra boss.
// Boss yenilince bolum biter. Sureler ve zorluk Levels.cs'ten gelir.
public class WaveDirector : MonoBehaviour
{
    public static WaveDirector Instance { get; private set; }

    public int Level { get; private set; } = 1;
    public int World => Levels.World(Level);
    public bool BossActive { get; private set; }
    public bool Cleared { get; private set; }

    // Dalga asamasinin ne kadari gecti (0-1). Bolum icinde dusmanlar giderek siklasir.
    public float WaveProgress => Mathf.Clamp01(levelTime / waveDuration);

    private float levelTime;
    private float waveDuration = 30f;
    private float shooterTimer = 6f;

    private void Awake()
    {
        Instance = this;
        Level = Progress.Level;
        waveDuration = Levels.WaveDuration(Level);
        Coin.AttractAll = false;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        WorldTheme.ApplyToBackground();

        Hud hud = GameManager.Instance.Hud;
        if (hud == null) return;
        hud.SetLevel(Level);

        // Yeni dunyanin ilk bolumunde dunyanin adi da gosterilir
        string banner = "LEVEL " + Level;
        if (Levels.IsFirstOfWorld(Level) && World > 1) banner = WorldTheme.Name + "\n" + banner;
        hud.ShowBanner(banner, new Color(0.55f, 0.85f, 1f), 2.4f);
    }

    private void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.IsGameOver || BossActive || Cleared) return;

        levelTime += Time.deltaTime;

        if (levelTime >= waveDuration)
        {
            StartCoroutine(BossIntro());
            return;
        }

        if (!Levels.ShootersUnlocked(Level)) return;

        shooterTimer -= Time.deltaTime;
        if (shooterTimer > 0f) return;
        shooterTimer = Levels.ShooterInterval(Level);

        if (CountShooters() >= Levels.MaxShooters(Level)) return;

        Vector2 pos = Playfield.At(Random.Range(0.25f, 0.75f), 1f);
        EnemyFactory.CreateShooter(new Vector3(pos.x, pos.y + 1f, 0f));
    }

    private IEnumerator BossIntro()
    {
        BossActive = true;

        Hud hud = GameManager.Instance.Hud;
        if (hud != null) hud.ShowBanner("WARNING", new Color(1f, 0.3f, 0.3f), 2.6f);
        Sfx.Play(GameAssets.I.bossWarning, 0.7f);

        // Boss oncesi bir can gucu duser: dalgada can kaybeden oyuncu toparlanabilsin
        Vector2 top = Playfield.At(Random.Range(0.3f, 0.7f), 1f);
        PowerUp.Spawn(new Vector3(top.x, top.y + 0.5f, 0f), PowerUp.Kind.Heal);
        yield return new WaitForSeconds(2.6f);

        if (GameManager.Instance.IsGameOver) yield break;

        Vector2 pos = Playfield.At(0.5f, 1f);
        EnemyFactory.CreateBoss(new Vector3(pos.x, pos.y + 2.5f, 0f), Level);
        if (hud != null) hud.ShowBossBar(true);
    }

    public void OnBossDefeated(Vector3 position)
    {
        if (Cleared) return;
        Cleared = true;
        BossActive = false;

        Hud hud = GameManager.Instance != null ? GameManager.Instance.Hud : null;
        if (hud != null) hud.ShowBossBar(false);

        if (GameManager.Instance != null) GameManager.Instance.LevelCleared();
    }

    // Spawner'lar bir prefab'i olusturmadan once sorar
    public static bool CanSpawn(Enemy prefab)
    {
        if (Instance == null || GameManager.Instance == null) return true;
        if (Instance.Cleared) return false;
        if (prefab is EnemyShip)
            return Levels.ShipsUnlocked(Instance.Level) && Instance.levelTime >= 4f && !Instance.BossActive;
        return true;
    }

    // Boss varken meteorlar seyrek gelir (zincir icin malzeme, ama kalabalik degil)
    public static float SpawnIntervalFactor => Instance != null && Instance.BossActive ? 2.2f : 1f;

    private static int CountShooters()
    {
        int count = 0;
        for (int i = 0; i < Enemy.Active.Count; i++)
            if (Enemy.Active[i] is EnemyShooter) count++;
        return count;
    }
}
