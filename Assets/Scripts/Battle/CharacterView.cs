using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS0649

/// <summary>
/// 전투 화면 유닛 뷰.
/// ATB 게이지 업데이트, 공격/스킬 모션, 피격/사망 처리를 담당.
/// 아군: unitId = unit.unitId / 적: unitId = enemy.instanceId
/// </summary>
public class CharacterView : MonoBehaviour
{
    // 공개 필드 (BattleManager가 주입)
    public string unitId;
    public bool   isAlly;

    // ATB
    private float _atbGauge;
    private float _spd;
    private bool  _isActing;

    // 이동
    private Vector3 _originPosition;

    // 공격 타겟
    private CharacterView _currentTarget;

    // HP바 (Filled Image — fillAmount으로 비율 제어)
    [SerializeField] private Image _hpBarFill;

    // Animator
    private Animator _animator;

    // 타임아웃 (Animator 미연결 fallback)
    private Coroutine _actionTimeoutCoroutine;
    private const float ACTION_TIMEOUT = 2f;

    // ─────────────────────────────────────────────────────────
    /// <summary>BattleManager가 Instantiate 직후 호출.</summary>
    public void Init(float spd)
    {
        _spd            = spd;
        _atbGauge       = 0f;
        _isActing       = false;
        _originPosition = transform.position;
        _animator       = GetComponent<Animator>();
        if (_hpBarFill != null) _hpBarFill.fillAmount = 1f;
    }

    void Update()
    {
        if (_isActing) return;

        _atbGauge += _spd * Time.deltaTime;
        if (_atbGauge >= 100f)
        {
            _atbGauge = 0f;
            if (BattleManager.Instance != null)
                BattleManager.Instance.QueueGaugeFull(this);
        }
    }

    // HP바 갱신
    public void UpdateHpBar(float current, float max)
    {
        if (_hpBarFill == null) return;
        _hpBarFill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
    }

    // 공격
    public void ExecuteAttack(CharacterView target)
    {
        _currentTarget = target;
        _isActing      = true;
        _actionTimeoutCoroutine = StartCoroutine(ActionTimeout());
        StartCoroutine(MoveAndAttack(target));
    }

    IEnumerator MoveAndAttack(CharacterView target)
    {
        Vector3 attackPos = Vector3.Lerp(transform.position, target.transform.position, 0.6f);
        yield return MoveToPosition(attackPos, 0.2f);

        if (_animator != null)
        {
            _animator.SetTrigger("AttackTrigger");
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
            OnAttackHit();
            yield return new WaitForSeconds(0.1f);
            OnActionComplete();
        }
    }

    // 스킬
    public void ExecuteSkill(CharacterView target)
    {
        _currentTarget = target;
        _isActing      = true;
        _actionTimeoutCoroutine = StartCoroutine(ActionTimeout());

        BattleManager.Instance.ClearPendingSkill(unitId);

        if (_animator != null)
            _animator.SetTrigger("SkillTrigger");
        else
            StartCoroutine(SkillFallback());
    }

    IEnumerator SkillFallback()
    {
        yield return new WaitForSeconds(0.1f);
        OnAttackHit();
        yield return new WaitForSeconds(0.1f);
        OnActionComplete();
    }

    // Animation Event 메서드
    /// <summary>공격 판정 프레임에 배치.</summary>
    public void OnAttackHit()
    {
        BattleManager.Instance.ProcessAttackHit(this, _currentTarget);
    }

    /// <summary>클립 마지막 프레임에 배치. Attack / Skill 공용.</summary>
    public void OnActionComplete()
    {
        if (_actionTimeoutCoroutine != null)
        {
            StopCoroutine(_actionTimeoutCoroutine);
            _actionTimeoutCoroutine = null;
        }
        StartCoroutine(ReturnToOrigin());
    }

    IEnumerator ReturnToOrigin()
    {
        yield return MoveToPosition(_originPosition, 0.2f);
        _isActing = false;
    }

    // 피격 / 사망
    public void PlayHit()
    {
        if (_animator != null)
            _animator.SetTrigger("HitTrigger");
    }

    public void OnDie()
    {
        _isActing = true;
        if (_animator != null)
            _animator.SetBool("IsDead", true);
        Destroy(gameObject, 0.8f);
    }

    // 머지 피드백
    public void PlayMergeFeedback()
    {
        StartCoroutine(ScalePop());
    }

    IEnumerator ScalePop()
    {
        transform.localScale = Vector3.one * 1.4f;
        yield return new WaitForSeconds(0.15f);
        transform.localScale = Vector3.one;
    }

    // 유틸
    IEnumerator MoveToPosition(Vector3 target, float duration)
    {
        Vector3 start   = transform.position;
        float   elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed            += Time.deltaTime;
            transform.position  = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        transform.position = target;
    }

    IEnumerator ActionTimeout()
    {
        yield return new WaitForSeconds(ACTION_TIMEOUT);
        if (_isActing)
        {
            Debug.LogWarning($"[CharacterView] ActionTimeout 발동: {unitId}");
            OnActionComplete();
        }
    }
}
