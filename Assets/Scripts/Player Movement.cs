using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed;
    public float walkSpeed;
    public float backwardSpeed;

    private float sprintSpeed = 12; //sprint speed
    private float maxSprintDuration = 6f; //how long can they sprint for
    private float sprintDuration; //how long the play has left to sprint for
    private float sprintDurationCD = 0; 
    private float sprintCDFull = 1.5f; //length before regaining stamina
    private float sprintMultiplier = 1; //multiplier to recover stamina
    private float sprintWait = 4f; //wait until they can sprint if they used ^all^ their sprint
    private bool canSprint = true;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    public float airTime;
    private float airDuration;
    public float fallSpeed;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    private bool isCrouched = false;
    private float crouchCooldown = 0;
    private bool canCrouch = true;
    private bool buttonReleased = true;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        backwards,
        sprinting,
        crouching,
        air
    }

    bool wasGrounded;
    bool wasFalling;
    float startOfFall;
    bool isFalling { get { return (!grounded && rb.velocity.y < 0); } }

    private void Update()
    {
        //check if the player has gone under a certain y limit, and if so teleport them back to where the player normally spawns
        if (gameObject.transform.position.y <= -10) gameObject.transform.position = new Vector3(-326,21,-319);

        groundCheck();

        MyInput();
        SpeedControl();
        StateHandler();

            if (grounded)
            {
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = 0;

                if (airDuration > 0)
                {
                    airDuration -= Time.deltaTime;
                }
                else
                {
                    //apply a force downwards to simulate gravity 
                    rb.AddForce(Vector3.down * fallSpeed);
                }
            }

            if (state != MovementState.sprinting || (state == MovementState.air && !Input.GetKey(sprintKey)))
            {
                sprintDurationCD += Time.deltaTime; //increment the recovery timer
                if (sprintDurationCD >= sprintCDFull && sprintDuration < maxSprintDuration) //if the recovery length is hit or exceed AND we have sprint to recover, do so
                {
                    sprintDuration += (Time.deltaTime * sprintMultiplier); //recover sprint
                    if(sprintDuration > maxSprintDuration) sprintDuration = maxSprintDuration; 
                    if(sprintDurationCD >= sprintWait) //set them able to sprint once the recovery hits the waiting time
                    {
                        canSprint = true;
                    }
                }
            }

            if (state == MovementState.sprinting) 
            {
                sprintDurationCD = 0; //reset the timer to recover
                sprintDuration -= (Time.deltaTime * 0.8f);  //take away stamina at a fixed rate
            }

            if(sprintDuration <= 0)
            {
                canSprint = false; //if stamina runs out then the player can't sprint
                sprintDuration = 0;
            }

            if(crouchCooldown > 0)
            {
                crouchCooldown -= Time.deltaTime;
                if(crouchCooldown <= 0)
                {
                    canCrouch = true;
                }
            }
    }

    private void FixedUpdate()
    {
        groundCheck();

        MovePlayer();

            if (!wasFalling && isFalling)
            {
                startOfFall = transform.position.y;
            }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        sprintDuration = maxSprintDuration;

        startYScale = transform.localScale.y;
    }

    private void groundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);


    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (PlayerPrefs.GetString("Toggle Crouch") == "False")
        {
            if (Input.GetKeyDown(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                state = MovementState.crouching;
            }

            if (Input.GetKeyUp(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }
        else if (PlayerPrefs.GetString("Toggle Crouch") == "True" && buttonReleased)
        {
            if (!isCrouched)
            {
                if (Input.GetKey(crouchKey) && canCrouch)
                {
                    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                    rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                    state = MovementState.crouching;
                    isCrouched = true;
                    canCrouch = false;
                    crouchCooldown = 0.35f;
                    buttonReleased = false;
                }
            }
            else
            {
                if (Input.GetKey(crouchKey) && canCrouch)
                {
                    transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                    isCrouched = false;
                    canCrouch = false;
                    crouchCooldown = 0.35f;
                    buttonReleased = false;
                }
            }
        }
        else if (PlayerPrefs.GetString("Toggle Crouch") == "True" && !buttonReleased)
        {
            if (Input.GetKeyUp(crouchKey))
            {
                buttonReleased = true;
            }
        }
    }

    private void StateHandler()
    {
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
        }

        if(grounded && Input.GetKey(sprintKey) && !CheckMovementDirection() && sprintDuration > 0 && canSprint && !isCrouched)
        {
            if (rb.velocity.x > 0.5 || rb.velocity.z > 0.5 || rb.velocity.x < -0.5 || rb.velocity.z < -0.5)
            {
                state = MovementState.sprinting;
                moveSpeed = sprintSpeed;
            }
            else if (grounded)
            {
                    if (!CheckMovementDirection())
                    {
                        moveSpeed = walkSpeed;
                        state = MovementState.walking;
                    }
                    else
                    {
                        moveSpeed = backwardSpeed;
                        state = MovementState.backwards;
                    }
            }
            else
            {
                state = MovementState.air;
            }
        }
        else if (grounded)
        {
            if (!CheckMovementDirection())
            {
                moveSpeed = walkSpeed;
                state = MovementState.walking;
            }
            else
            {
                moveSpeed = backwardSpeed;
                state = MovementState.backwards;
            }
        }
        else 
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeDirection() * moveSpeed * 20f, ForceMode.Force);

            if(rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        airDuration = airTime;
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private bool CheckMovementDirection() //--> returns true if moving forwards and false if backwards
    {
        if(Input.GetAxisRaw("Vertical") < 0)
        {
            return true;
        }
        return false;
    }

    //setter for the sprint settings (used by HUD Controller when gaining Anabolic Endurance (perk))
    public void setSprintSettings(float speed, float duration, float cooldown, float multplier, float wait)
    {
        sprintSpeed = speed; //how fast cam the player sprint
        maxSprintDuration = duration; //how long can the player sprint
        sprintCDFull = cooldown; //how long does it take to start recovering stamina
        sprintMultiplier = multplier; //multiplier for the recovery of stamina
        sprintWait = wait; //how long it takes for the player to sprint after they use all stamina
    }
}
