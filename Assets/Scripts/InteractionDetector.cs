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
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();

        if (context.started)
        {
            if (isInKitchen && player != null && player.GetMovement().magnitude < 0.1f)
            {
                Food food = player.GetCarriedFood();
                if (food != null && !food.isSalted && !food.isMouseAdded) // ✅ kiểm tra trước
                {
                    isHoldingSalt = true;
                    interactionIcon.SetActive(true);
                    interactionAnimator.SetBool("isAddingSalt", true);
                    saltHoldTimer = 0f;

                    // 🔥 Player chạy anim adding salt
                    player.GetComponent<Animator>().SetBool("isAddingSalt", true);

                    // 🔊 Play clocktick
                    AudioManager.Instance.playplayerSFX("ClockTick");
                }
                else
                {
                    Debug.Log("Food already salted or has mouse, cannot add salt!");
                }
            }
            else
            {
                Debug.Log("Not in kitchen or not standing still, cannot add salt!");
            }
        }
        if (context.canceled)
        {
            isHoldingSalt = false;
            interactionIcon.SetActive(false);
            interactionAnimator.SetBool("isAddingSalt", false);
            saltHoldTimer = 0f;

            if (player != null)
                player.GetComponent<Animator>().SetBool("isAddingSalt", false);

            // 🔊 Stop clocktick
            AudioManager.Instance.stopplayerSFX("ClockTick");
        }
    }

    public void OnAddMouse(InputAction.CallbackContext context)
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();

        if (context.started)
        {
            if (isInKitchen && player != null && player.GetMovement().magnitude < 0.1f)
            {
                Food food = player.GetCarriedFood();
                if (food != null && !food.isMouseAdded && !food.isSalted) // ✅ kiểm tra trước
                {
                    isHoldingMouse = true;
                    interactionIcon.SetActive(true);
                    interactionAnimator.SetBool("isAddingMouse", true);
                    mouseHoldTimer = 0f;

                    // 🔥 Player chạy anim adding mouse
                    player.GetComponent<Animator>().SetBool("isAddingMouse", true);

                    // 🔊 Play clocktick
                    AudioManager.Instance.playplayerSFX("ClockTick");
                }
                else
                {
                    Debug.Log("Food already has mouse or is salted, cannot add mouse!");
                }
            }
            else
            {
                Debug.Log("Not in kitchen or not standing still, cannot add mouse!");
            }
        }
        if (context.canceled)
        {
            isHoldingMouse = false;
            interactionIcon.SetActive(false);
            interactionAnimator.SetBool("isAddingMouse", false);
            mouseHoldTimer = 0f;

            if (player != null)
                player.GetComponent<Animator>().SetBool("isAddingMouse", false);

            // 🔊 Stop clocktick
            AudioManager.Instance.stopplayerSFX("ClockTick");
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
