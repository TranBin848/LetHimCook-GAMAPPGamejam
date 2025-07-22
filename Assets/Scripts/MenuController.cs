using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuPanel; // Kéo panel menu vào Inspector

    void Start()
    {
        menuPanel.SetActive(false); // Ẩn panel khi khởi động
    }

    // Gọi khi nhấn nút Setting
    public void OpenSettingsMenu()
    {
        menuPanel.SetActive(true);
    }

    // Gọi khi nhấn nút Done
    public void CloseSettingsMenu()
    {
        menuPanel.SetActive(false);
    }
}
