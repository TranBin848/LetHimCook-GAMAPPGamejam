using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class OrderCard : MonoBehaviour
{
    public Image dishImage;
    public Slider slider;
    public Image fillBarImage;
    private float prepTimeLimit;
    private float prepTimer;
    private Customer customer;
    public DishData dishData { get; private set; }
    public bool hasFinished = false;
    public Image backgroundImage; // background của card
    public Color finishedColor = new Color(0.8f, 1f, 0.8f, 1f); // xanh nhạt
    private void Start()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f; // slider từ 0 đến 1
        slider.value = 1f; // bắt đầu full 100%
    }

    public void Setup(DishData dish, float timeLimit, Customer customer)
    {
        dishData = dish;
        dishImage.sprite = dish.dishSprite;
        prepTimeLimit = timeLimit;
        prepTimer = timeLimit;
        this.customer = customer;
    }

    private void Update()
    {
        if (prepTimer > 0)
        {
            prepTimer -= Time.deltaTime;
            float fillRatio = prepTimer / prepTimeLimit;
            slider.value = fillRatio;

            // Chuyển màu từ xanh sang đỏ
            Color startColor = Color.green;
            Color endColor = Color.red;
            fillBarImage.color = Color.Lerp(endColor, startColor, fillRatio);
            // Nếu bạn dùng slider.fillRect.GetComponent<Image>(), thay fillBarImage bằng fill image của slider

            if (prepTimer <= 3 && prepTimer > 0)
            {
                // Hiệu ứng shake nếu cần
            }

            if (prepTimer <= 0)
            {
                customer.OnPrepTimeout();
                OrderManager.Instance.RemoveOrder(this);
            }
        }
    }
    public void MarkAsFinished()
    {
        hasFinished = true;
        if (backgroundImage != null)
        {
            backgroundImage.color = finishedColor;
        }
    }
}
