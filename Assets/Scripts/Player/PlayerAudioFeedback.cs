using UnityEngine;

    /// <summary>
    /// 플레이어와 관련된 효과음 재생을 담당합니다.
    /// 게임플레이 컴포넌트는 이벤트만 전달하고 실제 재생은 이 컴포넌트가 처리합니다.
    /// </summary>
    [RequireComponent(typeof(AudioSource), typeof(Rigidbody2D), typeof(PlayerJump))]
    public sealed class PlayerAudioFeedback : MonoBehaviour
    {
        // 점프 시 재생할 오디오 클립을 저장하는 변수입니다.
        [SerializeField] private AudioClip jumpClip;
        // 아이템 수집 시 재생할 오디오 클립을 저장하는 변수입니다.
        [SerializeField] private AudioClip collectibleClip;
        // 피해를 받을 때 재생할 오디오 클립을 저장하는 변수입니다.
        [SerializeField] private AudioClip damagedClip;
        // 적을 밟을 때 재생할 오디오 클립을 저장하는 변수입니다.
        [SerializeField] private AudioClip stompClip;

        // 효과음을 출력할 AudioSource를 저장하는 변수입니다.
        private AudioSource audioSource;
        // 점프 이벤트를 제공할 컴포넌트를 저장하는 변수입니다.
        private PlayerJump playerJump;
        // 피해 이벤트를 제공할 체력 컴포넌트를 저장하는 변수입니다.
        private Health health;
        // 밟기 성공 이벤트를 제공할 컴포넌트를 저장하는 변수입니다.
        private PlayerStompAttack stompAttack;
        /// <summary>밟기 효과음을 재생한 횟수를 제공합니다.</summary>
        public int StompSoundPlayCount { get; private set; }

        /// <summary>플레이어 행동별 오디오 클립을 설정합니다.</summary>
        /// <param name="jump">점프 효과음입니다.</param>
        /// <param name="collectible">아이템 수집 효과음입니다.</param>
        /// <param name="damaged">피격 효과음입니다.</param>
        /// <param name="stomp">밟기 공격 효과음입니다.</param>
        public void Configure(AudioClip jump, AudioClip collectible, AudioClip damaged, AudioClip stomp)
        {
            jumpClip = jump;
            collectibleClip = collectible;
            damagedClip = damaged;
            stompClip = stomp;
        }

        /// <summary>사운드 처리에 필요한 컴포넌트 참조를 가져옵니다.</summary>
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            playerJump = GetComponent<PlayerJump>();
            health = GetComponent<Health>();
            stompAttack = GetComponent<PlayerStompAttack>();
        }

        /// <summary>플레이어 행동 이벤트에 효과음 재생 함수를 등록합니다.</summary>
        private void OnEnable()
        {
            playerJump.Jumped += PlayJump;
            if (health != null)
            {
                health.Damaged += PlayDamaged;
            }
            if (stompAttack != null)
            {
                stompAttack.Stomped += PlayStomp;
            }
        }

        /// <summary>플레이어 행동 이벤트에서 효과음 재생 함수를 해제합니다.</summary>
        private void OnDisable()
        {
            if (playerJump != null)
            {
                playerJump.Jumped -= PlayJump;
            }
            if (health != null)
            {
                health.Damaged -= PlayDamaged;
            }
            if (stompAttack != null)
            {
                stompAttack.Stomped -= PlayStomp;
            }
        }

        /// <summary>아이템 수집 효과음을 재생합니다.</summary>
        public void PlayCollectible()
        {
            Play(collectibleClip);
        }

        /// <summary>회복 포션을 마시는 느낌의 밝고 높은 획득음을 재생합니다.</summary>
        public void PlayHealing()
        {
            PlayPitchedImpact(collectibleClip, 1.12f, 0.82f);
        }

        /// <summary>적 접촉 종류에 따라 일반 또는 강한 충돌음을 겹쳐 재생합니다.</summary>
        /// <param name="powerfulImpact">대시형 적의 강한 충돌이면 true입니다.</param>
        public void PlayEnemyCollision(bool powerfulImpact)
        {
            if (powerfulImpact == true)
            {
                PlayPitchedImpact(damagedClip, 0.68f, 1f);
                return;
            }

            PlayPitchedImpact(damagedClip, 0.92f, 0.55f);
        }

        /// <summary>가시의 날카로운 느낌을 강조한 높은 피격음을 겹쳐 재생합니다.</summary>
        public void PlaySpikeCollision()
        {
            PlayPitchedImpact(damagedClip, 1.32f, 0.72f);
        }

        /// <summary>점프 효과음을 재생합니다.</summary>
        private void PlayJump()
        {
            Play(jumpClip);
        }

        /// <summary>피격 효과음을 재생합니다.</summary>
        /// <param name="sourcePosition">피해가 발생한 월드 위치입니다.</param>
        /// <param name="invulnerabilityDuration">피해 후 무적 시간입니다.</param>
        private void PlayDamaged(Vector2 sourcePosition, float invulnerabilityDuration)
        {
            Play(damagedClip);
        }

        /// <summary>밟기 공격 효과음을 재생하고 재생 횟수를 기록합니다.</summary>
        private void PlayStomp()
        {
            StompSoundPlayCount++;
            Play(stompClip);
        }

        /// <summary>지정한 오디오 클립을 한 번 재생합니다.</summary>
        /// <param name="clip">재생할 오디오 클립입니다.</param>
        private void Play(AudioClip clip)
        {
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>공용 피격음을 별도 오디오 소스에서 지정한 음높이와 음량으로 재생합니다.</summary>
        /// <param name="clip">재생할 공용 피격 오디오 클립입니다.</param>
        /// <param name="pitch">충돌 종류를 표현할 음높이입니다.</param>
        /// <param name="volume">추가 충돌음의 음량입니다.</param>
        private void PlayPitchedImpact(
            AudioClip clip,
            float pitch,
            float volume)
        {
            if (clip == null)
            {
                return;
            }

            GameObject soundObject = new GameObject("Hazard Impact Sound");
            soundObject.transform.position = transform.position;
            AudioSource impactSource = soundObject.AddComponent<AudioSource>();
            impactSource.playOnAwake = false;
            impactSource.loop = false;
            impactSource.spatialBlend = 0f;
            impactSource.clip = clip;
            impactSource.pitch = pitch;
            impactSource.volume = volume;
            impactSource.Play();
            Destroy(soundObject, clip.length / Mathf.Max(0.01f, pitch) + 0.1f);
        }
    }
