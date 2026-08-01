using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
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
        // 사망 연출 동안 화면을 서서히 어둡게 만드는 이미지입니다.
        [SerializeField] private Image deathFadeImage;
        // 현재 스테이지를 다시 시작하는 버튼입니다.
        [SerializeField] private Button restartButton;
        // 해금된 다음 스테이지로 이동하는 버튼입니다.
        [SerializeField] private Button nextStageButton;
        // 스테이지 선택 화면으로 돌아가는 버튼입니다.
        [SerializeField] private Button stageSelectButton;

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

        /// <summary>HUD 버튼에 포인터 입력을 전달할 EventSystem이 없으면 자동으로 생성합니다.</summary>
        private void Awake()
        {
            ConfigureOverlayCanvas();
            EnsureEventSystem();
        }

        /// <summary>HUD가 카메라 흑백 후처리의 영향을 받지 않도록 오버레이 캔버스로 설정합니다.</summary>
        private void ConfigureOverlayCanvas()
        {
            Canvas hudCanvas = GetComponent<Canvas>();
            if (hudCanvas == null)
            {
                return;
            }

            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.worldCamera = null;
            hudCanvas.sortingOrder = 100;
        }

        /// <summary>현재 씬에 EventSystem과 기본 UI 입력 모듈이 하나만 존재하도록 보장합니다.</summary>
        private void EnsureEventSystem()
        {
            EventSystem currentEventSystem =
                Object.FindFirstObjectByType<EventSystem>();
            if (currentEventSystem != null)
            {
                return;
            }

            GameObject eventSystemObject =
                new GameObject("Event System");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        /// <summary>HUD가 사용할 체력과 UI 오브젝트 참조를 설정합니다.</summary>
        /// <param name="health">표시할 플레이어 체력입니다.</param>
        /// <param name="status">체력과 점수 상태 텍스트입니다.</param>
        /// <param name="message">조작 안내 텍스트입니다.</param>
        /// <param name="screen">게임 종료 화면 오브젝트입니다.</param>
        /// <param name="title">게임 종료 제목 텍스트입니다.</param>
        /// <param name="guide">게임 종료 안내 텍스트입니다.</param>
        public void Configure(Health health, Text status, Text message,
            GameObject screen, Text title, Text guide, Image fadeImage,
            Button restart, Button nextStage, Button stageSelect)
        {
            playerHealth = health;
            statusText = status;
            messageText = message;
            endScreen = screen;
            endTitleText = title;
            endGuideText = guide;
            deathFadeImage = fadeImage;
            restartButton = restart;
            nextStageButton = nextStage;
            stageSelectButton = stageSelect;
        }

        /// <summary>HUD 이벤트를 등록하고 초기 화면 내용을 표시합니다.</summary>
        private void Start()
        {
            playerHealth.Changed += OnHealthChanged;
            GameManager.Instance.ScoreChanged += OnScoreChanged;
            GameManager.Instance.GameFinished += OnFinished;
            GameManager.Instance.DeathSequenceStarted += OnDeathSequenceStarted;
            GameManager.Instance.ClearSequenceStarted += OnClearSequenceStarted;
            Refresh();
            messageText.text = string.Empty;
            messageText.gameObject.SetActive(false);
            endGuideText.text = string.Empty;
            endGuideText.gameObject.SetActive(false);
            endScreen.SetActive(false);
            SetDeathFadeAlpha(0f);
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
            GameManager.Instance.DeathSequenceStarted -= OnDeathSequenceStarted;
            GameManager.Instance.ClearSequenceStarted -= OnClearSequenceStarted;
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
                nextStageButton.gameObject.SetActive(
                    GameManager.Instance.StageNumber <
                    StageProgressData.TotalStageCount);
            }
            else
            {
                endTitleText.text = "GAME OVER";
                endTitleText.color = new Color(1f, 0.32f, 0.28f);
                nextStageButton.gameObject.SetActive(false);
            }
            endScreen.SetActive(true);
        }

        /// <summary>현재 스테이지를 페이드 효과와 함께 다시 시작합니다.</summary>
        public void RestartStage()
        {
            GameManager.Instance.Restart();
        }

        /// <summary>현재 스테이지 다음 번호의 스테이지로 이동합니다.</summary>
        public void GoToNextStage()
        {
            int nextStageNumber =
                GameManager.Instance.StageNumber + 1;
            if (nextStageNumber > StageProgressData.TotalStageCount)
            {
                return;
            }

            SceneFadeController.LoadSceneWithFade(
                StageProgressData.GetStageSceneName(nextStageNumber));
        }

        /// <summary>페이드 효과와 함께 스테이지 선택 화면으로 이동합니다.</summary>
        public void GoToStageSelect()
        {
            SceneFadeController.LoadSceneWithFade("StageSelect");
        }

        /// <summary>플레이어 사망 연출이 시작되면 화면 어두워짐 효과를 재생합니다.</summary>
        private void OnDeathSequenceStarted()
        {
            StartCoroutine(FadeForDeath());
        }

        /// <summary>레벨 클리어 연출이 시작되면 밝은 금빛 화면 효과를 재생합니다.</summary>
        private void OnClearSequenceStarted()
        {
            StartCoroutine(FadeForClear());
        }

        /// <summary>사망 연출 시간에 맞춰 화면을 천천히 어둡게 만듭니다.</summary>
        /// <returns>프레임마다 투명도를 변경하는 코루틴 열거자를 반환합니다.</returns>
        private IEnumerator FadeForDeath()
        {
            // 화면 어두워짐 효과가 재생된 시간입니다.
            float elapsedTime = 0f;

            // 화면 어두워짐 효과가 완료되는 시간입니다.
            float fadeDuration = 1.2f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                // 현재 화면 어두워짐 효과의 진행 비율입니다.
                float fadeProgress = Mathf.Clamp01(elapsedTime / fadeDuration);

                SetDeathFadeAlpha(fadeProgress * 0.72f);
                yield return null;
            }
        }

        /// <summary>클리어 연출 시간에 맞춰 화면에 밝은 금빛을 서서히 더합니다.</summary>
        /// <returns>프레임마다 투명도를 변경하는 코루틴 열거자를 반환합니다.</returns>
        private IEnumerator FadeForClear()
        {
            // 클리어 화면 효과가 재생된 시간입니다.
            float elapsedTime = 0f;

            // 클리어 화면 효과가 완료되는 시간입니다.
            float fadeDuration = 1.3f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                // 현재 클리어 화면 효과의 진행 비율입니다.
                float fadeProgress = Mathf.Clamp01(elapsedTime / fadeDuration);

                if (deathFadeImage != null)
                {
                    deathFadeImage.color = new Color(
                        1f,
                        0.78f,
                        0.18f,
                        fadeProgress * 0.32f);
                }

                yield return null;
            }
        }

        /// <summary>사망 화면 페이드 이미지의 투명도를 설정합니다.</summary>
        /// <param name="alpha">0에서 1 사이의 이미지 투명도입니다.</param>
        private void SetDeathFadeAlpha(float alpha)
        {
            if (deathFadeImage == null)
            {
                return;
            }

            deathFadeImage.color = new Color(0.02f, 0.01f, 0.04f, Mathf.Clamp01(alpha));
        }
    }
