using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 수치 텍스트 하나. 위로 떠오르며 페이드아웃.
/// FloatingTextManager가 오브젝트 풀로 관리한다.
/// </summary>
public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    public void Play(string text, Color color, Action onComplete)
    {
        label.text  = text;
        label.color = color;
        StartCoroutine(Animate(onComplete));
    }

    IEnumerator Animate(Action onComplete)
    {
        float    duration  = 0.8f;
        float    elapsed   = 0f;
        Vector3  startPos  = transform.position;
        Color    baseColor = label.color;

        while (elapsed < duration)
        {
            elapsed            += Time.deltaTime;
            float t             = elapsed / duration;
            transform.position  = startPos + Vector3.up * (t * 0.8f);
            label.color         = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - t);
            yield return null;
        }

        onComplete?.Invoke();
    }
}
