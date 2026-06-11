using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 수치 텍스트(+ATK, +HP 등)가 World Space에서 떠올라 사라지는 이펙트 전담 매니저.
/// FloatingText 오브젝트 풀로 관리.
/// </summary>
public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic() => Instance = null;

    [SerializeField] private GameObject floatingTextPrefab;  // FloatingText 프리팹
    [SerializeField] private Canvas     worldCanvas;         // World Space Canvas

    private Queue<FloatingText> _pool = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Show(Vector3 worldPosition, string text, Color color)
    {
        if (floatingTextPrefab == null) return;

        var item = GetFromPool();
        item.transform.position = worldPosition + Vector3.up * 0.5f;
        item.gameObject.SetActive(true);
        item.Play(text, color, () => ReturnToPool(item));
    }

    FloatingText GetFromPool()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();

        var parent = worldCanvas != null ? worldCanvas.transform : transform;
        var go     = Instantiate(floatingTextPrefab, parent);
        return go.GetComponent<FloatingText>();
    }

    void ReturnToPool(FloatingText item)
    {
        item.gameObject.SetActive(false);
        _pool.Enqueue(item);
    }
}
