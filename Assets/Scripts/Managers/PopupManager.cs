using UnityEngine;

/// <summary>
/// 게임 내 UI 팝업/오버레이 총괄 매니저.
/// 수치 이펙트(+ATK, +HP 등)는 FloatingTextManager 담당.
/// Phase 0: 결과 팝업만 존재. ResultPopupView 직접 참조.
/// </summary>
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() => Instance = null;

    [SerializeField] private ResultPopupView resultPopup;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ShowResultPopup(bool isCleared, int clearedWaves, int totalWaves)
    {
        if (resultPopup == null) return;
        resultPopup.Show(isCleared, clearedWaves, totalWaves);
    }
}
