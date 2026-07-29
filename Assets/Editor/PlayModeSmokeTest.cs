using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
/// <summary>주요 게임 기능을 실제 플레이 모드에서 자동으로 검증합니다.</summary>
public static class PlayModeSmokeTest
{
    // 테스트 실행 상태를 저장할 세션 키입니다.
    private const string RunningKey = "AcademyPlatformer.SmokeRunning";
    // 테스트 실패 상태를 저장할 세션 키입니다.
    private const string FailedKey = "AcademyPlatformer.SmokeFailed";
    // 테스트 시작 시각을 저장할 세션 키입니다.
    private const string StartKey = "AcademyPlatformer.SmokeStart";
    // 기본 검사 완료 상태를 저장할 세션 키입니다.
    private const string BasicCheckedKey = "AcademyPlatformer.BasicChecked";
    // 피격 검사 완료 상태를 저장할 세션 키입니다.
    private const string DamageCheckedKey = "AcademyPlatformer.DamageChecked";
    // 게임오버 검사 완료 상태를 저장할 세션 키입니다.
    private const string GameOverCheckedKey = "AcademyPlatformer.GameOverChecked";
    // 밟기 검사 완료 상태를 저장할 세션 키입니다.
    private const string StompCheckedKey = "AcademyPlatformer.StompChecked";
    // 밟기 검사 준비 상태를 저장할 세션 키입니다.
    private const string StompSetupKey = "AcademyPlatformer.StompSetup";
    // 밟기 직전 체력을 저장할 세션 키입니다.
    private const string StompHealthKey = "AcademyPlatformer.StompHealth";

    /// <summary>도메인 재로드 후 진행 중인 테스트 이벤트를 다시 연결합니다.</summary>
    static PlayModeSmokeTest()
    {
        if (SessionState.GetBool(RunningKey, false) == false)
        {
            return;
        }
        Application.logMessageReceived -= OnLog;
        Application.logMessageReceived += OnLog;
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    /// <summary>메인 씬을 열고 플레이 모드 자동 검사를 시작합니다.</summary>
    public static void Run()
    {
        SessionState.SetBool(RunningKey, true);
        SessionState.SetBool(FailedKey, false);
        SessionState.SetBool(BasicCheckedKey, false);
        SessionState.SetBool(DamageCheckedKey, false);
        SessionState.SetBool(GameOverCheckedKey, false);
        SessionState.SetBool(StompCheckedKey, false);
        SessionState.SetBool(StompSetupKey, false);
        SessionState.SetFloat(StartKey, (float)EditorApplication.timeSinceStartup);
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        Application.logMessageReceived += OnLog;
        EditorApplication.update += Update;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>플레이 모드 진입 시 테스트 기준 시각을 다시 설정합니다.</summary>
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            SessionState.SetFloat(StartKey, (float)EditorApplication.timeSinceStartup);
        }
    }

    /// <summary>오류, 예외와 검증 실패 로그를 테스트 실패로 기록합니다.</summary>
    private static void OnLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
        {
            SessionState.SetBool(FailedKey, true);
        }
    }

    /// <summary>경과 시간에 따라 각 게임 기능을 단계별로 검사합니다.</summary>
    private static void Update()
    {
        if (SessionState.GetBool(RunningKey, false) == false)
        {
            return;
        }
        double elapsed = EditorApplication.timeSinceStartup - SessionState.GetFloat(StartKey, 0f); // elapsed 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        if (EditorApplication.isPlaying == true && elapsed >= 2d &&
            SessionState.GetBool(BasicCheckedKey, false) == false)
        {
            SessionState.SetBool(BasicCheckedKey, true);
            GameObject player = GameObject.FindGameObjectWithTag("Player"); // player 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            if (player == null || player.transform.position.y < -3.1f)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Player landing test failed: the player fell through the starting platform.");
            }
            Animator playerAnimator = player != null ? player.GetComponentInChildren<Animator>() : null; // playerAnimator 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            if (playerAnimator == null || player.GetComponent<PlayerAnimationController>() == null)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Player animation test failed: Animator or state driver is missing.");
            }
            else if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") == false)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Player animation test failed: an untouched grounded player must be in Idle.");
            }
            PlayerJump playerJump = // playerJump 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                player != null ? player.GetComponent<PlayerJump>() : null;
            Rigidbody2D playerBody = player != null ? player.GetComponent<Rigidbody2D>() : null; // playerBody 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            float jumpHeight = playerJump != null && playerBody != null // jumpHeight 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                ? playerJump.JumpSpeed * playerJump.JumpSpeed /
                  (2f * Mathf.Abs(Physics2D.gravity.y * playerBody.gravityScale))
                : 0f;
            if (jumpHeight < 3.4f)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Jump reach test failed: the player cannot reach the raised platforms.");
            }
            Collider2D playerCollider = player != null ? player.GetComponent<Collider2D>() : null; // playerCollider 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            PhysicsMaterial2D playerMaterial = // playerMaterial 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                playerCollider != null ? playerCollider.sharedMaterial : null;
            if (playerMaterial == null || Mathf.Approximately(playerMaterial.friction, 0f) == false ||
                Mathf.Approximately(playerMaterial.bounciness, 0f) == false)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Player physics-material test failed: a frictionless collider is required.");
            }

            PatrolEnemy enemy = // enemy 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                Object.FindFirstObjectByType<PatrolEnemy>();
            Animator enemyAnimator = enemy != null ? enemy.GetComponentInChildren<Animator>() : null; // enemyAnimator 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            if (enemyAnimator == null || enemy.GetComponent<EnemyAnimationController>() == null ||
                enemy.GetComponent<EnemyNavigationSensor>() == null)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Enemy test failed: animation or navigation setup is missing.");
            }
            else if (enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Run") == false)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Enemy animation test failed: a moving patrol enemy must be in Run.");
            }
            ChasingEnemy chasingEnemy = Object.FindFirstObjectByType<ChasingEnemy>(); // 씬에 배치된 추적형 적입니다.
            if (chasingEnemy == null || chasingEnemy.GetComponent<StompableEnemy>() == null ||
                chasingEnemy.GetComponent<DamageDealer>() == null ||
                chasingEnemy.GetComponentInChildren<Animator>() == null ||
                chasingEnemy.GetComponent<EnemyNavigationSensor>() == null)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Chasing enemy test failed: behavior, combat, or animation setup is missing.");
            }
            if (chasingEnemy != null)
            {
                EnemyNavigationSensor navigationSensor =
                    chasingEnemy.GetComponent<EnemyNavigationSensor>(); // 낭떠러지 감지를 검증할 이동 센서입니다.
                Vector3 originalPosition = chasingEnemy.transform.position; // 검증 후 복원할 추적형 적의 원래 위치입니다.
                chasingEnemy.transform.position = new Vector3(29.6f, -2.2f);
                Physics2D.SyncTransforms();
                bool canRunOverEdge = navigationSensor != null &&
                    navigationSensor.CanMove(1f); // 오른쪽 낭떠러지 너머로 이동 가능하다고 잘못 판단하는지 여부입니다.
                chasingEnemy.transform.position = originalPosition;
                Physics2D.SyncTransforms();
                bool clearHorizontalSight = navigationSensor != null &&
                    navigationSensor.HasClearSight(
                        originalPosition + Vector3.right); // 가까운 수평 위치까지 시야가 열려 있는지 여부입니다.
                bool sightThroughGround = navigationSensor != null &&
                    navigationSensor.HasClearSight(
                        originalPosition + Vector3.down * 3f); // 지형을 통과한 위치까지 잘못 볼 수 있는지 여부입니다.
                bool chaseSpeedValid =
                    chasingEnemy.ChaseSpeed > chasingEnemy.PatrolSpeed; // 추적 속도가 순찰 속도보다 빠른지 여부입니다.
                if (canRunOverEdge == true || clearHorizontalSight == false ||
                    sightThroughGround == true || chaseSpeedValid == false)
                {
                    SessionState.SetBool(FailedKey, true);
                    Debug.LogError("Enemy navigation test failed: ledge, sight, or chase-speed behavior is invalid.");
                }
            }
            StompableEnemy[] placedEnemies =
                Object.FindObjectsByType<StompableEnemy>(FindObjectsSortMode.None); // 높이별 배치를 검사할 모든 적입니다.
            int elevatedEnemyCount = 0; // 지면보다 높은 발판에 배치된 적의 수입니다.
            foreach (StompableEnemy placedEnemy in placedEnemies)
            {
                if (placedEnemy.transform.position.y > -1f)
                {
                    elevatedEnemyCount++;
                }
            }
            if (elevatedEnemyCount < 3)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError(
                    $"Enemy placement test failed: only {elevatedEnemyCount} enemies remain on elevated platforms.");
            }
            ChasingEnemy[] chasingEnemies =
                Object.FindObjectsByType<ChasingEnemy>(FindObjectsSortMode.None); // 테스트 간섭을 막기 위해 일시 정지할 모든 추적형 적입니다.
            foreach (ChasingEnemy chasingBehaviour in chasingEnemies)
            {
                chasingBehaviour.enabled = false;
            }

            Tilemap terrain = Object.FindFirstObjectByType<Tilemap>(); // terrain 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            if (terrain == null || terrain.GetUsedTilesCount() < 1 ||
                terrain.GetComponent<TilemapCollider2D>() == null ||
                terrain.GetComponent<CompositeCollider2D>() == null)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Tilemap test failed: rendered tiles or composite terrain collision is missing.");
            }
            Collectible[] collectibles = // collectibles 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None);
            GameHUD gameHud = Object.FindFirstObjectByType<GameHUD>(); // Star 총 개수 표시를 확인할 HUD입니다.
            GameManager gameManager = GameManager.Instance; // 배치된 Star 총 개수를 관리하는 게임 관리자입니다.
            string expectedStarTotal =
                "/" + collectibles.Length; // HUD에 표시되어야 하는 Star 총 개수 문자열입니다.
            if (gameManager == null ||
                gameManager.TotalCollectibles != collectibles.Length ||
                gameHud == null ||
                gameHud.StatusDisplay.Contains(expectedStarTotal) == false)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Star total test failed: HUD does not show the serialized collectible count.");
            }
            foreach (Collectible collectible in collectibles)
            {
                if (Physics2D.OverlapPoint(collectible.transform.position, 1 << 6) == null)
                {
                    continue;
                }
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Collectible placement test failed: a Star center is inside terrain.");
                break;
            }

            ParallaxLayer[] parallaxLayers = // parallaxLayers 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                Object.FindObjectsByType<ParallaxLayer>(FindObjectsSortMode.None);
            if (parallaxLayers.Length != 3 ||
                Mathf.Approximately(parallaxLayers[0].MovementFactor, parallaxLayers[1].MovementFactor) ||
                Mathf.Approximately(parallaxLayers[1].MovementFactor, parallaxLayers[2].MovementFactor))
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Parallax test failed: three layers with different movement factors are required.");
            }
            foreach (ParallaxLayer parallaxLayer in parallaxLayers)
            {
                SpriteRenderer backgroundRenderer =
                    parallaxLayer.GetComponent<SpriteRenderer>(); // 세로 화면 범위를 검사할 배경 렌더러입니다.
                bool originalHeightPreserved = backgroundRenderer != null &&
                    backgroundRenderer.sprite != null &&
                    Mathf.Approximately(
                        backgroundRenderer.size.y,
                        backgroundRenderer.sprite.bounds.size.y); // 세로 타일 반복 없이 원본 높이를 유지하는지 여부입니다.
                if (originalHeightPreserved == false)
                {
                    SessionState.SetBool(FailedKey, true);
                    Debug.LogError("Background tiling test failed: vertical sprite repetition must be disabled.");
                    break;
                }
            }
            AudioListener[] listeners = // listeners 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (listeners.Length != 1 || listeners[0].isActiveAndEnabled == false)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Audio output test failed: exactly one active AudioListener is required.");
            }
        }
        if (EditorApplication.isPlaying == true && elapsed >= 2.5d &&
            SessionState.GetBool(DamageCheckedKey, false) == false)
        {
            SessionState.SetBool(DamageCheckedKey, true);
            GameObject player = GameObject.FindGameObjectWithTag("Player"); // player 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            PlayerDamageFeedback feedback = // feedback 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                player != null ? player.GetComponent<PlayerDamageFeedback>() : null;
            Health health = // health 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                player != null ? player.GetComponent<Health>() : null;
            PlayerAudioFeedback audioFeedback = // audioFeedback 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                player != null ? player.GetComponent<PlayerAudioFeedback>() : null;
            AudioSource audioSource = player != null ? player.GetComponent<AudioSource>() : null; // audioSource 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            Rigidbody2D body = player != null ? player.GetComponent<Rigidbody2D>() : null; // body 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            SpriteRenderer sprite = player != null ? player.GetComponentInChildren<SpriteRenderer>() : null; // sprite 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            Vector2 source = player != null // source 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                ? (Vector2)player.transform.position + Vector2.right
                : Vector2.zero;
            bool accepted = health != null && health.TakeDamage(1, source); // accepted 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            if (feedback == null || audioFeedback == null || accepted == false || body == null ||
                body.linearVelocity.x >= -0.5f || sprite == null || sprite.enabled)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Damage feedback test failed: knockback or invulnerability blinking is missing.");
            }
            if (audioSource == null || audioSource.isPlaying == false)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Audio feedback test failed: the damage sound did not play.");
            }
        }
        if (EditorApplication.isPlaying == true && elapsed >= 4.3d &&
            SessionState.GetBool(GameOverCheckedKey, false) == false)
        {
            SessionState.SetBool(GameOverCheckedKey, true);
            GameObject player = GameObject.FindGameObjectWithTag("Player"); // player 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            Health health = // health 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                player != null ? player.GetComponent<Health>() : null;
            GameHUD hud =
                Object.FindFirstObjectByType<GameHUD>();
            FallDeathZone fallDeathZone =
                Object.FindFirstObjectByType<FallDeathZone>(); // 씬에 배치된 추락 사망 구역입니다.
            Collider2D playerCollider =
                player != null ? player.GetComponent<Collider2D>() : null; // 추락 사망 검증에 사용할 플레이어 콜라이더입니다.
            bool accepted = fallDeathZone != null && playerCollider != null &&
                fallDeathZone.TryKill(playerCollider); // 추락 사망 처리가 실제로 적용되었는지 여부입니다.
            if (accepted == false || GameManager.Instance == null ||
                GameManager.Instance.GameEnded == false ||
                hud == null || hud.EndScreenVisible == false ||
                Mathf.Approximately(Time.timeScale, 0f) == false)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Game-over test failed: death must pause play and display the end screen.");
            }
        }
        if (EditorApplication.isPlaying == true && elapsed >= 3.4d &&
            SessionState.GetBool(StompSetupKey, false) == false)
        {
            SessionState.SetBool(StompSetupKey, true);
            GameObject player = GameObject.FindGameObjectWithTag("Player"); // player 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            PatrolEnemy selectedPatrol = Object.FindFirstObjectByType<PatrolEnemy>(); // 밟기 검증에 사용할 이동 가능한 순찰형 적입니다.
            StompableEnemy target = selectedPatrol != null // target 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                ? selectedPatrol.GetComponent<StompableEnemy>()
                : null;
            Rigidbody2D body = player != null ? player.GetComponent<Rigidbody2D>() : null; // body 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            Health health = // health 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                player != null ? player.GetComponent<Health>() : null;
            PatrolEnemy patrol = // patrol 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                target != null ? target.GetComponent<PatrolEnemy>() : null;
            if (patrol != null)
            {
                patrol.enabled = false;
            }
            if (player != null && target != null && body != null)
            {
                player.transform.position = target.transform.position + Vector3.up * 2f;
                body.linearVelocity = new Vector2(0f, -8f);
            }
            SessionState.SetInt(StompHealthKey, health != null ? health.Current : -1);
        }
        if (EditorApplication.isPlaying == true && elapsed >= 4d &&
            SessionState.GetBool(StompCheckedKey, false) == false)
        {
            SessionState.SetBool(StompCheckedKey, true);
            GameObject player = GameObject.FindGameObjectWithTag("Player"); // player 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            PlayerStompAttack attack = // attack 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                player != null ? player.GetComponent<PlayerStompAttack>() : null;
            Health health = // health 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                player != null ? player.GetComponent<Health>() : null;
            PlayerAudioFeedback audioFeedback = // audioFeedback 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
                player != null ? player.GetComponent<PlayerAudioFeedback>() : null;
            int healthBefore = SessionState.GetInt(StompHealthKey, -1); // healthBefore 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
            if (attack == null || attack.SuccessfulStomps < 1)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Stomp attack test failed: a real top collision did not defeat the enemy.");
            }
            if (health == null || health.Current != healthBefore)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Stomp safety test failed: a real top collision damaged the player.");
            }
            if (audioFeedback == null || audioFeedback.StompSoundPlayCount < 1)
            {
                SessionState.SetBool(FailedKey, true);
                Debug.LogError("Stomp audio test failed: a successful stomp did not play its sound.");
            }
        }
        if (EditorApplication.isPlaying == true && elapsed >= 5d)
        {
            EditorApplication.ExitPlaymode();
            return;
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode == true || elapsed < 5d)
        {
            return;
        }

        bool failed = SessionState.GetBool(FailedKey, false); // failed 값을 이 처리 단계에서 사용하기 위해 저장하는 지역 변수입니다.
        SessionState.SetBool(RunningKey, false);
        Application.logMessageReceived -= OnLog;
        EditorApplication.update -= Update;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        Debug.Log(failed ? "PLAY_MODE_SMOKE_TEST_FAILED" : "PLAY_MODE_SMOKE_TEST_PASSED");
        EditorApplication.Exit(failed ? 1 : 0);
    }
}
