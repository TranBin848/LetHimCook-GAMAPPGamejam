using UnityEngine;

public class Food : MonoBehaviour, IInteractable
{
    private bool canBeInteracted = true;
    public DishData dish;
    public GameObject food;

    public bool isSalted = false; // Món ăn có bị nêm muối không
    public bool isMouseAdded = false; // Món ăn có bị thêm chuột không 

    public void Interact()
    {
        if (canBeInteracted)
        {
            // Gắn món ăn vào người chơi
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                transform.SetParent(player.transform);
                food.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f); // Giảm kích thước món ăn
                canBeInteracted = false; // Ngăn tương tác lại
                player.SetCarryingFood(true, this); // Cập nhật trạng thái người chơi
            }
        }
    }

    public void AddSalt()
    {
        isSalted = true;
        Debug.Log($"{gameObject.name} đã bị nêm muối!");
    }

    public void AddMouse()
    {
        // Flag hoặc logic khi thêm chuột
        isMouseAdded = true;
        Debug.Log($"{gameObject.name} đã bị thả chuột!");
    }

    public bool canInteract()
    {
        return canBeInteracted;
    }

    public void UpdatePosition(Vector2 direction)
    {
        if (!canBeInteracted && transform.parent != null)
        {
            Vector2 newPos = new Vector2(0, -0.5f);
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                newPos = direction.x > 0 ? new Vector2(0.6f, -0.4f) : new Vector2(-0.55f, -0.4f);
            }
            else if (Mathf.Abs(direction.y) > 0)
            {
                newPos = direction.y > 0 ? new Vector2(0, 0.6f) : new Vector2(0, -0.5f);
            }
            transform.localPosition = new Vector3(newPos.x, newPos.y, 0);
        }
    }
}
