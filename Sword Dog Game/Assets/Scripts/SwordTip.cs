using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTip : MonoBehaviour
{
    public SwordFollow sword;

    public int damage = 1;

    public float knockback = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (PlayerHealth.dead && other.gameObject.CompareTag("Ground"))
        {
            sword.Freeze();
            return;
        }

        if (!sword.pmScript.attacking)
            return;

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();

        enemy?.TakeDamage(damage);

        if(otherRb != null)
        {
            if(otherRb.bodyType != RigidbodyType2D.Static && otherRb != sword.pmScript.rb)
                otherRb.AddForce(((Vector2)(other.transform.position - sword.pmScript.transform.position).normalized) * knockback * (Mathf.Clamp(sword.rb.velocity.magnitude, 0, 2)));
        }
    }
}
