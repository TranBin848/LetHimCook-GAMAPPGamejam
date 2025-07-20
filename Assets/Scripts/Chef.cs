using UnityEngine;
using System.Collections.Generic;

public class Chef : MonoBehaviour
{
    public Transform storagePoint;
    public Transform kitchenPoint;
    public Transform privateRoomPoint;
    public Transform tablePoint;
    public List<Transform> tablePositions = new List<Transform>();
    public List<GameObject> dishList; // Prefab món ăn
    public GameObject dishPrefab; // Prefab món ăn sẽ được đặt lên bàn
    private UnityEngine.AI.NavMeshAgent agent;
    private Animator animator;
    private enum BossState { Idle, MovingToStorage, MovingToKitchen, CollectingFish, CollectingVegetable, MovingToPrivateRoom, MovingToTable }
    private BossState currentState = BossState.Idle;
    public float collectFishTimer = 2f;
    public float collectVegetableTimer = 2f;
    public float cookingTimer = 5f;
    public float placeDishTimer = 1f;
    private OrderCard currentOrder; // OrderCard đang xử lý

    public GameObject interactionIcon; // Biểu tượng tương tác
    public GameObject Pan;
    public Animator interactionAnimator;
    void Start()
    {
        interactionIcon.SetActive(false); // Ẩn biểu tượng tương tác ban đầu
        Pan.SetActive(false );
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    Transform GetFirstEmptyTablePosition()
    {
        foreach (Transform pos in tablePositions)
        {
            if (pos.childCount == 0) // Nếu không có object con, vị trí trống
            {
                return pos;
            }
        }
        //Debug.LogWarning("Không còn chỗ trống trên bàn!");
        return tablePositions[0]; // fallback trả về vị trí đầu tiên nếu full
    }

    void Update()
    {
        // Cập nhật animation di chuyển
        Vector3 velocity = agent.velocity;
        if (velocity.magnitude > 0.1f)
        {
            animator.SetBool("isMoving", true);
            animator.SetFloat("moveX", velocity.normalized.x);
            animator.SetFloat("moveY", velocity.normalized.y);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        // Logic di chuyển dựa trên OrderCard
        switch (currentState)
        {
            case BossState.Idle:
                if (OrderManager.Instance.activeOrders.Count > 0)
                {
                    animator.SetBool("isResting", false); // Dừng animation nghỉ nếu có OrderCard mới
                    interactionIcon.SetActive(false); // Ẩn biểu tượng tương tác khi bắt đầu xử lý OrderCard
                    interactionAnimator.SetBool("isResting", false); // Tắt animation nghỉ trong InteractionController
                    currentOrder = OrderManager.Instance.activeOrders[0];
                    // Có OrderCard, đi đến kho
                    currentState = BossState.MovingToStorage;
                    agent.SetDestination(storagePoint.position);
                   // Debug.Log("Boss moving to storage");
                }
                else
                {
                    
                    // Không có OrderCard, đi đến phòng riêng
                    currentState = BossState.MovingToPrivateRoom;
                    agent.SetDestination(privateRoomPoint.position);
                    //Debug.Log("Boss moving to private room");
                }
                break;

            case BossState.MovingToStorage:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    currentState = BossState.CollectingFish;
                    agent.SetDestination(currentOrder.dishData.fishIngredient.transform.position);
                    //Debug.Log($"Boss moving to fish: {currentOrder.dishData.fishIngredient.name}");
                }
                break;

            case BossState.CollectingFish:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    collectFishTimer -= Time.deltaTime;
                    Debug.Log(collectFishTimer);
                    if (collectFishTimer <= 0)
                    {
                        animator.SetTrigger("CollectIngredients");
                        currentState = BossState.CollectingVegetable;
                        agent.SetDestination(currentOrder.dishData.vegetableIngredient.transform.position);
                        collectFishTimer = 2f;
                        //Debug.Log($"Boss collecting fish: {currentOrder.dishData.fishIngredient.name}");
                    }
                }
                break;

            case BossState.CollectingVegetable:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    collectVegetableTimer -= Time.deltaTime;
                    if (collectVegetableTimer <= 0)
                    {
                        animator.SetTrigger("CollectIngredients");
                        currentState = BossState.MovingToKitchen;
                        agent.SetDestination(kitchenPoint.position);
                        collectVegetableTimer = 2f;
                        
                        //Debug.Log($"Boss collecting vegetable: {currentOrder.dishData.vegetableIngredient.name}");
                    }
                }
                break;

            case BossState.MovingToKitchen:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    animator.SetBool("isCooking", true); // Animation nấu ăn
                    interactionIcon.SetActive(true); // Hiển thị biểu tượng tương tác khi khách rời đi
                    Pan.SetActive(true); // Hiển thị chảo khi nấu ăn
                    interactionAnimator.SetBool("isCooking", true); // Bật animation nấu ăn trong InteractionController
                    cookingTimer -= Time.deltaTime;
                    if (cookingTimer <= 0)
                    {
                        currentState = BossState.Idle;
                        cookingTimer = 4.2f;
                        animator.SetBool("isCooking", false); // Dừng animation nấu ăn
                        interactionIcon.SetActive(false); // Ẩn biểu tượng tương tác
                        Pan.SetActive(false); // Ẩn chảo khi nấu ăn xong
                        interactionAnimator.SetBool("isCooking", false); // Tắt animation nấu ăn trong InteractionController

                        // Sau khi nấu xong, chuyển sang MovingToTable
                        currentState = BossState.MovingToTable;
                        agent.SetDestination(tablePoint.position);

                        //Debug.Log("Boss finished at kitchen, checking orders");
                    }
                }
                break;

            case BossState.MovingToPrivateRoom:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    animator.SetBool("isResting", true); // Animation nghỉ
                    interactionIcon.SetActive(true); // Ẩn biểu tượng tương tác khi nghỉ
                    interactionAnimator.SetBool("isResting", true); // Bật animation nghỉ trong InteractionController
                    currentState = BossState.Idle;
                    //Debug.Log("Boss resting in private room");
                }
                break;
            case BossState.MovingToTable:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    animator.SetBool("isCooking", true); // Dừng animation di chuyển
                    // Đặt món ăn prefab lên bàn
                    Transform emptyPos = GetFirstEmptyTablePosition();
                    //Debug.Log(currentOrder.dishData.dishId);
                    switch(currentOrder.dishData.dishId)
                    {
                        case 1: // Món ăn 1
                            dishPrefab = dishList[0];
                            break;
                        case 2: // Món ăn 2
                            dishPrefab = dishList[1];
                            break;
                        case 3: // Món ăn 3
                            dishPrefab = dishList[2];
                            break;
                    }
                    if (emptyPos.childCount == 0)
                    {
                        Instantiate(dishPrefab, emptyPos.position, Quaternion.identity, emptyPos);
                        //Debug.Log("Boss placed dish on table at " + emptyPos.name);
                    }
                    else
                    {
                        //Debug.LogWarning("No empty table position found!");
                    }
                    animator.SetBool("isCooking", false); // Dừng animation nấu ăn
                    currentState = BossState.Idle;
                }
                break;
        }
    }
}
