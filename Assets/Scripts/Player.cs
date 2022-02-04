using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public bool isGrounded;
    public bool isSprinting;

    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float playerWidth = 0.15f; // Radius of the capsule

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum;
    private bool jumpRequest;

    private void Start() {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
    }

    private void FixedUpdate() {
        CalculateVelocity();

        // This is like this because sometimes the fixedUpdate misses the jump key.
        // This way it might not get to it immediately but it will get there eventually.
        if (jumpRequest)
            Jump();

        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.Rotate(Vector3.right * -mouseVertical);
        transform.Translate(velocity, Space.World);

    }

    private void Update() {
        GetPlayerInputs();
    }

    private void CalculateVelocity() {
        // Affect vertical momentum with gravity
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // If we're sprinting, use the sprint multiplier
        if (isSprinting)
            velocity = 
                ((transform.forward * vertical) +              // "vertical" here means "forward/backward"
                (transform.right * horizontal)).normalized *   // Add .normalized to your movement so that you don't move faster diagonally
                Time.fixedDeltaTime  * sprintSpeed;
        else
            velocity = 
                ((transform.forward * vertical) +
                (transform.right * horizontal)).normalized * 
                Time.fixedDeltaTime  * walkSpeed;

        // Apply vertical momentum (falling/jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        // if there's even a slight drift in the z axis (back and forward) and there's something in the way
        if ((velocity.z > 0 && front) || (velocity.z > 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x > 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);

    }

    private void GetPlayerInputs() {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;
    }

    // We check if, once the downSpeed is applied, the player would be in a solid voxel (i.e. if the player would be in the ground).
    // However, we can't just rely on the single voxel below the player because the player could be on top of the intersection of where 4 voxels meet,
    // i.e. on the "X" here:
    //  +-+-+
    //  | | |
    //  +-X-+
    //  | | |
    //  +-+-+
    // Therefore, we need to check if any of those 4 voxels is solid and can sustain the player.
    // If the player is in the air, return the same downSpeed and modify isGrounded to be false;
    // otherwise, set isGrounded to true and return 0 (because the player isn't falling anymore).
    // The (!left && !back) fix a bug where the player can hang onto walls because the checkDownSpeed(float) 
    // and checkUpSpeed(float) functions are accounting for the 4 possible blocks below it without 
    // even checking if those 4 blocks would be accessible to the player's feet (like if there is a
    // block on top of those blocks or not)
    private float checkDownSpeed(float downSpeed) {
        if (
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) && (!left && !back)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) && (!right && !back)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) && (!right && !front)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) && (!left && !front))
        ) {
            isGrounded = true;
            return 0;
        } else {
            isGrounded = false;
            return downSpeed;
        }
    }

    // see the documentation for checkDownSpeed; it's pretty much the reverse of it.
    // "+ 2f" because we are taking into account the player height (the collision detection is a little bit above the head) 
    private float checkUpSpeed(float upSpeed) {
        if (
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) && (!left && !back)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) && (!right && !back)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) && (!right && !front)) ||
            (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) && (!left && !front))
        ) {
            return 0;
        } else {
            return upSpeed;
        }
    }

    void Jump() {
        verticalMomentum = jumpForce; // Effectively turns verticalMomentum into a positive number
        isGrounded = false;
        jumpRequest = false;
    }

    // 2 checks because the player is 2 blocks high. We not only check the block in front of the player's feet, but also the one on top of it.
    public bool front {
        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
            )
                return true;
            else
                return false;
        }
    }

    public bool back {
        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
            )
                return true;
            else
                return false;
        }
    }
    
    public bool left {
        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
            )
                return true;
            else
                return false;
        }
    }

    public bool right {
        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
            )
                return true;
            else
                return false;
        }
    }

}
