using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;

/// <summary>외부 C# 편집기가 정의 이동과 자동 완성을 사용할 수 있도록 Unity 프로젝트 파일을 생성합니다.</summary>
[InitializeOnLoad]
public static class IdeProjectFileGenerator
{
    // 현재 Unity 실행에서 자동 생성을 이미 요청했는지 확인할 세션 키입니다.
    private const string GeneratedKey = "AcademyPlatformer.IdeProjectFilesGenerated";

    /// <summary>에디터가 스크립트를 불러온 뒤 한 번만 프로젝트 파일 생성을 예약합니다.</summary>
    static IdeProjectFileGenerator()
    {
        bool alreadyGenerated =
            SessionState.GetBool(GeneratedKey, false); // 현재 Unity 실행에서 생성 요청을 마쳤는지 여부입니다.
        if (alreadyGenerated == true)
        {
            return;
        }
        SessionState.SetBool(GeneratedKey, true);
        EditorApplication.delayCall += GenerateProjectFiles;
    }

    /// <summary>메뉴에서 C# 솔루션과 프로젝트 파일을 다시 생성합니다.</summary>
    [MenuItem("Tools/Academy Platformer/Regenerate C# Project Files")]
    public static void GenerateProjectFiles()
    {
        IExternalCodeEditor codeEditor =
            CodeEditor.CurrentEditor; // Unity 환경 설정에서 현재 선택된 외부 코드 편집기입니다.
        if (codeEditor == null)
        {
            Debug.LogWarning("No external C# editor is selected in Unity preferences.");
            return;
        }
        codeEditor.SyncAll();
        string projectDirectory =
            Directory.GetParent(Application.dataPath).FullName; // Unity 프로젝트의 루트 디렉터리입니다.
        string[] solutionFiles =
            Directory.GetFiles(projectDirectory, "*.sln*"); // 생성 결과를 확인할 솔루션 파일 목록입니다.
        if (solutionFiles.Length == 0)
        {
            Debug.LogWarning("C# solution generation was requested, but no solution file was found.");
            return;
        }
        Debug.Log("C# project files generated successfully.");
    }
}
