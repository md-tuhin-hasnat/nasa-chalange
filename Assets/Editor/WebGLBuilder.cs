using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AresResurgence.Editor
{
    public static class WebGLBuilder
    {
        [MenuItem("Ares Resurgence/Build WebGL")]
        public static void Build()
        {
            string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };
            string buildPath = "Build/WebGL";

            // Ensure destination directory exists
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            // Disable compression so that any standard static HTTP server can host it without custom brotli/gzip response headers
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.companyName = "NASA Challenge";
            PlayerSettings.productName = "Ares Resurgence - Mission 0";

            Debug.Log("[WebGLBuilder] Starting WebGL Build to " + buildPath + "...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[WebGLBuilder] WebGL Build SUCCEEDED: {summary.totalSize} bytes in {summary.totalTime.TotalSeconds:F1}s");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[WebGLBuilder] WebGL Build FAILED: {summary.totalErrors} errors.");
                EditorApplication.Exit(1);
            }
        }
    }
}
