using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Customer : MonoBehaviour, IInteractable
{
    public string customerID { get; private set; }

    public float moveSpeed = 2f;
    public Transform chairTarget; // ghế mà khách sẽ ngồi
    public Transform spawnPoint;
    public Chair chairScript;
    public MenuList menuList;
    public DishData orderedDish;
    public string orderID;
    public GameObject[] speechBubblePrefabs; // Các prefab khung chat
    public float orderTimeLimit; // Thời gian chờ gọi món
    public float prepTimeLimit; // Thời gian chờ chuẩn bị món
    public float speechBubbleOffsetY = 1.5f;
    private GameObject currentSpeechBubble; // Khung chat hiện tại
    private float orderTimer;
    public Sprite[] dishSprites;
    private int dishIndex;
    //private bool isMoving = false;
    private bool isSitting = false;
    private bool hasOrdered = false;

    NavMeshAgent agent;
    private Animator animator;

    public GameObject interactionIcon; // Biểu tượng tương tác
    public Animator interactionAnimator; // Animator cho biểu tượng tương tác

    public bool leaveAngry = false;
    void Start()
    {
        interactionIcon.SetActive(false); // Ẩn biểu tượng tương tác ban đầu
        customerID ??= GlobalHelper.GenerateUniqueId(gameObject); // Tạo ID duy nhất nếu chưa có
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.updateRotation = false; // Tắt cập nhật xoay của NavMeshAgent
        agent.updateUpAxis = false; // Tắt cập nhật trục Y của NavMeshAgent
        if (chairTarget != null)
        {
            agent.SetDestination(chairTarget.position); // Đặt điểm đến là ghế
        }
    }

    private void Update()
    {
        if (PauseController.IsGamePause)
        {
            agent.isStopped = true;
            animator.SetBool("isMoving", false);
            return;
        }

        agent.isStopped = false;

        // Cập nhật animation khi di chuyển
        Vector3 velocity = agent.velocity;
        if (velocity.magnitude > 0.1f)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
        animator.SetFloat("moveX", velocity.normalized.x);
        animator.SetFloat("moveY", velocity.normalized.y);

        // Kiểm tra khi đến ghế
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isSitting)
        {
            SitDown();
        }

        // Đếm thời gian chờ nhận món
        if (hasOrdered && orderTimer > 0 && isSitting)
        {
            orderTimer -= Time.deltaTime;
            if (orderTimer <= 0)
            {
                OnOrderTimeout();
            }
        }
    }

    void OrderFood()
    {
        hasOrdered = true;
        orderTimer = orderTimeLimit;
        // Hiển thị khung chat
        dishIndex = Random.Range(0, 3); // Random từ 0 đến 2
        Debug.Log(dishIndex);
        orderedDish = menuList.dishes[dishIndex];
        currentSpeechBubble = Instantiate(speechBubblePrefabs[dishIndex], transform.position + new Vector3(0, speechBubbleOffsetY, 0), Quaternion.identity);
        currentSpeechBubble.transform.SetParent(transform); // Gắn khung chat vào khách để di chuyển cùng
        AudioManager.Instance.playcustomerSFX("Bubble"); // Phát âm thanh khi khách gọi món
    }

    void PrepFood()
    {
        // Hiển thị khung chat
        Destroy(currentSpeechBubble); // Xóa khung chat cũ
        currentSpeechBubble = Instantiate(speechBubblePrefabs[dishIndex + 3], transform.position + new Vector3(0, speechBubbleOffsetY, 0), Quaternion.identity);
        AudioManager.Instance.playcustomerSFX("Bubble"); // Phát âm thanh khi khách gọi món
    }

    public void InteractWithOrder()
    {
        Debug.Log("Check");
        if (hasOrdered && orderTimer > 0)
        {
            hasOrdered = false;
            orderID = OrderManager.Instance.AddOrder(orderedDish, prepTimeLimit, this);
            AudioManager.Instance.playcustomerSFX("Interact"); // Phát âm thanh khi khách gọi món
            PrepFood();
            // TODO: Thêm logic để xử lý đơn hàng (như thêm vào danh sách nhiệm vụ người chơi)
        }
    }


    void OnOrderTimeout()
    {
        if (hasOrdered)
        {
            Debug.Log(gameObject.name + " hết thời gian chờ, khách phàn nàn!");
            hasOrdered = false;
            if (currentSpeechBubble != null)
            {
                Destroy(currentSpeechBubble);
            }
            GameManager.Instance.IncreaseAnger(GameManager.Instance.angerIncreaseRate); // Tăng giận dữ
            GameManager.Instance.DecreaseReputation(GameManager.Instance.reputationDecreaseOnTimeout); // Giảm danh tiếng quán
            StartCoroutine(LeaveAngryRestaurant(1));
            // TODO: Giảm danh tiếng quán (cần tích hợp với hệ thống danh tiếng)
        }
    }

    public void OnPrepTimeout()
    {
        GameManager.Instance.IncreaseAnger(GameManager.Instance.angerIncreaseRate);
        GameManager.Instance.DecreaseReputation(GameManager.Instance.reputationDecreaseOnOrderFailure); // Giảm danh tiếng quán
        StartCoroutine(LeaveAngryRestaurant(1));
        // TODO: Giảm danh tiếng quán
    }

    IEnumerator LeaveHappyRestaurant()
    {
        if (currentSpeechBubble != null)
        {
            Destroy(currentSpeechBubble);
        }
        isSitting = false;
        interactionIcon.SetActive(true); // Hiển thị biểu tượng tương tác khi khách rời đi
        interactionAnimator.SetBool("isHappy", true);
        AudioManager.Instance.playcustomerSFX("Wow"); // Phát âm thanh khách vui vẻ khi rời đi
        GameManager.Instance.IncreaseReputation(GameManager.Instance.reputationIncrease); // Tăng danh tiếng quán
        chairScript.isOccupied = false; // Giải phóng ghế
        animator.SetBool("isSitting", false);
        animator.SetBool("isMoving", true);
        agent.SetDestination(spawnPoint.position); // Quay lại điểm spawn
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
        Destroy(gameObject); // Xóa khách
    }
    IEnumerator LeaveAngryRestaurant(int index)
    {
        if (currentSpeechBubble != null)
        {
            Destroy(currentSpeechBubble);
        }
        isSitting = false;
        interactionIcon.SetActive(true); // Hiển thị biểu tượng tương tác khi khách rời đi
        switch (index)
        {
            case 1:
                break;
            case 2:
                interactionAnimator.SetBool("isAngrySalted", true);
                break;
            case 3:
                interactionAnimator.SetBool("isAngryMouse", true);
                break;
        }

        chairScript.isOccupied = false; // Giải phóng ghế
        animator.SetBool("isSitting", false);
        animator.SetBool("isMoving", true);
        AudioManager.Instance.playcustomerSFX("Aww"); // Phát âm thanh khách giận dữ khi rời đi
        agent.SetDestination(spawnPoint.position); // Quay lại điểm spawn
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);
        Destroy(gameObject); // Xóa khách
    }

    void SitDown()
    {
        if (chairScript != null)
        {
            isSitting = true;
            animator.SetBool("isSitting", true);
            animator.SetInteger("sitDirection", chairScript.sitDirection);
            agent.isStopped = true;
            agent.ResetPath();
            Debug.Log(gameObject.name + " đã ngồi vào ghế");
            OrderFood();
        }
    }

    public bool canInteract()
    {
        return isSitting; // Chỉ có thể tương tác khi đã ngồi
    }
    public void Interact()
    {
        Debug.Log(canInteract() ? "Có thể tương tác với đơn hàng" : "Không thể tương tác với đơn hàng");
        if (canInteract())
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            Food carriedFood = player?.GetCarriedFood();
            if (carriedFood != null)
            {
                Debug.Log(carriedFood.dish.dishId);
            }

            if (carriedFood != null && carriedFood.dish.dishId == orderedDish.dishId && hasOrdered == false)
            {
                // Ẩn carriedFood (giả lập khách đang ăn)
                carriedFood.gameObject.SetActive(false);
                player.SetCarryingFood(false);
                // Bắt đầu coroutine kiểm tra sau 2s
                AudioManager.Instance.playcustomerSFX("Eat");
                StartCoroutine(CheckFoodAfterEating(carriedFood, player));
            }
            else
            {
                Debug.Log($"{gameObject.name}: No dish or wrong dish delivered");
                InteractWithOrder();
            }
        }
    }

    private IEnumerator CheckFoodAfterEating(Food carriedFood, PlayerMovement player)
    {
        yield return new WaitForSeconds(3f);

        // Kiểm tra muối hoặc chuột
        if(carriedFood.isMouseAdded || carriedFood.isSalted)
        {
            if (carriedFood.isSalted)
            {
                GameManager.Instance.DecreaseReputation(GameManager.Instance.reputationDecreaseOnSalted); // Giảm danh tiếng quán
                StartCoroutine(LeaveAngryRestaurant(2));
            }
            else if (carriedFood.isMouseAdded)
            {
                GameManager.Instance.DecreaseReputation(GameManager.Instance.reputationDecreaseOnMouse); // Giảm danh tiếng quán
                StartCoroutine(LeaveAngryRestaurant(3));
            }
            Destroy(carriedFood.gameObject);
            Debug.Log(orderID);
            OrderManager.Instance.RemoveFinishOrder(orderID);
        }
        else
        {
            Destroy(carriedFood.gameObject);
            Debug.Log(orderID);
            OrderManager.Instance.RemoveFinishOrder(orderID);
            StartCoroutine(LeaveHappyRestaurant());
        }
    }
}
