using UnityEngine;

// Prefab'i olmayan dusmanlari (atesci gemi, boss) kodla olusturur.
public static class EnemyFactory
{
    public static EnemyShooter CreateShooter(Vector3 position)
    {
        EnemyShooter shooter = Create<EnemyShooter>("EnemyShooter", GameAssets.I.shooterSprite, position, 1f, 0.5f);
        shooter.Configure(5f * Levels.EnemyHealth(Progress.Level), 40, 0.12f, 2);
        return shooter;
    }

    // Boss'un cani ve puani bolume gore belirlenir (Levels.cs)
    public static Boss CreateBoss(Vector3 position, int level)
    {
        Boss boss = Create<Boss>("Boss", GameAssets.I.bossSprite, position, 1.3f, 0.95f);
        boss.Configure(Levels.BossHealth(level), Levels.BossScore(level), 0f, Levels.BossCoins(level));
        return boss;
    }

    private static T Create<T>(string name, Sprite sprite, Vector3 position, float scale, float colliderRadius) where T : Enemy
    {
        GameObject go = new GameObject(name);
        int layer = LayerMask.NameToLayer("Enemy");
        if (layer >= 0) go.layer = layer;
        go.transform.position = new Vector3(position.x, position.y, -5f);
        go.transform.localScale = new Vector3(scale, scale, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        if (GameAssets.I.spriteMaterial != null) sr.sharedMaterial = GameAssets.I.spriteMaterial;
        sr.sortingOrder = 5;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = colliderRadius;

        return go.AddComponent<T>();
    }
}
