using System;
using UnityEngine;
using UnityEngine.SceneManagement;

    /// <summary>점수, 승패, 재시작과 같은 전체 게임 상태를 관리합니다.</summary>
    public sealed class GameManager : MonoBehaviour
    {
        /// <summary>현재 씬의 게임 관리자 인스턴스를 제공합니다.</summary>
        public static GameManager Instance { get; private set; }
        // 플레이어 체력 컴포넌트를 저장하는 변수입니다.
        [SerializeField] private Health playerHealth;
        // 플레이어가 사망했을 때 게임 종료 전에 재생할 연출 컴포넌트입니다.
        [SerializeField] private PlayerDeathSequence playerDeathSequence;
        // 레벨 클리어 후 UI 표시 전에 재생할 플레이어 연출 컴포넌트입니다.
        [SerializeField] private PlayerClearSequence playerClearSequence;
        // 씬에 배치된 전체 수집 아이템 수를 저장하고 직렬화하는 변수입니다.
        [SerializeField, Min(0)] private int totalCollectibles;
        // 현재 플레이 중인 스테이지 번호입니다.
        [SerializeField, Min(1)] private int stageNumber = 1;
        /// <summary>현재 획득 점수를 제공합니다.</summary>
        public int Score { get; private set; }
        /// <summary>씬에 배치된 전체 수집 아이템 수를 제공합니다.</summary>
        public int TotalCollectibles => totalCollectibles;
        /// <summary>현재 플레이 중인 스테이지 번호를 제공합니다.</summary>
        public int StageNumber => stageNumber;
        /// <summary>게임 종료 여부를 제공합니다.</summary>
        public bool GameEnded { get; private set; }
        /// <summary>점수가 변경될 때 호출되는 이벤트입니다.</summary>
        public event Action<int> ScoreChanged;
        /// <summary>게임이 승리 또는 패배로 끝날 때 호출되는 이벤트입니다.</summary>
        public event Action<bool> GameFinished;
        /// <summary>플레이어 사망 연출이 시작될 때 호출되는 이벤트입니다.</summary>
        public event Action DeathSequenceStarted;
        /// <summary>레벨 클리어 연출이 시작될 때 호출되는 이벤트입니다.</summary>
        public event Action ClearSequenceStarted;

        /// <summary>플레이어 체력과 전체 수집 아이템 수를 설정합니다.</summary>
        /// <param name="health">관리할 플레이어 체력입니다.</param>
        /// <param name="totalCollectibles">씬의 전체 수집 아이템 수입니다.</param>
        public void Configure(
            Health health,
            int totalCollectibles = 0,
            PlayerDeathSequence deathSequence = null,
            PlayerClearSequence clearSequence = null,
            int currentStageNumber = 1)
        {
            playerHealth = health;
            this.totalCollectibles = Mathf.Max(0, totalCollectibles);
            playerDeathSequence = deathSequence;
            playerClearSequence = clearSequence;
            stageNumber = Mathf.Clamp(
                currentStageNumber,
                1,
                StageProgressData.TotalStageCount);
        }

        /// <summary>싱글턴과 시간 배율을 초기화합니다.</summary>
        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
            string sceneName =
                SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Stage") == true)
            {
                string numberText =
                    sceneName.Substring("Stage".Length);
                int parsedStageNumber;
                if (int.TryParse(
                    numberText,
                    out parsedStageNumber) == true)
                {
                    stageNumber = Mathf.Clamp(
                        parsedStageNumber,
                        1,
                        StageProgressData.TotalStageCount);
                }
            }
        }

        /// <summary>플레이어 사망 이벤트를 등록하고 초기 점수를 알립니다.</summary>
        private void Start()
        {
            if (playerHealth != null)
            {
                playerHealth.Died += BeginLoseSequence;
            }
            if (playerDeathSequence != null)
            {
                playerDeathSequence.Completed += CompleteLoseSequence;
            }
            if (playerClearSequence != null)
            {
                playerClearSequence.Completed += CompleteWinSequence;
            }
            ScoreChanged?.Invoke(Score);
        }

        /// <summary>등록한 이벤트와 싱글턴 참조를 정리합니다.</summary>
        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= BeginLoseSequence;
            }
            if (playerDeathSequence != null)
            {
                playerDeathSequence.Completed -= CompleteLoseSequence;
            }
            if (playerClearSequence != null)
            {
                playerClearSequence.Completed -= CompleteWinSequence;
            }
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>재시작 및 종료 키 입력을 처리합니다.</summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) == true)
            {
                Restart();
            }
            if (Input.GetKeyDown(KeyCode.Escape) == true)
            {
                Application.Quit();
            }
        }

        /// <summary>게임이 진행 중일 때 점수를 추가합니다.</summary>
        /// <param name="amount">추가할 점수입니다.</param>
        public void AddScore(int amount)
        {
            if (GameEnded == true)
            {
                return;
            }
            Score += amount;
            ScoreChanged?.Invoke(Score);
        }

        /// <summary>게임을 승리 상태로 종료합니다.</summary>
        public void Win()
        {
            if (GameEnded == true)
            {
                return;
            }
            ClearSequenceStarted?.Invoke();
            if (playerClearSequence != null)
            {
                playerClearSequence.Play();
            }
            else
            {
                CompleteWinSequence();
            }
        }
        /// <summary>플레이어 클리어 연출이 끝난 뒤 실제 승리 상태로 전환합니다.</summary>
        private void CompleteWinSequence()
        {
            StageProgressData.RecordStageClear(stageNumber);
            Finish(true);
        }
        /// <summary>게임을 패배 상태로 종료합니다.</summary>
        private void BeginLoseSequence()
        {
            if (GameEnded == true)
            {
                return;
            }
            DeathSequenceStarted?.Invoke();
            Time.timeScale = 0f;
            if (playerDeathSequence != null)
            {
                playerDeathSequence.Play();
            }
            else
            {
                CompleteLoseSequence();
            }
        }
        /// <summary>플레이어 사망 연출이 끝난 뒤 실제 게임오버 상태로 전환합니다.</summary>
        private void CompleteLoseSequence()
        {
            Finish(false);
        }
        /// <summary>승패 상태를 알리고 게임 시간을 정지합니다.</summary>
        /// <param name="won">승리 여부입니다.</param>
        private void Finish(bool won)
        {
            if (GameEnded == true)
            {
                return;
            }
            GameEnded = true;
            GameFinished?.Invoke(won);
            Time.timeScale = 0f;
        }

        /// <summary>현재 씬을 다시 불러와 게임을 재시작합니다.</summary>
        public void Restart()
        {
            Time.timeScale = 1f;
            SceneFadeController.LoadSceneWithFade(
                SceneManager.GetActiveScene().name);
        }
    }
