using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
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

    public string AddOrder(DishData dish, float orderTime, Customer customer)
    {
        GameObject card = Instantiate(orderCardPrefab, orderPanel);
        OrderCard cardScript = card.GetComponent<OrderCard>();

        // Gán orderID
        cardScript.orderID = System.Guid.NewGuid().ToString();

        // Setup order
        cardScript.Setup(dish, orderTime, customer);
        activeOrders.Add(cardScript);

        // Start spawn animation
        StartCoroutine(SpawnOrderCard(cardScript));

        return cardScript.orderID;
    }

    private IEnumerator SpawnOrderCard(OrderCard cardScript)
    {
        RectTransform cardRect = cardScript.GetComponent<RectTransform>();

        yield return null; // Đợi 1 frame để Layout Group tính xong
        LayoutRebuilder.ForceRebuildLayoutImmediate(orderPanel.GetComponent<RectTransform>()); // Force rebuild

        Vector2 targetPos = cardRect.anchoredPosition;
        float outsideX = orderPanel.GetComponent<RectTransform>().rect.width + 200f;
        cardRect.anchoredPosition = new Vector2(outsideX, targetPos.y);

        cardRect.DOAnchorPos(targetPos, 0.5f).SetEase(Ease.OutBack);
    }

    public void RemoveFinishOrder(string orderID)
    {
        var card = activeOrders.Find(c => c.orderID == orderID);
        if (card == null)
        {
            Debug.LogWarning("OrderCard không tồn tại với ID: " + orderID);
            return;
        }

        int index = activeOrders.IndexOf(card);
        activeOrders.RemoveAt(index);

        RectTransform removedRect = card.GetComponent<RectTransform>();

        // Fade out + scale down
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg == null) cg = card.gameObject.AddComponent<CanvasGroup>();
        cg.DOFade(0, 0.3f);
        removedRect.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(card.gameObject);

            // Move up các card bên dưới
            for (int i = index; i < activeOrders.Count; i++)
            {
                RectTransform rt = activeOrders[i].GetComponent<RectTransform>();
                Vector2 targetPos = rt.anchoredPosition;
                rt.DOAnchorPosY(targetPos.y + removedRect.rect.height + 90f, 0.3f).From().SetEase(Ease.OutBack);
            }
        });
    }
    public void RemoveOrder(OrderCard card)
    {
        if (card == null)
        {
            Debug.LogWarning("OrderCard null");
            return;
        }

        int index = activeOrders.IndexOf(card);
        if (index == -1)
        {
            Debug.LogWarning("OrderCard không tồn tại trong danh sách");
            return;
        }

        activeOrders.RemoveAt(index);

        RectTransform removedRect = card.GetComponent<RectTransform>();

        // Fade out + scale down
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg == null) cg = card.gameObject.AddComponent<CanvasGroup>();
        cg.DOFade(0, 0.3f);
        removedRect.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(card.gameObject);

            // Move up các card bên dưới
            for (int i = index; i < activeOrders.Count; i++)
            {
                RectTransform rt = activeOrders[i].GetComponent<RectTransform>();
                Vector2 targetPos = rt.anchoredPosition;
                rt.DOAnchorPosY(targetPos.y + removedRect.rect.height + 90f, 0.3f).From().SetEase(Ease.OutBack);
                // +90f là spacing nếu bạn để spacing trong layout group
            }
        });
    }

}
