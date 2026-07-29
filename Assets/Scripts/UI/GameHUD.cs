using UnityEngine;
using UnityEngine.UI;

    /// <summary>체력, 점수, 조작 안내와 게임 종료 화면을 표시합니다.</summary>
    public sealed class GameHUD : MonoBehaviour
    {
        // 화면에 표시할 플레이어 체력을 저장하는 변수입니다.
        [SerializeField] private Health playerHealth;
        // 체력과 점수를 표시할 텍스트를 저장하는 변수입니다.
        [SerializeField] private Text statusText;
        // 조작 안내를 표시할 텍스트를 저장하는 변수입니다.
        [SerializeField] private Text messageText;
        // 게임 종료 화면 오브젝트를 저장하는 변수입니다.
        [SerializeField] private GameObject endScreen;
        // 게임 종료 제목을 표시할 텍스트를 저장하는 변수입니다.
        [SerializeField] private Text endTitleText;
        // 게임 종료 후 안내를 표시할 텍스트를 저장하는 변수입니다.
        [SerializeField] private Text endGuideText;

        /// <summary>게임 종료 화면이 현재 표시 중인지 제공합니다.</summary>
        public bool EndScreenVisible => endScreen != null && endScreen.activeSelf;
        /// <summary>현재 HUD 상태 텍스트에 표시된 문자열을 제공합니다.</summary>
        public string StatusDisplay
        {
            get
            {
                if (statusText == null)
                {
                    return string.Empty;
                }
                return statusText.text;
            }
        }

        /// <summary>HUD가 사용할 체력과 UI 오브젝트 참조를 설정합니다.</summary>
        /// <param name="health">표시할 플레이어 체력입니다.</param>
        /// <param name="status">체력과 점수 상태 텍스트입니다.</param>
        /// <param name="message">조작 안내 텍스트입니다.</param>
        /// <param name="screen">게임 종료 화면 오브젝트입니다.</param>
        /// <param name="title">게임 종료 제목 텍스트입니다.</param>
        /// <param name="guide">게임 종료 안내 텍스트입니다.</param>
        public void Configure(Health health, Text status, Text message,
            GameObject screen, Text title, Text guide)
        {
            playerHealth = health;
            statusText = status;
            messageText = message;
            endScreen = screen;
            endTitleText = title;
            endGuideText = guide;
        }

        /// <summary>HUD 이벤트를 등록하고 초기 화면 내용을 표시합니다.</summary>
        private void Start()
        {
            playerHealth.Changed += OnHealthChanged;
            GameManager.Instance.ScoreChanged += OnScoreChanged;
            GameManager.Instance.GameFinished += OnFinished;
            Refresh();
            messageText.text = "A/D or Arrows: Move    Space: Jump    R: Restart";
            endScreen.SetActive(false);
        }

        /// <summary>HUD가 등록한 체력 및 게임 상태 이벤트를 해제합니다.</summary>
        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.Changed -= OnHealthChanged;
            }
            if (GameManager.Instance == null)
            {
                return;
            }
            GameManager.Instance.ScoreChanged -= OnScoreChanged;
            GameManager.Instance.GameFinished -= OnFinished;
        }

        /// <summary>체력이 변경되면 상태 텍스트를 갱신합니다.</summary>
        /// <param name="currentHealth">변경된 현재 체력입니다.</param>
        /// <param name="maximumHealth">플레이어의 최대 체력입니다.</param>
        private void OnHealthChanged(int currentHealth, int maximumHealth)
        {
            Refresh();
        }
        /// <summary>점수가 변경되면 상태 텍스트를 갱신합니다.</summary>
        /// <param name="currentScore">변경된 현재 점수입니다.</param>
        private void OnScoreChanged(int currentScore)
        {
            Refresh();
        }
        /// <summary>현재 체력과 점수를 상태 텍스트에 표시합니다.</summary>
        private void Refresh()
        {
            string healthDisplay =
                $"HP: {playerHealth.Current}/{playerHealth.Max}"; // HUD에 표시할 현재 체력 문자열입니다.
            string starDisplay =
                $"Stars: {GameManager.Instance.Score}/{GameManager.Instance.TotalCollectibles}"; // HUD에 표시할 Star 획득 현황 문자열입니다.
            statusText.text = healthDisplay + "    " + starDisplay;
        }
        /// <summary>승패 결과에 맞는 게임 종료 화면을 표시합니다.</summary>
        /// <param name="won">승리 여부입니다.</param>
        private void OnFinished(bool won)
        {
            if (won == true)
            {
                endTitleText.text = "LEVEL CLEAR!";
                endTitleText.color = new Color(0.35f, 1f, 0.55f);
                endGuideText.text = "Press R to play again";
            }
            else
            {
                endTitleText.text = "GAME OVER";
                endTitleText.color = new Color(1f, 0.32f, 0.28f);
                endGuideText.text = "Press R to retry";
            }
            messageText.text = string.Empty;
            endScreen.SetActive(true);
        }
    }
