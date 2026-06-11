using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 클리어/실패 결과 팝업.
/// GameScene UI 명세 §7 기준.
/// PopupManager가 Show() 호출. 버튼은 다시하기 / 로비로.
/// </summary>
public class ResultPopupView : MonoBehaviour
{
    [SerializeField] private GameObject      resultCanvas;    // ResultCanvas (기본 비활성)
    [SerializeField] private TextMeshProUGUI resultText;      // "클리어!" / "실패..."
    [SerializeField] private TextMeshProUGUI waveResultText;  // "3/4 Wave 클리어"
    [SerializeField] private TextMeshProUGUI rewardText;      // 메타 재화 (미결, 빈칸)
    [SerializeField] private Button          retryButton;
    [SerializeField] private Button          lobbyButton;

    void Start()
    {
        retryButton.onClick.AddListener(OnRetry);
        lobbyButton.onClick.AddListener(OnLobby);
    }

    public void Show(bool isCleared, int clearedWaves, int totalWaves)
    {
        resultText.text     = isCleared ? "클리어!" : "실패...";
        waveResultText.text = $"{clearedWaves}/{totalWaves} Wave 클리어";
        rewardText.text     = "";  // 메타 재화 확정 후 채울 것
        resultCanvas.SetActive(true);
        Time.timeScale = 0f;  // 팝업 중 게임 정지
    }

    void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    void OnLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LobbyScene");
    }
}
