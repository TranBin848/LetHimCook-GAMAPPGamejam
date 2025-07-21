using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon; // Biểu tượng tương tác
    public Animator interactionAnimator;

    private float saltHoldTimer = 0f;
    public float saltHoldTimeRequired = 3f;
    private bool isHoldingSalt = false;

    private float mouseHoldTimer = 0f;
    public float mouseHoldTimeRequired = 3f;
    private bool isHoldingMouse = false;

    private float angerCooldown = 0f;
    public float angerCooldownDuration = 2f; // chỉ tăng giận dữ mỗi 1 giây

    private bool isInKitchen = false; // ✅ biến kiểm tra có trong kitchen không
    private bool isInChefVision = false; // ✅ biến kiểm tra có trong ChefVision không

    public Chef chef;

    void Start()
    {
        chef = FindFirstObjectByType<Chef>();
        interactionIcon.SetActive(false); // Ẩn biểu tượng tương tác ban đầu
    }

    void Update()
    {
        if (angerCooldown > 0)
            angerCooldown -= Time.deltaTime;

        if (isHoldingSalt && isInKitchen)
        {
            saltHoldTimer += Time.deltaTime;
            if (saltHoldTimer >= saltHoldTimeRequired)
            {
                AddSaltToFood();
                isHoldingSalt = false;
                saltHoldTimer = 0f;
            }
        }

        if (isHoldingMouse && isInKitchen)
        {
            mouseHoldTimer += Time.deltaTime;
            if (mouseHoldTimer >= mouseHoldTimeRequired)
            {
                AddMouseToFood();
                isHoldingMouse = false;
                mouseHoldTimer = 0f;
            }
        }

        if (isInChefVision && (isHoldingSalt || isHoldingMouse))
        {
            if (angerCooldown <= 0f)
            {
                interactionIcon.SetActive(true);
                GameManager.Instance.IncreaseAnger(GameManager.Instance.angerIncreaseRate);
                angerCooldown = angerCooldownDuration;

                // ✅ Gọi chef set animation
                if (chef != null)
                {
                    if (isHoldingSalt)
                    {
                        chef.SetAngrySalt(true);
                        chef.SetAngryMouse(false);
                    }
                    else if (isHoldingMouse)
                    {
                        chef.SetAngryMouse(true);
                        chef.SetAngrySalt(false);
                    }
                }
            }
        }
        else
        {
            // ✅ Reset animation khi không bị phát hiện nữa
            if (chef != null)
            {
                chef.SetAngryMouse(false);
                chef.SetAngrySalt(false);
            }
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactableInRange?.Interact();
        }
    }

    public void OnAddSalt(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isInKitchen) // ✅ chỉ bắt đầu khi trong kitchen
            {
                isHoldingSalt = true;
                interactionIcon.SetActive(true);
                interactionAnimator.SetBool("isAddingSalt", true);
                saltHoldTimer = 0f;
            }
            else
            {
                Debug.Log("Not in kitchen, cannot add salt!");
            }
        }
        if (context.canceled)
        {
            isHoldingSalt = false;
            interactionIcon.SetActive(false);
            interactionAnimator.SetBool("isAddingSalt", false);
            saltHoldTimer = 0f;
        }
    }

    public void OnAddMouse(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isInKitchen) // ✅ chỉ bắt đầu khi trong kitchen
            {
                isHoldingMouse = true;
                interactionIcon.SetActive(true);
                interactionAnimator.SetBool("isAddingMouse", true);
                mouseHoldTimer = 0f;
            }
            else
            {
                Debug.Log("Not in kitchen, cannot add mouse!");
            }
        }
        if (context.canceled)
        {
            isHoldingMouse = false;
            interactionIcon.SetActive(false);
            interactionAnimator.SetBool("isAddingMouse", false);
            mouseHoldTimer = 0f;
        }
    }

    private void AddSaltToFood()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            Debug.Log("Player found");
            Food food = player.GetCarriedFood();
            if (food != null)
            {
                Debug.Log("Food found, adding salt");
                if (!food.isSalted && !food.isMouseAdded)
                    food.AddSalt();
            }
            else
            {
                Debug.Log("Player is not carrying any food");
            }
        }
    }

    private void AddMouseToFood()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            Debug.Log("Player found");
            Food food = player.GetCarriedFood();
            if (food != null)
            {
                Debug.Log("Food found, adding mouse");
                if (!food.isMouseAdded && !food.isSalted)
                    food.AddMouse();
            }
            else
            {
                Debug.Log("Player is not carrying any food");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"OnTriggerEnter2D with {collision.gameObject.name}");
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.canInteract())
        {
            interactableInRange = interactable;
        }

        // ✅ kiểm tra nếu vào vùng kitchen
        if (collision.CompareTag("Kitchen"))
        {
            isInKitchen = true;
        }

        // ✅ kiểm tra nếu vào vùng ChefVision khi đang cầm muối hoặc chuột
        if (collision.CompareTag("ChefVision"))
        {
            isInChefVision = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }

        // ✅ kiểm tra nếu rời khỏi vùng kitchen
        if (collision.CompareTag("Kitchen"))
        {
            isInKitchen = false;
        }
        if (collision.CompareTag("ChefVision"))
        {
            isInChefVision = false;
            Debug.Log("Exited ChefVision area");
        }
    }
}
