using UnityEngine;
using UnityEditor;
using System.Linq;

namespace KA
{
    public class MainWindow : EditorWindow
    {
        private static MainWindow mainWindow;

        [MenuItem("Assets/Create/My Scriptable Object")]
        public static void CreateMyAsset()
        {
            EditorConfig asset = ScriptableObject.CreateInstance<EditorConfig>();

            AssetDatabase.CreateAsset(asset, "Assets/Config.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }

        [UnityEditor.MenuItem("Window/Kill Asset _%&k")]
        public static void OpenAssetHunter()
        {
            if (mainWindow == null)
                InitializeWindow();
        }

        static void InitializeWindow()
        {
            mainWindow = GetWindow<MainWindow>();
            mainWindow.titleContent = new GUIContent("Kill Asset");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Build"))
            {
                string path = EditorUtility.SaveFolderPanel("Build Location:", "", "");
                BuildPlayerOptions options = new BuildPlayerOptions();
                options.locationPathName = path + "/KABuild.exe";
                options.scenes = EditorBuildSettings.scenes.Where(v => v.enabled).Select(v => v.path).ToArray<string>();
                options.target = EditorUserBuildSettings.activeBuildTarget;
                options.targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                options.options = BuildOptions.None;

                BuildPipeline.BuildPlayer(options);
            }
        }
    }
}

