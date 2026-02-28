using UnityEngine;

public class Meteor : Enemy
{

    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    private float speed;
    [SerializeField] private float rotateSpeed;

    void Start()
    {
        speed = Random.Range(minSpeed, maxSpeed);
        rb.linearVelocity = Vector2.down * speed;
    }

    void Update()
    {
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }


    public override void HurtSequence()
    {
            //Damage animation
    }
    
    public override void DeathSequence()
    {
        //Destroy animation
    }


    private void OnTriggerEnter2D(Collider2D otherColl)
    {

        if (otherColl.CompareTag("Player"))
        {
            Destroy(otherColl.gameObject);
        }

        
    }


    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }


}
