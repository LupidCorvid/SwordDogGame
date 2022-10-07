using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static GameObject instance;
    public static PlayerMovement controller;
    private Rigidbody2D rb;
    private Animator anim;
    private bool trotting, wasGrounded, holdingJump;
    public bool facingRight, isGrounded, isJumping, isFalling, isSprinting;
    private float moveX, prevMoveX, beenOnLand, lastOnLand, jumpTime, jumpSpeedMultiplier, timeSinceJumpPressed, fallTime, sprintSpeedMultiplier;
    private int stepDirection, stops;
    private Vector3 targetVelocity, velocity = Vector3.zero;
    [SerializeField] private float speed = 4f;

    // Radius of the overlap circle to determine if grounded
    const float groundedRadius = 0.2f;

    // A mask determining what is ground to the character
    [SerializeField] public LayerMask whatIsGround;

    // Positions marking where to check if the player is grounded
    //[SerializeField] public Transform[] groundChecks;
    public CollisionsTracker groundCheck;

    // Amount of force added when the player jumps
    [SerializeField] private float jumpForce = 2000f;

    // How much to smooth out movement
    [Range(0, .3f)][SerializeField] private float movementSmoothing = 0.05f;

    // Slope variables
    private Vector2 colliderSize;
    [SerializeField] private float slopeCheckDistance;
    [SerializeField] private float maxSlopeAngle;
    private float slopeDownAngle;
    private float slopeDownAngleOld;
    private float slopeSideAngle;
    private Vector2 slopeNormalPerp;
    private bool isOnSlope, canWalkOnSlope;
    public PhysicsMaterial2D slippery, friction;
    public float calculatedSpeed = 4.0f;

    Collider2D cldr;

    Vector2 upperLeftCorner;
    Vector2 upperRightCorner;

    public bool dead, resetting, invincible;

    public bool onlyRotateWhenGrounded;
    float lastGroundedSlope = 0;
    float lastUngroundedSlope = 0;
    public float landAnimTime = .5f;
    float lastLand = 0;

    Vector2 lastMidairVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        cldr = GetComponent<Collider2D>();
        colliderSize = GetComponent<BoxCollider2D>().size;
        timeSinceJumpPressed = 0.2f;
        jumpSpeedMultiplier = 1.0f;
        sprintSpeedMultiplier = 1.0f;
        fallTime = 0.0f;
        jumpTime = 0.0f;

        stepDirection = 1;
        facingRight = true;

        // Singleton design pattern
        if (instance != null && instance != this)
        {
            // Destroy(gameObject);
        }
        else
        {
            controller = this;
            instance = gameObject;
            DontDestroyOnLoad(gameObject);
        }

        upperLeftCorner = new Vector2((-cldr.bounds.extents.x * 1) + cldr.offset.x, cldr.bounds.extents.y + cldr.offset.y);
        upperRightCorner = new Vector2((cldr.bounds.extents.x * 1) + cldr.offset.x, upperLeftCorner.y);

        groundCheck.triggerEnter += checkIfLanding;
    }

    public void checkIfLanding(Collider2D collision)
    {
        if(Mathf.Pow(2, collision.gameObject.layer) == whatIsGround && !isGrounded)
        {
            lastLand = Time.time;
        }
    }

    IEnumerator RemoveStop()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        stops--;
    }

    void Update()
    {
        // remember previous movement input
        prevMoveX = moveX;

        // grab movement input from horizontal axis
        moveX = Input.GetAxisRaw("Horizontal");

        anim.SetBool("moveX", moveX != 0 && Mathf.Abs(rb.velocity.x) > 0f);

        // track stops per second
        if (prevMoveX != 0 && moveX == 0)
        {
            stops++;
            StartCoroutine("RemoveStop");
        }

        // fix input spam breaking trot state
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("idleAnim"))
        {
            trotting = false;
        }

        // start trotting if player gives input and is moving
        if (isGrounded && moveX != 0 && !trotting && rb.velocity.x != 0 && !isJumping)
        {
            anim.SetTrigger("trot");
            trotting = true;
        }

        // jump code
        Jump();

        // release jump
        if (Input.GetButtonUp("Jump"))
        {
            holdingJump = false;
        }

        // TODO REMOVE - debug cinematic bars keybind
        if (Input.GetKeyDown(KeyCode.V))
        {
            GameObject.FindObjectOfType<CinematicBars>().Show(200, .3f);
        }
        if (Input.GetKeyUp(KeyCode.V))
        {
            GameObject.FindObjectOfType<CinematicBars>().Hide(.3f);
        }

        // sprinting
        if (trotting && !isSprinting && Input.GetButton("Sprint"))
        {
            isSprinting = true;
            if (!isJumping)
                anim.SetTrigger("start_sprint");
        }
        if (Input.GetButtonUp("Sprint") || moveX == 0 || rb.velocity.x == 0)
        {
            isSprinting = false;
            anim.ResetTrigger("start_sprint");
        }
        anim.SetBool("sprinting", isSprinting);

        if (isSprinting)
        {
            sprintSpeedMultiplier = Mathf.Lerp(sprintSpeedMultiplier, 1.75f, 0.05f);
        }
        else
        {
            sprintSpeedMultiplier = Mathf.Lerp(sprintSpeedMultiplier, 1.0f, 0.5f);
        }
    }

    void FixedUpdate()
    {
        // calculate speed
        calculatedSpeed = speed * Mathf.Min(jumpSpeedMultiplier * sprintSpeedMultiplier, 2.0f);

        // flip sprite depending on direction of input
        if ((moveX < 0 && facingRight) || (moveX > 0 && !facingRight))
        {
            Flip();
        }

        // calculate target velocity
        Vector3 targetVelocity = new Vector2(moveX * calculatedSpeed, rb.velocity.y);

        // sloped movement
        if (isOnSlope && isGrounded && !isJumping && canWalkOnSlope)
        {
            targetVelocity.Set(moveX * calculatedSpeed * -slopeNormalPerp.x, moveX * speed * -slopeNormalPerp.y, 0.0f);
        }

        // apply velocity, dampening between current and target
        if (moveX == 0.0 && rb.velocity.x != 0.0f)
        {
            if (canWalkOnSlope)
                GetComponent<BoxCollider2D>().sharedMaterial = friction;
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing * 2.5f);
        }
        else
        {
            GetComponent<BoxCollider2D>().sharedMaterial = slippery;
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing);
        }

        // calculate speed multiplier for trot animation
        CalculateSpeedMultiplier();

        // check if player is on ground
        CheckGround();

        // rotate player based on slope
        transform.rotation = Quaternion.Euler(0, 0, slopeSideAngle);

        // hold jump distance extentions
        if (isJumping)
        {
            jumpTime += Time.fixedDeltaTime;
            jumpSpeedMultiplier = 1f + 2f/(10f * jumpTime + 4f);
            if (holdingJump)
            {
                jumpSpeedMultiplier *= 1.25f;
                rb.AddForce(new Vector2(0f, jumpForce / 400f / jumpTime));
            }
        }
        else 
        {
            jumpSpeedMultiplier = Mathf.Lerp(jumpSpeedMultiplier, 1, 0.3f);
        }

        // fall detection
        if (beenOnLand >= 0.1f && !isJumping && !isGrounded && !isFalling)
        {
            anim.SetTrigger("fall");
            isFalling = true;
        }
        if (isFalling)
        {
            fallTime += Time.fixedDeltaTime;
            if (isGrounded && fallTime > 0.1f)
            {
                anim.ResetTrigger("fall");
                isFalling = false;
                fallTime = 0.0f;
            }   
        }

        if (!isGrounded)
        {
            beenOnLand = 0f;
        }
        else
        {
            if (beenOnLand < 5f)
                beenOnLand += Time.fixedDeltaTime;
            if (!(rb.velocity.y > 0f) && isJumping)
            {
                jumpSpeedMultiplier = 1f;
                isJumping = false;
                jumpTime = 0f;
            }
        }
    }

    // flips sprite when player changes movement direction
    void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-1 * transform.localScale.x, 1, 1);
    }

    // stops trotting on specific frames if player has released input
    public void StopTrot(int frame)
    {
        if (((moveX == 0 || (moveX != 0 && rb.velocity.x == 0)) && stops <= 2) // if either not giving input or giving input against a barrier *and* hasn't stopped moving more than twice in the last second
            || (stops > 2 && Mathf.Abs(rb.velocity.x) < 0.01f)) // or has stopped moving more than twice in the last second and moving sufficiently slowly
        {
            switch (frame)
            {
                // stop here
                case 0 or 5 or 6 or 11:
                    if (!isJumping)
                        anim.SetTrigger("trot");
                    trotting = false;
                    stepDirection = 1;
                    break;

                // step backwards here
                case 1 or 2 or 7 or 8:
                    stepDirection = -1;
                    break;

                // step forwards here
                case 3 or 4 or 9 or 10:
                    stepDirection = 1;
                    break;
            }
        }
        // step forwards again if still moving
        else
        {
            stepDirection = 1;
        }

        CalculateSpeedMultiplier();
    }

    void CalculateSpeedMultiplier()
    {
        // calculate trot "speed" multiplier
        float speedMultiplier = rb.velocity.x;

        // disregard direction of movement
        speedMultiplier = Mathf.Abs(speedMultiplier);

        // scale + clamp magnitude of normal speed (makes for smoother transitions)
        speedMultiplier = 1.1f * speedMultiplier / 4;
        speedMultiplier = Mathf.Clamp(speedMultiplier, 0.7f, 1.5f);

        // multiply by "step direction" - determines whether animation plays forwards/backwards for smoother stopping
        speedMultiplier *= stepDirection;

        // send speed multiplier to animator parameter
        anim.SetFloat("speed", speedMultiplier);
    }

    private void SlopeCheck()
    {
        Vector2 checkPos = transform.position - new Vector3(0.0f, colliderSize.y / 2);
        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        RaycastHit2D leftHit = Physics2D.Raycast((upperLeftCorner) + (Vector2)transform.position, Vector2.down, slopeCheckDistance + colliderSize.y, whatIsGround);
        RaycastHit2D rightHit = Physics2D.Raycast((upperRightCorner) + (Vector2)transform.position, Vector2.down, slopeCheckDistance + colliderSize.y, whatIsGround);
        
        if (leftHit.point == new Vector2(0, 0) && !onlyRotateWhenGrounded)
        {
            //if (onlyRotateWhenGrounded)
            //    return;
            leftHit.point = upperLeftCorner + (Vector2)transform.position + (Vector2.down * (slopeCheckDistance + colliderSize.y));
            leftHit.distance = (Vector2.Distance(upperLeftCorner + (Vector2)transform.position, leftHit.point));
        }
        if (rightHit.point == new Vector2(0, 0) && !onlyRotateWhenGrounded)
        {
            //if (onlyRotateWhenGrounded)
            //    return;
            rightHit.point = upperRightCorner + (Vector2)transform.position + (Vector2.down * (slopeCheckDistance + colliderSize.y));
            rightHit.distance = (Vector2.Distance(upperRightCorner + (Vector2)transform.position, rightHit.point));
        }

        Debug.DrawLine(upperLeftCorner + (Vector2)transform.position, leftHit.point, Color.red);
        Debug.DrawLine(upperRightCorner + (Vector2)transform.position, rightHit.point, Color.red);

        if (leftHit.distance == rightHit.distance && !onlyRotateWhenGrounded)
        {
            slopeSideAngle = 0;
            return;
        }

        RaycastHit2D farHit = rightHit.distance > leftHit.distance ? rightHit : leftHit;
        RaycastHit2D nearHit = rightHit.distance < leftHit.distance ? rightHit : leftHit;

        int right = leftHit.distance < rightHit.distance ? -1 : 1;
        
        Vector2 acrossCheckSpot = new Vector2(farHit.point.x, nearHit.point.y + (farHit.point.y - nearHit.point.y) / 2);
        Vector2 acrossCheck2 = new Vector2(farHit.point.x, nearHit.point.y + (farHit.point.y - nearHit.point.y) / 4);
        RaycastHit2D across = Physics2D.Raycast(acrossCheckSpot, 
                                                new Vector2(right, 0), Mathf.Abs(upperRightCorner.x - upperLeftCorner.x), whatIsGround);
        RaycastHit2D across2 = Physics2D.Raycast(acrossCheck2,
                                                new Vector2(right, 0), Mathf.Abs(upperRightCorner.x - upperLeftCorner.x), whatIsGround);
        Debug.DrawLine(across.point, acrossCheckSpot, Color.green);
        Debug.DrawLine(across2.point, acrossCheck2, Color.green);

        float unsmoothedSlope = Mathf.Atan((rightHit.point.y - leftHit.point.y)/(rightHit.point.x - leftHit.point.x)) * Mathf.Rad2Deg;
        float acrossPercent = across.distance / (Mathf.Abs(upperRightCorner.x - upperLeftCorner.x));
        float acrossPercent2 = across2.distance / (Mathf.Abs(upperRightCorner.x - upperLeftCorner.x));


        bool onLedge = false;
        //Makessure that it is not reading the slope of the underside of a slope by not taking abs val. 
        if (acrossPercent2 - acrossPercent < .01)//If issues arise get abs value
        {
            slopeSideAngle = 0;
            onLedge = true;
            if (acrossPercent != 0 && acrossPercent2 != 0 && (!onlyRotateWhenGrounded /*|| isGrounded*/))
                return;
        }
        if(!float.IsNaN(unsmoothedSlope) && !onLedge)
            slopeSideAngle = unsmoothedSlope * Mathf.Lerp(1, 0, (Mathf.Abs((acrossPercent/.5f) - 1)));

        if (!isGrounded)
        {
            const float ROTATION_INTENSITY = 75;
            int negative = 1;
            if (!facingRight)
                negative = -1;
            float rotationAmount = (rb.velocity.y * Time.deltaTime * ROTATION_INTENSITY * negative);
            rotationAmount = Mathf.Clamp(rotationAmount, -75, 75);
            slopeSideAngle = lastGroundedSlope + rotationAmount;
        }
        if (isGrounded)
            lastGroundedSlope = slopeSideAngle;
        else
        {
            lastMidairVelocity = rb.velocity;
            lastUngroundedSlope = slopeSideAngle;
        }
        if (isGrounded)
        {
            if(lastLand + landAnimTime > Time.time)
            {
                //float adjustedRotationTime = landAnimTime/lastMidairVelocity.y;
                slopeSideAngle = Mathf.Lerp(lastUngroundedSlope, slopeSideAngle, Mathf.Clamp((Time.time - lastLand)* Mathf.Abs(lastMidairVelocity.y) / (landAnimTime), 0, 1));
            }
        }

    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        anim.SetBool("ground_close", false);
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, whatIsGround);
        if (hit)
        {
            anim.SetBool("ground_close", true);
            // Debug.DrawRay(hit.point, hit.normal, Color.red, 0.01f, false);
            slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            if (slopeDownAngle != slopeDownAngleOld)
            {
                isOnSlope = true;
            }

            slopeDownAngleOld = slopeDownAngle;
            // Debug.DrawRay(hit.point, slopeNormalPerp, Color.yellow, 0.01f, false);
        }

        if (slopeDownAngle > maxSlopeAngle || slopeSideAngle > maxSlopeAngle)
        {
            canWalkOnSlope = false;
        }
        else
        {
            canWalkOnSlope = true;
        }
    }

    void CheckGround()
    {
        SlopeCheck();

        lastOnLand = Mathf.Clamp(lastOnLand + Time.fixedDeltaTime, 0, 20f);

        bool wasGrounded = isGrounded;
        isGrounded = false;
        foreach(Collider2D collision in groundCheck.triggersInContact)
        {
            //Debug.Log(LayerMask.GetMask("Terrain"));
            if(Mathf.Pow(2, collision.gameObject.layer) == whatIsGround)
            {
                anim.SetBool("grounded", true);
                isGrounded = true;
                lastOnLand = 0f;
                break;
            }
        }
        if ((isJumping && jumpTime < 0.1f) || (isFalling && fallTime < 0.1f))
            anim.SetBool("grounded", false);
        else
            anim.SetBool("grounded", isGrounded);

        anim.SetBool("jump", isJumping);
    }

    void Jump()
    {
        // if player presses jump button
        if (Input.GetButtonDown("Jump"))
        {
            timeSinceJumpPressed = 0.0f;
        }

        if (Input.GetButton("Jump") && timeSinceJumpPressed < 0.2f)
        {
            if (!isJumping)
            {
                holdingJump = true;
            }
        }

        if (timeSinceJumpPressed < 1f)
            timeSinceJumpPressed += Time.deltaTime;

        // incorporates coyote time and input buffering
        if (timeSinceJumpPressed < 0.2f && (isGrounded || lastOnLand < 0.2f) && !isJumping)
        {
            if (isOnSlope && slopeDownAngle > maxSlopeAngle)
                return;
            
            // Add a vertical force to the player
            isGrounded = false;
            isJumping = true;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(new Vector2(0f, jumpForce)); // force added during a jump
            anim.SetTrigger("start_jump");
            GetComponentInChildren<SoundPlayer>()?.PlaySound(0);
        }        
    }
}