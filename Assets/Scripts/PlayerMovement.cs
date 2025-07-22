using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // tốc độ di chuyển
    private Vector2 movement;

    private Rigidbody2D rb;
    private Animator animator;

    private bool isCarryingFood = false; // Trạng thái cầm món ăn
    private Food carriedFood;

    // Thuộc tính nêm muối
    public float saltHoldTimeRequired = 3f;
    private float saltHoldTimer = 0f;
    private bool isHoldingSalt = false;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
       
    }

    public void Move(InputAction.CallbackContext context)
    {
        // ✅ Nếu game pause thì không nhận input
        if (PauseController.IsGamePause)
        {
            movement = Vector2.zero;
            animator.SetBool("isWalking", false);
            AudioManager.Instance.stopplayerSFX("Footsteps");
            return;
        }

        animator.SetBool("isWalking", true);

        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", movement.x);
            animator.SetFloat("LastInputY", movement.y);

            AudioManager.Instance.stopplayerSFX("Footsteps");
        }

        movement = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", movement.x);
        animator.SetFloat("InputY", movement.y);

        if (movement.magnitude > 0.1f)
        {
            if (!AudioManager.Instance.playerSFXSource.isPlaying)
            {
                AudioManager.Instance.playplayerSFX("Footsteps");
            }
        }
        else
        {
            if (AudioManager.Instance.playerSFXSource.isPlaying)
            {
                AudioManager.Instance.stopplayerSFX("Footsteps");
            }
        }
    }

    void FixedUpdate()
    {
        if (PauseController.IsGamePause)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = movement * moveSpeed;
    }

    public void SetCarryingFood(bool isCarrying, Food food = null)
    {
        isCarryingFood = isCarrying;
        carriedFood = food;
        //animator.SetBool("isBringFood", isCarryingFood);
        //if (isCarrying && food != null)
        //{
        //    // Khởi tạo vị trí món ăn dựa trên hướng hiện tại
        //    food.UpdatePosition(new Vector2(animator.GetFloat("LastInputX"), animator.GetFloat("LastInputY")));
        //}
    }

    public Food GetCarriedFood()
    {
        return carriedFood;
    }

    public Vector2 GetMovement()
    {
        return movement;
    }
}
