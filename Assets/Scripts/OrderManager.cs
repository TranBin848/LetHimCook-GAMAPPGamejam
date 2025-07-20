using UnityEngine;
using System.Collections.Generic;
    
public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;
    public GameObject orderCardPrefab;
    public Transform orderPanel;
    public List<OrderCard> activeOrders = new List<OrderCard>();
   
    private void Awake()
    {
        Instance = this;
    }

    public int AddOrder(DishData dish, float orderTime, Customer customer)
    {
        // Tạo card mới
        GameObject card = Instantiate(orderCardPrefab, orderPanel);
        OrderCard cardScript = card.GetComponent<OrderCard>();
        cardScript.Setup(dish, orderTime, customer);
        activeOrders.Add(cardScript);
        return activeOrders.Count - 1; // Trả về chỉ số của order mới
    }

    public void RemoveFinishOrder(int index)
    {
        OrderCard card = activeOrders[index];
        if (card != null)
        {
            activeOrders.RemoveAt(index);
            Destroy(card.gameObject);
        }
        else
        {
            Debug.LogWarning("OrderCard không tồn tại tại chỉ số: " + index);
        }
    }
    public void RemoveOrder(OrderCard card)
    {
        activeOrders.Remove(card);
        Destroy(card.gameObject);
    }
}
