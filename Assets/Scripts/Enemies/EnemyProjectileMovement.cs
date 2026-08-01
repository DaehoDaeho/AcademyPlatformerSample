using UnityEngine;

/// <summary>
/// 적이 발사한 투사체를 일정한 속도로 이동시키고 수명이 끝나면 제거합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyProjectileMovement : MonoBehaviour
{
    /// <summary>
    /// 플레이어가 보고 점프로 피할 수 있도록 제한한 투사체 이동 속도입니다.
    /// </summary>
    [SerializeField, Min(0.5f)] private float moveSpeed = 3.2f;

    /// <summary>
    /// 화면 밖에 투사체가 계속 남지 않도록 제한하는 생존 시간입니다.
    /// </summary>
    [SerializeField, Min(0.5f)] private float lifeTime = 4f;

    /// <summary>
    /// 투사체 이동에 사용하는 물리 본체입니다.
    /// </summary>
    private Rigidbody2D body;

    /// <summary>
    /// 현재 투사체가 이동하는 수평 방향입니다.
    /// </summary>
    private float moveDirection = -1f;

    /// <summary>
    /// 설정된 투사체 속도를 외부 검증 코드에 제공합니다.
    /// </summary>
    public float MoveSpeed => moveSpeed;

    /// <summary>
    /// 필요한 Rigidbody2D 참조를 가져옵니다.
    /// </summary>
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 투사체의 이동 방향을 설정하고 제한 시간이 지나면 자동 제거되게 합니다.
    /// </summary>
    /// <param name="direction">음수이면 왼쪽, 양수이면 오른쪽 방향입니다.</param>
    public void Launch(float direction)
    {
        if (direction >= 0f)
        {
            moveDirection = 1f;
        }
        else
        {
            moveDirection = -1f;
        }

        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// 물리 프레임마다 투사체에 일정한 수평 속도를 적용합니다.
    /// </summary>
    private void FixedUpdate()
    {
        body.linearVelocity = Vector2.right * moveDirection * moveSpeed;
    }
}
