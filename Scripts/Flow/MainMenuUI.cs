using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    [Header("最高分显示")]
    [SerializeField] private TextMeshProUGUI highScoreText;  // ✅ 新增
    void Start()
    {

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        //初始显示主菜单
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        // ✅ 显示最高分
        if (highScoreText != null && SaveManager.Instance != null)
        {
            highScoreText.text = $"最高分: {SaveManager.Instance.Data.highScore}";
        }
    }
    void Update()
    {
        // ✅ 每帧检查当前游戏状态
        if (GameFlowManager.Instance != null)
        {
            // 如果游戏处于主菜单状态，就显示面板
            if (GameFlowManager.Instance.CurrentState == GameState.MainMenu)
            {
                if (mainMenuPanel != null && !mainMenuPanel.activeSelf)
                {
                    mainMenuPanel.SetActive(true);
                }
            }
            else
            {
                // 如果游戏在运行或暂停，就隐藏主菜单
                if (mainMenuPanel != null && mainMenuPanel.activeSelf)
                {
                    mainMenuPanel.SetActive(false);
                }
            }
        }
    }

    void OnStartClicked()
    {
        Debug.Log("点击开始");
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        GameFlowManager.Instance?.StartGame();
    }

    void OnQuitClicked()
    {
        Debug.Log("点击退出");
        GameFlowManager.Instance?.QuitGame();
    }
}