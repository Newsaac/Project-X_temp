using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private float horizontal;
    private float vertical;
    
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    private bool isWallSliding;
    private bool isWallJumping;
    private Vector3 wallJumpingDirection;
    private Vector3 wallNormalSum;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.2f;

    

    private Vector3 jointOriginalPos;
    private float timer = 0;
    private bool isWalking = false;
    private bool isSprinting = false;
    
    public Transform joint;

    [Header("General Movement")]
    [SerializeField] Camera playerCamera;
    [SerializeField] float walkSpeed = 6f;
    [SerializeField] float runSpeed = 12f;
    [SerializeField] float jumpPower = 7f;
    [SerializeField] float gravity = 10f;

    [Header("Camera Movement")]
    [SerializeField] float lookSpeed = 2f;
    [SerializeField] float lookXLimit = 45f;
    [Space(10)]
    [SerializeField] float bobSpeed = 10f;
    [SerializeField] float sprintBobIncrease = 3f;
    [SerializeField] Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    [Header("Wall Jumping")]
    [SerializeField] float wallSlidingSpeed = 0.5f;
    [SerializeField] Vector3 wallJumpingPower = new(8f, 8f, 8f);
    [SerializeField] LayerMask wallLayer;
    [SerializeField] Transform wallCheck;

    CharacterController characterController;
    GameManager gameManager;
    GameSettings settings;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        jointOriginalPos = joint.localPosition;
    }

    void Start() {
        settings = gameManager.settings;
    }

    void Update() {
        if (gameManager.gameOver)
            return;

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        isSprinting = Input.GetKey(settings.sprint) && characterController.isGrounded;
        isWalking = (horizontal != 0 || vertical != 0) && characterController.isGrounded;

        // Press Left Shift to run
        if (!isWallJumping) {
            #region Handles Movement

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            bool isRunning = Input.GetKey(settings.sprint);
            float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * vertical : 0;
            float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * horizontal : 0;
            float movementDirectionY = moveDirection.y;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            #endregion

            #region Handles Jumping
            if (!isWallJumping) {
                if (Input.GetKey(settings.jump) && canMove && characterController.isGrounded) {
                    moveDirection.y = jumpPower;
                }
                else {
                    moveDirection.y = movementDirectionY;
                }
            }
        }

        if (!characterController.isGrounded) {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        #endregion

        #region Handles Rotation

        if (canMove) {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed * settings.sensitivity;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed * settings.sensitivity, 0);
        }

        #endregion

        WallSlide();
        WallJump();
        HeadBob();

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HeadBob() {
        if (isWalking) {
            // Calculates HeadBob speed during sprint
            if (isSprinting) {
                timer += Time.deltaTime * (bobSpeed + sprintBobIncrease);
            }
            // Calculates HeadBob speed during walking
            else {
                timer += Time.deltaTime * bobSpeed;
            }
            // Applies HeadBob movement
            joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x, jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y, jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
        }
        else {
            // Resets when play stops moving
            timer = 0;
            joint.localPosition = new Vector3(Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
        }
    }

    private bool IsWalled() {
        Vector3 point0 = wallCheck.position; point0.y += characterController.height / 2 - characterController.radius;
        Vector3 point1 = wallCheck.position; point1.y -= characterController.height / 2 + characterController.radius;
        Collider[] colliders = Physics.OverlapCapsule(point0, point1, characterController.radius + characterController.skinWidth, wallLayer);
        int wallCnt = 0;
        wallNormalSum = Vector3.zero;

        foreach (Collider collider in colliders) {
            if(collider.gameObject.tag == "Wall") {
                wallCnt++;
                Vector3 closestPoint = Physics.ClosestPoint(wallCheck.position, collider, collider.transform.position, collider.transform.rotation);
                Vector3 wallNormal = (wallCheck.position - closestPoint).normalized;
                wallNormalSum += wallNormal;
            }
        }
        wallNormalSum.Normalize();
        return wallCnt > 0;
    }

    private void WallSlide() {
        if (IsWalled() && !characterController.isGrounded && (horizontal != 0f || vertical != 0f)) {
            isWallSliding = true;
            moveDirection = new Vector3(moveDirection.x, Mathf.Clamp(moveDirection.y, -wallSlidingSpeed, float.MaxValue), moveDirection.z);
        }
        else {
            isWallSliding = false;
        }
    }

    private void WallJump() {
        if(isWallSliding) {
            isWallJumping = false;
            wallJumpingCounter = wallJumpingTime;

            wallJumpingDirection = (transform.TransformDirection(Vector3.forward).normalized + wallNormalSum).normalized;

            CancelInvoke(nameof(StopWallJumping));
        }
        else {
            wallJumpingCounter -= Time.deltaTime;
        }

        if(Input.GetButton("Jump") && wallJumpingCounter > 0f) {
            isWallJumping = true;
            moveDirection = new Vector3(wallJumpingDirection.x * wallJumpingPower.x, wallJumpingPower.y, wallJumpingDirection.z * wallJumpingPower.z);
            wallJumpingCounter = 0f;

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping() {
        isWallJumping = false;
    }
}
