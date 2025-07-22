using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Slider angerSlider;
    public Image angerFill;
    public Slider reputationSlider;
    public Image reputationFill;
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.02f;

    private Vector3 angerBarInitialPos;
    private Vector3 reputationBarInitialPos;
    private float shakeTimer;

    public GameObject menuPanel; // Kéo panel menu vào Inspector
    private RectTransform menuRect;
    void Awake()
    {
        Instance = this;
        if (angerSlider == null) Debug.LogError("angerSlider is null!");
        if (angerFill == null) Debug.LogError("angerFill is null!");
        //if (angerText == null) Debug.LogError("angerText is null!");
        if (reputationSlider == null) Debug.LogError("reputationSlider is null!");
        if (reputationFill == null) Debug.LogError("reputationFill is null!");
        //if (reputationText == null) Debug.LogError("reputationText is null!");
        

        angerBarInitialPos = angerSlider.transform.localPosition;
        reputationBarInitialPos = reputationSlider.transform.localPosition;
        // Thiết lập Slider
        angerSlider.minValue = 0f;
        angerSlider.maxValue = GameManager.Instance.maxAnger;
        reputationSlider.minValue = 0f;
        reputationSlider.maxValue = GameManager.Instance.maxReputation;

        menuRect = menuPanel.GetComponent<RectTransform>();
        menuPanel.SetActive(false);
    }

    void Update()
    {
        // Cập nhật Anger
        float anger = GameManager.Instance.GetAnger();
        angerSlider.value = anger;
        angerFill.fillAmount = anger / GameManager.Instance.maxAnger;
        //angerText.text = $"Anger: {Mathf.RoundToInt(anger)}/100";
        angerFill.color = Color.Lerp(Color.green, Color.red, anger / GameManager.Instance.maxAnger);

        // Cập nhật Reputation
        float reputation = GameManager.Instance.GetReputation();
        reputationSlider.value = reputation;
        reputationFill.fillAmount = reputation / GameManager.Instance.maxReputation;
        //reputationText.text = $"Reputation: {Mathf.RoundToInt(reputation)}/100";
        reputationFill.color = Color.Lerp(Color.gray, Color.green, reputation / GameManager.Instance.maxReputation);

        // Rung thanh khi thay đổi
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            angerSlider.transform.localPosition = angerBarInitialPos + shakeOffset;
            reputationSlider.transform.localPosition = reputationBarInitialPos + shakeOffset;
        }
        else
        {
            angerSlider.transform.localPosition = angerBarInitialPos;
            reputationSlider.transform.localPosition = reputationBarInitialPos;
        }
    }

    public void ShakeBars()
    {
        shakeTimer = shakeDuration;
    }

    public void OpenSettingsMenu()
    {
        Debug.Log("Check");
        menuPanel.SetActive(true);

        // Reset vị trí menuPanel ở ngoài màn hình trước
        menuRect.anchoredPosition = new Vector2(0, Screen.height);

        // Dùng DOTween trượt xuống vị trí 0
        menuRect.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack);
    }

    public void CloseSettingsMenu()
    {
        // Trượt lên rồi disable panel
        menuRect.DOAnchorPos(new Vector2(0, Screen.height), 0.5f).SetEase(Ease.InBack)
            .OnComplete(() => menuPanel.SetActive(false));
    }
}
