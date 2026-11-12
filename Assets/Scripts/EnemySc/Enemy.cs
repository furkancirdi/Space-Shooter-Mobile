using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] protected float health;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected int scoreValue = 10;

    protected bool isDead;


    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        health -= dmg;
        HurtSequence();

        if (health <= 0)
        {
            isDead = true;
            DeathSequence();
        }

    }


    public virtual void HurtSequence()
    {
        //Damage animation
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

}
