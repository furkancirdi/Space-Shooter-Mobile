using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] protected float health;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected int scoreValue = 10;

    [Header("Effects")]
    [SerializeField] protected Sprite[] explosionFrames;
    [SerializeField] protected Color hitFlashColor = new Color(1f, 0.45f, 0.45f);

    protected bool isDead;
    protected SpriteRenderer sr;
    private Color baseColor = Color.white;
    private Coroutine flashRoutine;

    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }


    public void TakeDamage(float dmg)
    {
        TakeDamage(dmg, transform.position);
    }

    public void TakeDamage(float dmg, Vector3 hitPoint)
    {
        if (isDead) return;

        health -= dmg;

        if (health <= 0)
        {
            isDead = true;
            DeathSequence();
        }
        else
        {
            HurtSequence(hitPoint);
        }

    }


    // Varsayilan: renk flasi + vurulan noktada kucuk kivilcim
    public virtual void HurtSequence(Vector3 hitPoint)
    {
        Flash();
        Effects.PlayAnimation(explosionFrames, hitPoint, 0.35f, 30f, Color.white, Material, SortingOrder + 1);
    }

    // Alt siniflar base.DeathSequence() cagirarak puani verir
    public virtual void DeathSequence()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);
    }

    protected void KillPlayer(GameObject player)
    {
        Destroy(player);
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }

    protected Material Material => sr != null ? sr.sharedMaterial : null;
    protected int SortingOrder => sr != null ? sr.sortingOrder : 5;

    protected void Flash()
    {
        if (sr == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        sr.color = hitFlashColor;
        yield return new WaitForSeconds(0.08f);
        sr.color = baseColor;
        flashRoutine = null;
    }

}
