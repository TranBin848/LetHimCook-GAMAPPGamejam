using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class StartMenuController : MonoBehaviour
{
    public GameObject helpPanel;
    public RectTransform transitionPanel; // Kéo RectTransform của TransitionPanel vào

    public void Start()
    {
        PauseController.SetPause(false);
        helpPanel.SetActive(false); // Ẩn panel trợ giúp ban đầu
        // Đặt transitionPanel ra ngoài màn hình ban đầu (ở trên)
        float screenHeight = Screen.height;
        transitionPanel.anchoredPosition = new Vector2(0, screenHeight);
    }
    public void OnStartClick()
    {
        // Slide panel từ trên xuống (0.5s) rồi load scene
        transitionPanel.DOAnchorPos(Vector2.zero, 0.5f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                SceneManager.LoadScene("SampleScene");
            });
    }
    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Dừng chơi trong Unity Editor
#endif
        Application.Quit(); // Thoát ứng dụng
    }
    public void OpenHelpPanel()
    {
        helpPanel.SetActive(true);
        // Reset scale về nhỏ trước khi zoom
        helpPanel.transform.localScale = Vector3.zero;

        // Zoom to lên trong 0.3 giây với easing OutBack
        helpPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    public void CloseHelpPanel()
    {
        // Zoom nhỏ lại trong 0.2 giây với easing InBack, sau đó ẩn panel
        helpPanel.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() => helpPanel.SetActive(false));
    }

}
