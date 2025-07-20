
using UnityEngine;
using UnityEngine.UI;
public class OrderCard : MonoBehaviour
{
    public Image dishImage;
    public Image fillBarImage;
    private Vector3 initialScale;
    private float prepTimeLimit; 
    private float prepTimer;
    private Customer customer;
    public DishData dishData { get; private set; }
    public void Setup(DishData dish, float timeLimit, Customer customer)
    {
        dishData = dish;
        dishImage.sprite = dish.dishSprite;
        prepTimeLimit = timeLimit;
        prepTimer = timeLimit;
        this.customer = customer;
        initialScale = fillBarImage.transform.localScale;
        fillBarImage.transform.localScale = new Vector3(0, initialScale.y, initialScale.z);
    }

    private void Update()
    {
        if (prepTimer > 0)
        {
            prepTimer -= Time.deltaTime;
            float fillRatio = 1 - (prepTimer / prepTimeLimit); // Tỷ lệ từ 0 đến 1
            fillBarImage.transform.localScale = new Vector3(fillRatio * initialScale.x, initialScale.y, initialScale.z); // Scale thanh
            if (prepTimer <= 3 && prepTimer > 0) // Rung khi gần hết
            {
                fillBarImage.transform.localPosition += Random.insideUnitSphere * 0.02f;
            }
            if (prepTimer <= 0)
            {
                customer.OnPrepTimeout();
                OrderManager.Instance.RemoveOrder(this);
            }
        }
    }
}
