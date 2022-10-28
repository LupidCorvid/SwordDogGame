using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordFollow : MonoBehaviour
{
    public GameObject player;
    Vector3 playerLocation;
    Vector3 swordTargetLocation;
    Vector3 swordPreviousLocation;
    public float speed;
    public float adjustLocationY;
    public float adjustLocationX, adjustDefaultX;
    SpriteRenderer sr;
    Rigidbody2D rb;

    private PlayerMovement pmScript;
    bool triggeredPMScript;

    // Start is called before the first frame update
    void Start()
    {
        //adjust the adjustLocation variables per sword type
        speed = 12;
        adjustLocationY = 1;
        adjustDefaultX = -.5f;
        sr = gameObject.GetComponent<SpriteRenderer>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        
        triggeredPMScript = false;
    }
    

    // Update is called once per frame
    void FixedUpdate()
    {
        //Accesses PlayerMovement script ONCE
        if(!triggeredPMScript)
        {
            pmScript = player.GetComponent<PlayerMovement>();
            triggeredPMScript = true;
        }
        
        if(player != null && !(PlayerHealth.dead && !PlayerHealth.gettingUp))
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            //Assigns target transform values
            pmScript = player.GetComponent<PlayerMovement>();

            player = GameObject.FindGameObjectWithTag("Player");
            playerLocation = player.transform.position;

            var offset = player.transform.rotation * new Vector2(adjustLocationX, adjustLocationY);
            swordTargetLocation = playerLocation + offset;

            //Moves
            swordPreviousLocation = transform.position;
            
            if (!PlayerHealth.gettingUp)
            {
                transform.position = Vector3.Lerp(transform.position, swordTargetLocation, 2 + 4 * pmScript.calculatedSpeed * Time.deltaTime); //start value, end val, value used to interpolate between a and b
                transform.rotation = player.transform.rotation;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, swordTargetLocation, 4 * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, player.transform.rotation, 4 * Time.deltaTime);
            }

            //Checks when to flip and adjust sprite
            
            if (pmScript.isJumping)
            {
                if (!pmScript.facingRight) adjustLocationX = -adjustDefaultX - .8f;
                else adjustLocationX = adjustDefaultX + .8f;
                
                adjustLocationY = 0.9f;
            }
            else
            {
                adjustLocationY = 0.7f;
            }

            if (pmScript.isSprinting)
            {
                adjustDefaultX = Mathf.Lerp(adjustDefaultX, 0.2f, 0.1f);
            }
            else
            {
                adjustDefaultX = Mathf.Lerp(adjustDefaultX, -0.2f, 0.4f);
            }
            
            if (pmScript.facingRight == false)
            {
                adjustLocationX = -adjustDefaultX;
                sr.flipX = true;
            }
            else
            {
                adjustLocationX = adjustDefaultX;
                sr.flipX = false;
            }
        }
        else
        {
            rb.gravityScale = 5;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.AddTorque((sr.flipX) ? -5f : 5f);
        }
    }
}
