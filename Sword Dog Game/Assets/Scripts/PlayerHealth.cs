using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    private int maxHealth = 8;
    public int health = 8;
    public bool dead;
    private float iFrameTime = 1.0f, lastDamaged = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health > maxHealth)
        {
            health = maxHealth;
        }

        // TODO remove debug keybinds 
        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeDamage(1);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(1);
        }
    }

    public void UpdateHealth()
    {
        int temp = health;
        for (int i = 0; i < CanvasManager.hearts.Count; i++)
        {
            CanvasManager.hearts[i].SetSprite(temp);
            temp -= 2;
        }
    }

    // TODO add directional damage parameter for knockback
    public void TakeDamage(int damage, bool bypass = false)
    {
        if ((!bypass && Time.time > lastDamaged + iFrameTime) || bypass)
        {
            lastDamaged = Time.time;
            health -= damage;
            if (health <= 0) {
                Die();
            }

            // TODO this code is absolutely horrible, in the future we will prob want an instance variable reference to the sword object or to do this in swordfollow
            FindObjectOfType<SwordFollow>().GetComponent<SimpleFlash>().Flash(1f, 3, true);
            GetComponent<SimpleFlash>().Flash(1f, 3, true);
            GetComponent<TimeStop>().StopTime();
            UpdateHealth();
        }
    }

    public void Heal(int amount)
    {
        health += amount;
        UpdateHealth();
    }

    // TODO something more fancy later for deaths
    public void Die()
    {
        health = 0;
        dead = true;
    }
}
