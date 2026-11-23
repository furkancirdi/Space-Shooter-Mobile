using UnityEngine;

// Kalici ilerleme: coin, ulasilan bolum, marketten alinan yukseltmeler ve rekor.
// Hepsi PlayerPrefs'te tutulur.
public static class Progress
{
    public enum Upgrade { Damage, FireRate, Hull, Magnet, PowerTime }

    public class UpgradeInfo
    {
        public string title;
        public string description;
        public int[] costs;      // her kademe icin fiyat; uzunlugu = en yuksek kademe
    }

    // Marketteki urunler. Fiyatlar ve etkiler buradan ayarlanir.
    public static readonly UpgradeInfo[] Upgrades =
    {
        new UpgradeInfo { title = "LASER POWER", description = "+20% damage", costs = new[] { 40, 90, 160, 260, 400 } },
        new UpgradeInfo { title = "FIRE RATE", description = "+10% shots", costs = new[] { 40, 90, 160, 260, 400 } },
        new UpgradeInfo { title = "HULL", description = "+1 starting life", costs = new[] { 150, 400 } },
        new UpgradeInfo { title = "MAGNET", description = "+40% coin range", costs = new[] { 30, 70, 130 } },
        new UpgradeInfo { title = "POWER TIME", description = "+20% power-up time", costs = new[] { 50, 110, 200 } },
    };

    // Testler gercek kaydi bozmasin diye anahtarlarin onune eklenir
    public static string KeyPrefix = "";

    // Bir oturumda bolumden bolume tasinan skor (menuye donunce ya da olunce sifirlanir)
    public static int RunScore;

    private static string Key(string name) { return KeyPrefix + name; }

    public static int Coins
    {
        get { return PlayerPrefs.GetInt(Key("Coins"), 0); }
        private set { PlayerPrefs.SetInt(Key("Coins"), Mathf.Max(0, value)); }
    }

    // Oyuncunun oynayacagi bolum (1'den baslar). Boss yenilince bir artar.
    public static int Level
    {
        get { return Mathf.Max(1, PlayerPrefs.GetInt(Key("Level"), 1)); }
        set { PlayerPrefs.SetInt(Key("Level"), Mathf.Max(1, value)); }
    }

    public static int HighScore
    {
        get { return PlayerPrefs.GetInt(Key("HighScore"), 0); }
        set { PlayerPrefs.SetInt(Key("HighScore"), value); }
    }

    public static void AddCoins(int amount)
    {
        Coins = Coins + amount;
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    // ---------------- Yukseltmeler ----------------

    public static int Tier(Upgrade u)
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(Key("Up_" + u), 0), 0, MaxTier(u));
    }

    public static int MaxTier(Upgrade u) { return Upgrades[(int)u].costs.Length; }

    public static bool IsMaxed(Upgrade u) { return Tier(u) >= MaxTier(u); }

    // Siradaki kademenin fiyati; en yuksek kademedeyse -1
    public static int NextCost(Upgrade u)
    {
        return IsMaxed(u) ? -1 : Upgrades[(int)u].costs[Tier(u)];
    }

    public static bool TryBuy(Upgrade u)
    {
        int cost = NextCost(u);
        if (cost < 0 || Coins < cost) return false;

        Coins = Coins - cost;
        PlayerPrefs.SetInt(Key("Up_" + u), Tier(u) + 1);
        Save();
        return true;
    }

    public static float DamageMultiplier => 1f + 0.2f * Tier(Upgrade.Damage);
    public static float FireRateMultiplier => 1f + 0.1f * Tier(Upgrade.FireRate);
    public static int ExtraLives => Tier(Upgrade.Hull);
    public static float MagnetMultiplier => 1f + 0.4f * Tier(Upgrade.Magnet);
    public static float PowerTimeMultiplier => 1f + 0.2f * Tier(Upgrade.PowerTime);

    // Test ve "ilerlemeyi sifirla" icin
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(Key("Coins"));
        PlayerPrefs.DeleteKey(Key("Level"));
        PlayerPrefs.DeleteKey(Key("HighScore"));
        foreach (Upgrade u in System.Enum.GetValues(typeof(Upgrade)))
            PlayerPrefs.DeleteKey(Key("Up_" + u));
        Save();
    }
}
