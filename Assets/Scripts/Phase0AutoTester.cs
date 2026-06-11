using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 0 자동 테스트: 생성 → 머지 → 전투 흐름 검증.
/// Play Mode 진입 후 자동 실행. 테스트 완료 시 자신을 제거.
/// </summary>
public class Phase0AutoTester : MonoBehaviour
{
    IEnumerator Start()
    {
        Debug.Log("=== Phase0 AutoTest 시작 ===");

        // 1초 대기 (GameManager.Start() + 웨이브 스폰 완료 대기)
        yield return new WaitForSeconds(1f);

        // ── 1. 에너지 확인 ────────────────────────────────────────
        Debug.Log($"[Test-1] 초기 에너지: {GeneratorEnergy.Instance.Current}");

        // ── 2. 생성기 3회 스폰 ────────────────────────────────────
        for (int i = 0; i < 3; i++)
        {
            GeneratorEnergy.Instance.AddEnergy(10);
            Generator.Instance.RequestSpawn();
            yield return new WaitForSeconds(0.15f);
        }
        Debug.Log($"[Test-2] 스폰 3회 완료. 에너지: {GeneratorEnergy.Instance.Current} | 보드 빈칸: {MergeBoard.Instance.HasEmptyCell()}");

        // ── 3. 보드 상태 ──────────────────────────────────────────
        int tier = MergeBoard.Instance.GetMaxTierOf("DUMMY_A");
        Debug.Log($"[Test-3] DUMMY_A 현재 최고 tier: {tier}");

        // ── 4. 머지 가능하도록 tier1 피스 추가 배치 ──────────────
        var data = DataManager.GetUnitData("DUMMY_A", 1);
        if (data != null)
        {
            MergeBoard.Instance.PlaceInitialPiece(data);  // tier1 추가
            MergeBoard.Instance.PlaceInitialPiece(data);  // tier1 추가 (2개면 드래그로 머지 가능)
            yield return new WaitForSeconds(0.1f);
            Debug.Log($"[Test-4] PlaceInitialPiece x2 완료. 보드 빈칸: {MergeBoard.Instance.HasEmptyCell()}");
        }

        // ── 5. 전투 진행 5초 관찰 ────────────────────────────────
        Debug.Log("[Test-5] 5초 전투 진행 관찰...");
        yield return new WaitForSeconds(5f);
        Debug.Log($"[Test-6] 5초 경과. DUMMY_A 최고 tier: {MergeBoard.Instance.GetMaxTierOf("DUMMY_A")}");

        Debug.Log("=== Phase0 AutoTest 완료 (위 로그에 오류 없으면 정상) ===");
        Destroy(gameObject);
    }
}
