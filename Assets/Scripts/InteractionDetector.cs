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

    private bool isInKitchen = false; // ✅ biến kiểm tra có trong kitchen không

    void Start()
    {
        interactionIcon.SetActive(false); // Ẩn biểu tượng tương tác ban đầu
    }

    void Update()
    {
        if (isHoldingSalt && isInKitchen) // ✅ chỉ thực hiện nếu đang trong kitchen
        {
            saltHoldTimer += Time.deltaTime;
            Debug.Log($"Holding salt... {saltHoldTimer}");
            if (saltHoldTimer >= saltHoldTimeRequired)
            {
                AddSaltToFood();
                isHoldingSalt = false;
                saltHoldTimer = 0f;
            }
        }

        if (isHoldingMouse && isInKitchen) // ✅ chỉ thực hiện nếu đang trong kitchen
        {
            mouseHoldTimer += Time.deltaTime;
            Debug.Log($"Holding mouse... {mouseHoldTimer}");
            if (mouseHoldTimer >= mouseHoldTimeRequired)
            {
                AddMouseToFood();
                isHoldingMouse = false;
                mouseHoldTimer = 0f;
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
            Debug.Log("Entered kitchen area");
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
            Debug.Log("Exited kitchen area");
        }
    }
}
