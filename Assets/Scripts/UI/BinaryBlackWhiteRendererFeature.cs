using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>카메라 최종 화면을 순수한 검은색과 흰색 두 색으로 변환하는 URP 기능입니다.</summary>
public sealed class BinaryBlackWhiteRendererFeature : ScriptableRendererFeature
{
    // 화면 이진화에 사용할 전용 머티리얼입니다.
    [SerializeField] private Material effectMaterial;
    // 실제 렌더링 명령을 실행하는 패스입니다.
    private BinaryBlackWhiteRenderPass renderPass;

    /// <summary>에디터 설정 코드에서 화면 이진화 머티리얼을 연결합니다.</summary>
    /// <param name="material">흰색과 검은색 변환에 사용할 머티리얼입니다.</param>
    public void Configure(Material material)
    {
        effectMaterial = material;
        Create();
    }

    /// <summary>렌더러 기능이 활성화될 때 전용 렌더 패스를 생성합니다.</summary>
    public override void Create()
    {
        renderPass = new BinaryBlackWhiteRenderPass(effectMaterial);
        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    /// <summary>카메라가 화면을 그릴 때 이진화 렌더 패스를 실행 목록에 추가합니다.</summary>
    /// <param name="renderer">현재 카메라가 사용하는 스크립터블 렌더러입니다.</param>
    /// <param name="renderingData">현재 프레임의 렌더링 정보입니다.</param>
    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (effectMaterial == null)
        {
            return;
        }

        renderer.EnqueuePass(renderPass);
    }

    /// <summary>렌더러 기능이 제거될 때 임시 렌더 텍스처를 해제합니다.</summary>
    /// <param name="disposing">관리되는 리소스도 함께 해제할지 나타냅니다.</param>
    protected override void Dispose(bool disposing)
    {
        renderPass?.Dispose();
    }

    /// <summary>완성된 카메라 화면에 흑백 이진화 머티리얼을 적용하는 렌더 패스입니다.</summary>
    private sealed class BinaryBlackWhiteRenderPass : ScriptableRenderPass
    {
        // 이진화 셰이더를 사용하는 머티리얼입니다.
        private readonly Material material;
        // 같은 화면을 안전하게 다시 쓰기 위한 임시 렌더 텍스처입니다.
        private RTHandle temporaryColorTarget;

        /// <summary>사용할 머티리얼과 필요한 컬러 입력을 설정합니다.</summary>
        /// <param name="targetMaterial">화면 이진화 머티리얼입니다.</param>
        public BinaryBlackWhiteRenderPass(Material targetMaterial)
        {
            material = targetMaterial;
            ConfigureInput(ScriptableRenderPassInput.Color);
        }

        /// <summary>임시 화면을 만든 뒤 이진화 결과를 카메라 화면으로 복사합니다.</summary>
        /// <param name="context">렌더 명령을 실행할 컨텍스트입니다.</param>
        /// <param name="renderingData">현재 프레임의 렌더링 정보입니다.</param>
        public override void Execute(
            ScriptableRenderContext context,
            ref RenderingData renderingData)
        {
            if (material == null)
            {
                return;
            }

            RTHandle cameraColorTarget =
                renderingData.cameraData.renderer.cameraColorTargetHandle;
            RenderTextureDescriptor descriptor =
                renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateHandleIfNeeded(
                ref temporaryColorTarget,
                descriptor,
                FilterMode.Point,
                TextureWrapMode.Clamp,
                name: "BinaryBlackWhiteTemporary");

            CommandBuffer commandBuffer =
                CommandBufferPool.Get("Binary Black White");
            Blitter.BlitCameraTexture(
                commandBuffer,
                cameraColorTarget,
                temporaryColorTarget);
            Blitter.BlitCameraTexture(
                commandBuffer,
                temporaryColorTarget,
                cameraColorTarget,
                material,
                0);
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }

        /// <summary>사용이 끝난 임시 렌더 텍스처를 해제합니다.</summary>
        public void Dispose()
        {
            temporaryColorTarget?.Release();
        }
    }
}
