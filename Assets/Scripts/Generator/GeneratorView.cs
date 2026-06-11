using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 생성기 버튼 UI.
/// 클릭 시 Generator.RequestSpawn() 호출.
/// OnBoardFullChanged / OnEnergyChanged 구독으로 버튼 상태와 에너지 UI 갱신.
/// </summary>
public class GeneratorView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Text  energyLabel;  // 에너지 수치 표시 (선택)
    [SerializeField] private Image buttonImage;  // 버튼 이미지 (비활성 색상 처리용)

    private bool _isCooldown = false;
    private bool _boardFull  = false;

    static readonly Color ActiveColor   = new Color(0.20f, 0.65f, 0.30f);
    static readonly Color InactiveColor = new Color(0.40f, 0.40f, 0.40f);

    void OnEnable()
    {
        EventBus.Subscribe<bool>(EventNames.OnBoardFullChanged, OnBoardFullChanged);
        EventBus.Subscribe<int> (EventNames.OnEnergyChanged,    OnEnergyChanged);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<bool>(EventNames.OnBoardFullChanged, OnBoardFullChanged);
        EventBus.Unsubscribe<int> (EventNames.OnEnergyChanged,    OnEnergyChanged);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isCooldown || _boardFull) return;
        if (!GeneratorEnergy.Instance.CanSpawn) return;
        StartCoroutine(SpawnWithCooldown());
    }

    IEnumerator SpawnWithCooldown()
    {
        _isCooldown = true;
        Generator.Instance.RequestSpawn();
        yield return new WaitForSeconds(0.2f);
        _isCooldown = false;
    }

    private void OnBoardFullChanged(bool isFull)
    {
        _boardFull = isFull;
        RefreshButtonColor();
    }

    private void OnEnergyChanged(int current)
    {
        if (energyLabel != null)
            energyLabel.text = current.ToString();
        RefreshButtonColor();
    }

    void RefreshButtonColor()
    {
        if (buttonImage == null) return;
        bool interactable = !_isCooldown && !_boardFull && GeneratorEnergy.Instance.CanSpawn;
        buttonImage.color = interactable ? ActiveColor : InactiveColor;
    }
}
