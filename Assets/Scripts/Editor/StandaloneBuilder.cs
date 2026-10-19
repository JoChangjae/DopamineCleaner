using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DopamineCleaner.Editor
{
    public static class StandaloneBuilder
    {
        private static readonly string[] Scenes = {
            "Assets/Scenes/TownScene.unity",
            "Assets/Scenes/MainGame.unity"
        };

        [MenuItem("Dopamine Cleaner/📦 Build Windows 64-bit Release (.exe)")]
        public static void BuildWindowsPlayer()
        {
            Debug.Log("==================================================================");
            Debug.Log("🚀 [Windows 11 배포용 빌드 시작] StandaloneWindows64 빌드 파이프라인 가동...");
            Debug.Log("==================================================================");

            // 1. 빌드 전 최신 씬 재생성 보장
            TownSceneBuilder.BuildTownScene();
            DopamineSceneBuilder.BuildScene();

            string buildDir = "Builds/DopamineCleaner_Win64";
            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }

            string exePath = Path.Combine(buildDir, "DopamineCleaner.exe");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"🎉 [빌드 대성공!] 총 용량: {summary.totalSize / (1024 * 1024):F1} MB, 소요시간: {summary.totalTime.TotalSeconds:F1}초");
                Debug.Log($"📂 실행 파일 위치: {Path.GetFullPath(exePath)}");
                Debug.Log("💡 이 폴더(Builds/DopamineCleaner_Win64)를 압축(ZIP)하여 친구에게 전달하면 바로 플레이할 수 있습니다!");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"❌ [빌드 실패] 오류 {summary.totalErrors}개 발생!");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            Debug.LogError($"  - {msg.content}");
                        }
                    }
                }
            }
            Debug.Log("==================================================================");
        }
    }
}
