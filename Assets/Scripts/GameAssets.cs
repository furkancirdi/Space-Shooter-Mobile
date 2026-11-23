using UnityEngine;

// Kodla olusturulan nesnelerin (dusman, mermi, guc, HUD, ses) kullandigi ortak varliklar.
// Assets/Resources/GameAssets.asset dosyasindan yuklenir; Inspector'dan degistirilebilir.
public class GameAssets : ScriptableObject
{
    private static GameAssets instance;

    public static GameAssets I
    {
        get
        {
            if (instance == null) instance = Resources.Load<GameAssets>("GameAssets");
            return instance;
        }
    }

    [Header("Sprites")]
    public Sprite shooterSprite;
    public Sprite bossSprite;
    public Sprite enemyBulletSprite;
    public Sprite bossBulletSprite;
    public Sprite healSprite;
    public Sprite tripleShotSprite;
    public Sprite shieldPickupSprite;
    public Sprite shieldBubbleSprite;
    public Sprite heartSprite;
    public Sprite coinSprite;

    [Header("World themes (2. dunyadan itibaren, her tema icin bir tane)")]
    public Sprite[] themeShips;
    public Sprite[] themeShooters;
    public Sprite[] themeBosses;
    [Tooltip("Her tema icin 3 katman: BackgroundBase, Background2, Background3")]
    public Sprite[] themeBackgrounds;
    public Sprite[] explosionFrames;

    [Header("Rendering")]
    public Material spriteMaterial;
    public Font font;

    [Header("Audio")]
    public AudioClip laser;
    public AudioClip hit;
    public AudioClip explosion;
    public AudioClip explosionBig;
    public AudioClip chain;
    public AudioClip powerup;
    public AudioClip playerHit;
    public AudioClip gameOver;
    public AudioClip enemyShoot;
    public AudioClip bossWarning;
    public AudioClip shieldBlock;
    public AudioClip uiClick;
    public AudioClip coin;
    public AudioClip levelComplete;
    public AudioClip buy;
    public AudioClip music;
}
