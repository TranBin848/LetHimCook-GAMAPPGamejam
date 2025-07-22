using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Chef : MonoBehaviour, IInteractable
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
    private float angerTimer = 0f;
    public float angerIncreaseInterval = 7f;
    private OrderCard currentOrder; // OrderCard đang xử lý

    public GameObject interactionIcon; // Biểu tượng tương tác
    public GameObject Pan;
    public Animator interactionAnimator;

    private bool isCookingSoundPlaying = false;
    private bool isAngryNoEmptyPosition = false;

    public ChefDialogue dialogueData;
    public GameObject dialoguePanel; // Panel hiển thị hội thoại
    public TMP_Text diaLogueText, nameText, instructionText;
    public Image portraitImage;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    private bool hasInteracted = false;

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

    public void SetAngryMouse(bool value)
    { 
        if (value == true)
        {
            interactionIcon.SetActive(true);
            AudioManager.Instance.playchefSFX("Angry"); // 🔊 Play khi giận dữ
        }
        interactionAnimator.SetBool("isAngryMouse", value);
    }

    public void SetAngrySalt(bool value)
    {
        
        if (value == true)
        {
            interactionIcon.SetActive(true);
            AudioManager.Instance.playchefSFX("Angry"); // 🔊 Play khi giận dữ
        }
        interactionAnimator.SetBool("isAngrySalt", value);
    }

    void Update()
    {
        if (PauseController.IsGamePause)
        {
            agent.isStopped = true;
            animator.SetBool("isMoving", false);
            animator.SetBool("isCooking", false);
            animator.SetBool("isResting", false);
            return;
        }

        // Nếu chưa interact thì đứng yên không làm gì
        if (!hasInteracted)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isResting", false);
            animator.SetBool("isCooking", false);
            agent.isStopped = true; // Dừng agent
            return;
        }

        agent.isStopped = false; // Mở agent khi đã interact

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
                // Tìm OrderCard chưa hoàn thành
                currentOrder = null;
                foreach (OrderCard order in OrderManager.Instance.activeOrders)
                {
                    if (!order.hasFinished)
                    {
                        currentOrder = order;
                        break;
                    }
                }

                if (currentOrder != null)
                {
                    animator.SetBool("isResting", false);
                    interactionIcon.SetActive(false);
                    interactionAnimator.SetBool("isResting", false);

                    currentState = BossState.MovingToStorage;
                    agent.SetDestination(storagePoint.position);
                }
                else
                {
                    // Không có OrderCard chưa hoàn thành, đi nghỉ
                    currentState = BossState.MovingToPrivateRoom;
                    agent.SetDestination(privateRoomPoint.position);
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
                        //animator.SetTrigger("CollectIngredients");
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
                        //animator.SetTrigger("CollectIngredients");
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
                    interactionIcon.SetActive(true); 
                    Pan.SetActive(true); // Hiển thị chảo khi nấu ăn
                    interactionAnimator.SetBool("isCooking", true); // Bật animation nấu ăn trong InteractionController
                                                                    // 🔥 Play clock tick sound nếu chưa phát
                    if (!isCookingSoundPlaying)
                    {
                        AudioManager.Instance.playchefSFX("ClockTick");
                        isCookingSoundPlaying = true;
                    }

                    cookingTimer -= Time.deltaTime;
                    if (cookingTimer <= 0)
                    {
                        currentState = BossState.Idle;
                        cookingTimer = 4.2f;
                        animator.SetBool("isCooking", false); // Dừng animation nấu ăn
                        interactionIcon.SetActive(false); // Ẩn biểu tượng tương tác
                        Pan.SetActive(false); // Ẩn chảo khi nấu ăn xong
                        interactionAnimator.SetBool("isCooking", false); // Tắt animation nấu ăn trong InteractionController

                        // 🔥 Stop clock tick sound khi nấu xong
                        AudioManager.Instance.stopchefSFX("ClockTick");
                        isCookingSoundPlaying = false;

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
                    animator.SetBool("isCooking", false);
                    Transform emptyPos = GetFirstEmptyTablePosition();
                    switch (currentOrder.dishData.dishId)
                    {
                        case 1:
                            dishPrefab = dishList[0];
                            break;
                        case 2:
                            dishPrefab = dishList[1];
                            break;
                        case 3:
                            dishPrefab = dishList[2];
                            break;
                    }

                    if (emptyPos.childCount == 0)
                    {
                        if (isAngryNoEmptyPosition)
                        {
                            interactionAnimator.SetBool("isAngry", false);
                            interactionIcon.SetActive(false); // Ẩn biểu tượng tương tác khi đặt món thành công
                            isAngryNoEmptyPosition = false;
                        }
                        Instantiate(dishPrefab, emptyPos.position, Quaternion.identity, emptyPos);

                        // Đánh dấu order đã hoàn thành
                        currentOrder.MarkAsFinished();
                        interactionAnimator.SetBool("isAngry", false);
                        interactionIcon.SetActive(false);

                        animator.SetBool("isCooking", false);
                        AudioManager.Instance.playchefSFX("Interact");
                        angerTimer = 0f; // Reset timer nếu đã đặt món thành công

                        currentState = BossState.Idle;
                    }
                    else
                    {
                        if(!isAngryNoEmptyPosition)
                        {
                            AudioManager.Instance.playchefSFX("Angry");
                            // Không có chỗ trống
                            interactionIcon.SetActive(true);
                            interactionAnimator.SetBool("isAngry", true);
                            isAngryNoEmptyPosition = true;
                        }
                        // Tăng anger mỗi 7 giây
                        angerTimer -= Time.deltaTime;
                        if (angerTimer <= 0f)
                        {
                            GameManager.Instance.IncreaseAnger(GameManager.Instance.angerIncreaseRate);
                            angerTimer = angerIncreaseInterval;
                        }

                        // Giữ nguyên state MovingToTable, Chef sẽ đứng im chờ
                    }
                }
                break;
        }
    }

    public void Interact()
    {
        if (dialogueData == null)
        {
            return;
        }
        if(isDialogueActive)
        {
            AudioManager.Instance.stopchefSFX("ChefPortrait"); // 🔊 Dừng âm thanh khi kết thúc hội thoại
            AudioManager.Instance.playchefSFX("ChefPortrait"); // 🔊 Phát âm thanh khi bắt đầu hội thoại
            NextLine();
        }
        else
        {
            // Bắt đầu hội thoại
            StartDialogue();
            instructionText.text = ""; // Xóa hướng dẫn khi bắt đầu hội thoại
            AudioManager.Instance.playchefSFX("ChefPortrait"); // 🔊 Phát âm thanh khi bắt đầu hội thoại
        }
    }

    public bool canInteract()
    {
        return !isDialogueActive;
    }

    private void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        dialoguePanel.SetActive(true);
        
        nameText.text = dialogueData.chefName;
        portraitImage.sprite = dialogueData.ncpPortrait;
        diaLogueText.text = "";
        isTyping = true;
        StartCoroutine(TypeLine());
    
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            diaLogueText.text = dialogueData.dialogueLines[dialogueIndex];
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
            AudioManager.Instance.stopchefSFX("ChefPortrait"); // 🔊 Dừng âm thanh khi kết thúc hội thoại
            hasInteracted = true; // ✅ Bắt đầu hoạt động sau khi kết thúc hội thoại
        }
    }
    IEnumerator TypeLine()
    {
        isTyping = true;
        diaLogueText.text = "";

        string line = dialogueData.dialogueLines[dialogueIndex];
        int i = 0;

        while (i < line.Length)
        {
            if (line[i] == '<')
            {
                // Bắt đầu tag rich text
                int closingIndex = line.IndexOf('>', i);
                if (closingIndex != -1)
                {
                    string tag = line.Substring(i, closingIndex - i + 1);
                    diaLogueText.text += tag;
                    i = closingIndex + 1;
                    continue;
                }
            }

            diaLogueText.text += line[i];
            i++;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        // Tự động chuyển sang dòng tiếp theo nếu cần
        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }
    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        diaLogueText.text = "";
        dialoguePanel.SetActive(false);
    }
}
