using UnityEngine;

/// <summary>추적형 적이 질주하는 동안 발밑에서 이동 반대 방향으로 먼지 파티클을 생성합니다.</summary>
[RequireComponent(typeof(ChasingEnemy))]
public sealed class EnemyDashDust : MonoBehaviour
{
    // URP에서 먼지 파티클을 표시할 머티리얼입니다.
    [SerializeField] private Material dustMaterial;
    // 현재 질주 여부와 방향을 제공하는 추적형 적 컴포넌트입니다.
    private ChasingEnemy chasingEnemy;
    // 발밑 먼지를 생성하고 움직이는 파티클 시스템입니다.
    private ParticleSystem dustParticles;
    // 먼지 파티클의 화면 표시를 담당하는 렌더러입니다.
    private ParticleSystemRenderer dustRenderer;

    /// <summary>추적형 적 컴포넌트를 가져오고 발밑 먼지 파티클 시스템을 생성합니다.</summary>
    private void Awake()
    {
        chasingEnemy = GetComponent<ChasingEnemy>();
        CreateDustParticles();
    }

    /// <summary>매 프레임 실제 질주 여부에 따라 먼지 방출과 이동 방향을 갱신합니다.</summary>
    private void Update()
    {
        if (chasingEnemy == null || dustParticles == null)
        {
            return;
        }

        ParticleSystem.EmissionModule emission =
            dustParticles.emission;
        emission.enabled = chasingEnemy.IsDashing;
        ParticleSystem.VelocityOverLifetimeModule velocity =
            dustParticles.velocityOverLifetime;
        float horizontalVelocity =
            -chasingEnemy.FacingDirection * 1.1f;
        velocity.x = new ParticleSystem.MinMaxCurve(
            horizontalVelocity,
            horizontalVelocity);
    }

    /// <summary>에디터 생성 코드에서 먼지 파티클이 사용할 머티리얼을 설정합니다.</summary>
    /// <param name="material">URP 파티클 머티리얼입니다.</param>
    public void Configure(Material material)
    {
        dustMaterial = material;
    }

    /// <summary>질주 중에만 재생되는 월드 공간 먼지 파티클 시스템을 발밑에 구성합니다.</summary>
    private void CreateDustParticles()
    {
        GameObject particleObject = new GameObject("Dash Dust");
        particleObject.transform.SetParent(transform, false);
        particleObject.transform.localPosition =
            new Vector3(0f, -0.58f, 0f);
        dustParticles = particleObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = dustParticles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.42f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.09f, 0.18f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.72f, 0.65f, 0.5f, 0.82f),
            new Color(0.9f, 0.84f, 0.7f, 0.65f));
        main.gravityModifier = -0.08f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 36;

        ParticleSystem.EmissionModule emission = dustParticles.emission;
        emission.rateOverTime = 24f;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = dustParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.13f;

        ParticleSystem.VelocityOverLifetimeModule velocity =
            dustParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.25f, 0.75f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
            dustParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fadeGradient = new Gradient();
        fadeGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.85f, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = fadeGradient;

        dustRenderer =
            particleObject.GetComponent<ParticleSystemRenderer>();
        dustRenderer.material = dustMaterial;
        dustRenderer.sortingOrder = 5;
    }
}
