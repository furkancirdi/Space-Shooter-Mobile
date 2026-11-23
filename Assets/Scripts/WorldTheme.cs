using UnityEngine;

// Her 20 bolumde bir gecilen "dunya"nin gorunumu: uzayin rengi ve dusman temasi.
// 1. dunya orijinal gorunumdur. Sonraki dunyalar, arka planin ve dusmanlarin renk tonu
// degistirilmis kopyalarini kullanir (Assets/Sprites/Themes, GameAssets'e bagli).
public static class WorldTheme
{
    private static readonly string[] Names = { "DEEP SPACE", "CRIMSON NEBULA", "EMERALD RIFT", "SOLAR STORM", "VIOLET VOID" };

    // Meteorlar ayri cizilmez; dunyanin dusman rengine hafifce boyanir
    private static readonly Color[] MeteorTints =
    {
        Color.white,
        new Color(0.7f, 0.95f, 1f),
        new Color(1f, 0.85f, 0.65f),
        new Color(0.8f, 0.75f, 1f),
        new Color(0.8f, 1f, 0.7f),
    };

    private const int ThemeCount = 4;   // 1. dunya disindaki tema sayisi

    private static int World => WaveDirector.Instance != null ? WaveDirector.Instance.World : Levels.World(Progress.Level);

    // -1 = orijinal gorunum; 0-3 = renkli temalar. 5. dunyadan sonra temalar sirayla tekrar eder.
    private static int ThemeIndex => World <= 1 ? -1 : (World - 2) % ThemeCount;

    public static string Name => Names[ThemeIndex + 1];
    public static Color MeteorTint => MeteorTints[ThemeIndex + 1];

    // Bir temanin sprite listesinden bulunulan dunyaya ait olani verir (1. dunyada null)
    public static Sprite Pick(Sprite[] themed, int perTheme = 1, int offset = 0)
    {
        int index = ThemeIndex * perTheme + offset;
        if (ThemeIndex < 0 || themed == null || index >= themed.Length) return null;
        return themed[index];
    }

    // Kayan arka plan katmanlarini dunyanin renkli kopyalariyla degistirir (yildizlar ayni kalir)
    public static void ApplyToBackground()
    {
        GameAssets a = GameAssets.I;
        if (a == null || ThemeIndex < 0) return;

        foreach (Parallax layer in Object.FindObjectsByType<Parallax>(FindObjectsSortMode.None))
        {
            foreach (SpriteRenderer sr in layer.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sr.sprite == null) continue;
                string name = sr.sprite.name;
                int slot = name.StartsWith("BackgroundBase") ? 0 : name.StartsWith("Background2") ? 1 : name.StartsWith("Background3") ? 2 : -1;
                if (slot < 0) continue;

                Sprite themed = Pick(a.themeBackgrounds, 3, slot);
                if (themed != null) sr.sprite = themed;
            }
        }
    }
}
