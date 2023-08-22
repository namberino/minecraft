using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;

    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float playerWidth = 0.15f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    public Transform highlightBlock;
    public Transform placeBlock; // change the PlaceHighlightBlock's Cube's rendering mode to transparent to see the place block highlight
    public float checkIncrement = 0.01f;
    public float reach = 8f;

    public byte selectedBlockIndex = 1;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        //if (!world.inUI)
        //{
        CalculateVelocity();
        if (jumpRequest)
            Jump();

        // update player view and walking direction 
        transform.Rotate(Vector3.up * mouseHorizontal * world.settings.mouseSensitivity);
        cam.Rotate(Vector3.right * -mouseVertical * world.settings.mouseSensitivity); // rotate around the right axis
        transform.Translate(velocity, Space.World);
        //}
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    world.inUI = !world.inUI;
        //}

        //if (!world.inUI)
        //{
        GetPlayerInputs();
        PlaceCursorBlock();
        //}

        //GetPlayerInputs();
        //PlaceCursorBlock();
    }

    // calculate the vertical momentum and based on the jump force
    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;

    }

    // a method for calculating the velocity of the player
    private void CalculateVelocity()
    {
        // affect vertical momentum with gravity
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // if sprinting, use sprint multiplier
        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        // apply vertical momentum (falling / jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = CheckDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = CheckUpSpeed(velocity.y);
    }

    // getting the inputs of the player
    private void GetPlayerInputs()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //    Application.Quit();

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        // sprint button is set to shift
        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        // for jumping
        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;

        if (highlightBlock.gameObject.activeSelf)
        {
            // destroy block
            if (Input.GetMouseButtonDown(0))
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);

            // place block
            if (Input.GetMouseButtonDown(1))
                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, selectedBlockIndex);
        }
    }

    // this method is for placing the block in front of the player within a certain range
    private void PlaceCursorBlock()
    {
        float step = checkIncrement; // this is how far the player's cursor is to the block that it is pointing towards
        Vector3 lastPos = new Vector3();

        // only allow placing block when the player is in range
        while (step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);

            if (world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    // VERTICAL COLLISION DETECTION

    // downwards collision detection
    private float CheckDownSpeed(float downSpeed)
    {
        // applying the velocity leaves them in the ground, return 0;
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
           )
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    // upwards collision detection
    private float CheckUpSpeed(float upSpeed)
    {
        // applying the velocity leaves them in a solid block, return 0;
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) && (!left && !back) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) && (!right && !back) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) && (!right && !front) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) && (!left && !front)
           )
        {
            verticalMomentum = 0; // set to 0 when player's head hit a block while jumping
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    // HORIZONTAL COLLISION DETECTION

    // check the 2 blocks in front of the player
    public bool front
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
               )
                return true;
            else
                return false;
        }
    }


    // check the 2 blocks behind the player
    public bool back
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
               )
                return true;
            else
                return false;
        }
    }

    // check the 2 blocks to the left of the player
    public bool left
    {
        get
        {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
               )
                return true;
            else
                return false;
        }
    }

    // check the 2 blocks to the right of the player
    public bool right
    {
        get
        {
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


