using UnityEngine;

// Bolumlerin zorluk egrisi. Oyunun butun denge sayilari burada toplanir:
// bir bolum zor ya da kolay gelirse yalnizca bu dosyadaki sayilari degistirmek yeterli.
//
// Her dunyada 20 bolum vardir. Bolum = dalga asamasi + boss. Yeni dunyaya gecince
// zorluk bir onceki dunyanin sonundan biraz geri cekilir, sonra yeniden tirmanir.
public static class Levels
{
    public const int PerWorld = 20;

    public static int World(int level) { return (level - 1) / PerWorld + 1; }

    // Dunya icindeki sira: 0-19
    public static int Index(int level) { return (level - 1) % PerWorld; }

    public static bool IsFirstOfWorld(int level) { return Index(level) == 0; }

    // Dusmanlarin gelme sikligi carpani (1 = en seyrek)
    public static float SpawnRate(int level)
    {
        return Mathf.Min(3.5f, 1f + 0.06f * Index(level) + 0.35f * (World(level) - 1));
    }

    // Dusman, mermi ve arka plan hiz carpani
    public static float Speed(int level)
    {
        return Mathf.Min(1.9f, 1f + 0.02f * Index(level) + 0.12f * (World(level) - 1));
    }

    // Boss gelmeden onceki dalga asamasinin suresi (saniye)
    public static float WaveDuration(int level)
    {
        return 30f + Index(level);
    }

    // Dusman can carpani (meteor, gemi, atesci)
    public static float EnemyHealth(int level)
    {
        return 1f + 0.025f * Index(level) + 0.25f * (World(level) - 1);
    }

    public static float BossHealth(int level)
    {
        return (40f + 8f * Index(level)) * (1f + 0.25f * (World(level) - 1));
    }

    public static int BossScore(int level) { return 200 + 50 * (level - 1); }

    // Boss'un yelpaze atisindaki mermi sayisi (cani yariya inince 2 artar)
    public static int BossFanBullets(int level)
    {
        return World(level) > 1 || Index(level) >= 10 ? 7 : 5;
    }

    // Boss'un saldiri temposu ve mermi hizi: ilk bolumlerde yavas (0.6), 11. bolumden itibaren tam (1)
    public static float BossPace(int level)
    {
        if (World(level) > 1) return 1f;
        return Mathf.Lerp(0.6f, 1f, Index(level) / 10f);
    }

    // Nisanli seri atistaki mermi sayisi
    public static int BossBurst(int level)
    {
        return World(level) == 1 && Index(level) < 4 ? 2 : 3;
    }

    // Zikzak gemiler 2. bolumde, atesci gemiler 3. bolumde baslar
    public static bool ShipsUnlocked(int level) { return level >= 2; }
    public static bool ShootersUnlocked(int level) { return level >= 3; }

    public static int MaxShooters(int level)
    {
        if (World(level) > 1) return Index(level) >= 10 ? 3 : 2;
        if (level >= 15) return 3;
        return level >= 8 ? 2 : 1;
    }

    public static float ShooterInterval(int level)
    {
        return 11f / (1f + 0.04f * Index(level) + 0.2f * (World(level) - 1));
    }

    // ---------------- Coin ----------------

    // Sonraki dunyalarda her coin daha degerlidir
    public static int CoinValue(int level) { return World(level); }

    public static int BossCoins(int level) { return Mathf.Min(30, 8 + Index(level) / 2); }
}
