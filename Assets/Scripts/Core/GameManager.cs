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
        // 씬에 배치된 전체 수집 아이템 수를 저장하고 직렬화하는 변수입니다.
        [SerializeField, Min(0)] private int totalCollectibles;
        /// <summary>현재 획득 점수를 제공합니다.</summary>
        public int Score { get; private set; }
        /// <summary>씬에 배치된 전체 수집 아이템 수를 제공합니다.</summary>
        public int TotalCollectibles => totalCollectibles;
        /// <summary>게임 종료 여부를 제공합니다.</summary>
        public bool GameEnded { get; private set; }
        /// <summary>점수가 변경될 때 호출되는 이벤트입니다.</summary>
        public event Action<int> ScoreChanged;
        /// <summary>게임이 승리 또는 패배로 끝날 때 호출되는 이벤트입니다.</summary>
        public event Action<bool> GameFinished;

        /// <summary>플레이어 체력과 전체 수집 아이템 수를 설정합니다.</summary>
        /// <param name="health">관리할 플레이어 체력입니다.</param>
        /// <param name="totalCollectibles">씬의 전체 수집 아이템 수입니다.</param>
        public void Configure(Health health, int totalCollectibles = 0)
        {
            playerHealth = health;
            this.totalCollectibles = Mathf.Max(0, totalCollectibles);
        }

        /// <summary>싱글턴과 시간 배율을 초기화합니다.</summary>
        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
        }

        /// <summary>플레이어 사망 이벤트를 등록하고 초기 점수를 알립니다.</summary>
        private void Start()
        {
            if (playerHealth != null)
            {
                playerHealth.Died += Lose;
            }
            ScoreChanged?.Invoke(Score);
        }

        /// <summary>등록한 이벤트와 싱글턴 참조를 정리합니다.</summary>
        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= Lose;
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
            Finish(true);
        }
        /// <summary>게임을 패배 상태로 종료합니다.</summary>
        private void Lose()
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
