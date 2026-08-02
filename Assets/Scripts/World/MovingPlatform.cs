using System.Collections.Generic;
using UnityEngine;

/// <summary>설정된 여러 경로 지점을 순서대로 반복 이동하며 탑승한 플레이어를 함께 운반합니다.</summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[DefaultExecutionOrder(200)]
public sealed class MovingPlatform : MonoBehaviour
{
    // 이동 플랫폼이 순서대로 방문할 월드 좌표 목록입니다.
    [SerializeField] private Vector2[] pathPoints;
    // 이동 플랫폼이 초당 이동할 거리입니다.
    [SerializeField, Min(0.1f)] private float moveSpeed = 2f;
    // 각 경로 지점에 도착한 뒤 기다리는 시간입니다.
    [SerializeField, Min(0f)] private float waitDuration = 0.5f;
    // 이동 플랫폼의 물리 이동을 담당하는 Rigidbody2D입니다.
    private Rigidbody2D platformBody;
    // 플레이어가 플랫폼 위에 있는지 판단할 때 사용하는 Collider2D입니다.
    private Collider2D platformCollider;
    // 현재 이동 목표로 사용하는 경로 지점의 인덱스입니다.
    private int targetPointIndex = 1;
    // 다음 이동을 시작하기 전까지 남은 대기 시간입니다.
    private float waitTimer;
    // 현재 플랫폼 위에서 함께 이동해야 하는 Rigidbody2D 목록입니다.
    private readonly List<Rigidbody2D> passengerBodies =
        new List<Rigidbody2D>();

    /// <summary>이동 경로와 속도 및 경로 지점 대기 시간을 설정합니다.</summary>
    /// <param name="points">순서대로 반복 방문할 월드 좌표 목록입니다.</param>
    /// <param name="speed">플랫폼의 초당 이동 거리입니다.</param>
    /// <param name="waitTime">각 지점에 도착한 뒤 기다릴 시간입니다.</param>
    public void Configure(
        Vector2[] points,
        float speed,
        float waitTime)
    {
        pathPoints = points;
        moveSpeed = Mathf.Max(0.1f, speed);
        waitDuration = Mathf.Max(0f, waitTime);
        targetPointIndex = 1;
        waitTimer = 0f;
    }

    /// <summary>필요한 물리 컴포넌트를 가져오고 플랫폼을 첫 번째 경로 지점에 배치합니다.</summary>
    private void Awake()
    {
        platformBody = GetComponent<Rigidbody2D>();
        platformCollider = GetComponent<Collider2D>();
        if (pathPoints != null && pathPoints.Length > 0)
        {
            platformBody.position = pathPoints[0];
        }
    }

    /// <summary>고정 물리 주기마다 다음 경로 지점으로 이동하고 탑승자를 같은 거리만큼 운반합니다.</summary>
    private void FixedUpdate()
    {
        bool hasUsablePath =
            pathPoints != null && pathPoints.Length >= 2;
        if (hasUsablePath == false)
        {
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 currentPosition = platformBody.position;
        Vector2 targetPosition = pathPoints[targetPointIndex];
        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime);
        Vector2 movement = nextPosition - currentPosition;
        MovePassengers(movement);
        platformBody.MovePosition(nextPosition);

        bool reachedTarget =
            Vector2.Distance(nextPosition, targetPosition) <= 0.01f;
        if (reachedTarget == true)
        {
            targetPointIndex++;
            if (targetPointIndex >= pathPoints.Length)
            {
                targetPointIndex = 0;
            }

            waitTimer = waitDuration;
        }
    }

    /// <summary>플랫폼 위에 있는 모든 탑승자를 플랫폼의 이동량만큼 함께 이동시킵니다.</summary>
    /// <param name="movement">이번 물리 주기에 플랫폼이 이동할 월드 거리입니다.</param>
    private void MovePassengers(Vector2 movement)
    {
        float carriedVerticalMovement = 0f;
        if (movement.y < 0f)
        {
            carriedVerticalMovement = movement.y;
        }

        Vector2 passengerMovement = new Vector2(
            movement.x,
            carriedVerticalMovement);
        foreach (Rigidbody2D passengerBody in passengerBodies)
        {
            if (passengerBody != null)
            {
                passengerBody.position =
                    passengerBody.position + passengerMovement;
            }
        }
    }

    /// <summary>충돌 중인 플레이어가 플랫폼 위에 서 있으면 탑승자 목록에 추가합니다.</summary>
    /// <param name="collision">플랫폼과 계속 접촉 중인 충돌 정보입니다.</param>
    private void OnCollisionStay2D(Collision2D collision)
    {
        Rigidbody2D passengerBody = collision.rigidbody;
        if (passengerBody == null)
        {
            return;
        }

        bool isPlayer = collision.gameObject.CompareTag("Player");
        if (isPlayer == false)
        {
            return;
        }

        Collider2D passengerCollider =
            collision.gameObject.GetComponent<Collider2D>();
        if (passengerCollider == null)
        {
            return;
        }

        float platformTop = platformCollider.bounds.max.y;
        float passengerBottom = passengerCollider.bounds.min.y;
        bool isStandingAbove = passengerBottom >= platformTop - 0.2f;
        if (isStandingAbove == true &&
            passengerBodies.Contains(passengerBody) == false)
        {
            passengerBodies.Add(passengerBody);
        }
    }

    /// <summary>플레이어가 플랫폼과 접촉을 끝내면 탑승자 목록에서 제거합니다.</summary>
    /// <param name="collision">플랫폼과 접촉이 끝난 충돌 정보입니다.</param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D passengerBody = collision.rigidbody;
        if (passengerBody != null)
        {
            passengerBodies.Remove(passengerBody);
        }
    }
}
