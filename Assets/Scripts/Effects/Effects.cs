using UnityEngine;

// Prefab gerektirmeyen basit sprite efektleri (patlama animasyonu, parcaciklar).
public static class Effects
{
    // Sprite karelerini sirayla oynatip kendini yok eden efekt.
    public static void PlayAnimation(Sprite[] frames, Vector3 position, float scale, float fps,
                                     Color tint, Material material, int sortingOrder)
    {
        if (frames == null || frames.Length == 0) return;

        GameObject go = new GameObject("FX_Animation");
        go.transform.position = new Vector3(position.x, position.y, -6f);
        go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        if (material != null) sr.sharedMaterial = material;
        sr.sortingOrder = sortingOrder;
        sr.color = tint;

        go.AddComponent<SpriteAnimationFx>().Init(frames, fps);
    }

    // Verilen sprite'in kucuk kopyalarini disari dogru savurur (kirilan meteor vb.).
    public static void SpawnDebris(Sprite sprite, Vector3 position, int count, float scale,
                                   float speed, Color tint, Material material, int sortingOrder)
    {
        if (sprite == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject("FX_Debris");
            go.transform.position = new Vector3(position.x, position.y, -6f);
            go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            go.transform.localScale = Vector3.one * scale * Random.Range(0.6f, 1.2f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (material != null) sr.sharedMaterial = material;
            sr.sortingOrder = sortingOrder;
            sr.color = tint;

            Vector2 dir = Random.insideUnitCircle.normalized;
            go.AddComponent<DebrisFx>().Init(dir * speed * Random.Range(0.6f, 1.3f), Random.Range(-360f, 360f), Random.Range(0.35f, 0.6f));
        }
    }
}
