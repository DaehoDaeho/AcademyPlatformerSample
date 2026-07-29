using UnityEngine;

/// <summary>적의 진행 방향 앞에 이동 가능한 지면이 있는지와 벽이 막고 있는지를 검사합니다.</summary>
public sealed class EnemyNavigationSensor : MonoBehaviour
{
    // 지면과 벽으로 판정할 물리 레이어를 저장하는 변수입니다.
    [SerializeField] private LayerMask terrainLayers = 1 << 6;
    // 적 중심에서 진행 방향으로 검사할 앞쪽 거리를 저장하는 변수입니다.
    [SerializeField, Min(0.1f)] private float forwardDistance = 0.75f;
    // 앞쪽 검사 지점에서 아래로 지면을 찾을 거리를 저장하는 변수입니다.
    [SerializeField, Min(0.1f)] private float downwardDistance = 1.2f;
    // 적 중심에서 진행 방향으로 벽을 찾을 거리를 저장하는 변수입니다.
    [SerializeField, Min(0.1f)] private float wallDistance = 0.7f;

    /// <summary>진행 방향 앞에 지면이 있고 벽이 없어서 안전하게 이동할 수 있는지 확인합니다.</summary>
    /// <param name="direction">검사할 수평 방향입니다.</param>
    /// <returns>해당 방향으로 이동 가능한지 여부입니다.</returns>
    public bool CanMove(float direction)
    {
        float normalizedDirection = -1f; // 검사에 사용할 정규화된 수평 방향입니다.
        if (direction >= 0f)
        {
            normalizedDirection = 1f;
        }
        Vector2 groundOrigin = (Vector2)transform.position +
            Vector2.right * normalizedDirection * forwardDistance; // 낭떠러지 확인을 시작할 앞쪽 위치입니다.
        RaycastHit2D groundHit = Physics2D.Raycast(
            groundOrigin, Vector2.down, downwardDistance, terrainLayers); // 앞쪽 아래에서 감지한 지면 정보입니다.
        Vector2 wallOrigin = (Vector2)transform.position +
            Vector2.up * 0.05f; // 지면과 겹치지 않도록 약간 올린 벽 검사 시작 위치입니다.
        RaycastHit2D wallHit = Physics2D.Raycast(
            wallOrigin, Vector2.right * normalizedDirection, wallDistance,
            terrainLayers); // 진행 방향에서 감지한 벽 정보입니다.
        bool groundAhead = groundHit.collider != null; // 진행 방향 앞에 지면이 존재하는지 여부입니다.
        bool wallAhead = wallHit.collider != null; // 진행 방향 앞에 벽이 존재하는지 여부입니다.
        return groundAhead == true && wallAhead == false;
    }

    /// <summary>현재 위치에서 목표 위치까지 지형에 가로막히지 않은 직선 시야가 있는지 확인합니다.</summary>
    /// <param name="targetPosition">시야 검사를 수행할 목표 월드 위치입니다.</param>
    /// <returns>목표까지 지형에 막히지 않았는지 여부입니다.</returns>
    public bool HasClearSight(Vector2 targetPosition)
    {
        RaycastHit2D sightHit = Physics2D.Linecast(
            transform.position, targetPosition, terrainLayers); // 현재 위치와 목표 사이에서 감지한 지형 정보입니다.
        return sightHit.collider == null;
    }
}
