using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace KA
{
    public class MainWindow : EditorWindow
    {
        private static MainWindow mainWindow;

        [MenuItem("Assets/Create/My Scriptable Object")]
        public static void CreateMyAsset()
        {
            BuildInfoConfig asset = ScriptableObject.CreateInstance<BuildInfoConfig>();

            AssetDatabase.CreateAsset(asset, string.Format("Assets/{0}.asset", asset.GetType().Name));
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

        private void OnEnable()
        {
            if (_treeviewState == null)
                _treeviewState = new TreeViewState();

            SerializeBuildInfo.Init();
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load"))
            {
                string selectedFile = EditorUtility.OpenFilePanel("", Application.dataPath + "/../", EditorConfig.Instance.dataFileExtension);
                if (string.IsNullOrEmpty(selectedFile))
                {
                    return;
                }

                SerializeBuildInfo.Inst.Serialize(selectedFile);
                if (_treeView == null)
                {
                    var root = AssetTreeElement.CreateRoot();
                    var treeModel = new TreeModel<AssetTreeElement>(SerializeBuildInfo.Inst.sceneAssets, root);
                    _treeView = new AssetTreeView(_treeviewState, treeModel);
                }

                _treeView.Reload();
            }

            if (GUILayout.Button("Build"))
            {
                string path = EditorUtility.SaveFolderPanel("Build Location:", "", "");
                if (string.IsNullOrEmpty(path))
                    return;

                BuildPlayerOptions options = new BuildPlayerOptions();
                options.locationPathName = path + "/KABuild.exe";
                options.scenes = EditorBuildSettings.scenes.Where(v => v.enabled).Select(v => v.path).ToArray<string>();
                options.target = EditorUserBuildSettings.activeBuildTarget;
                options.targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                options.options = BuildOptions.None;

                BuildPipeline.BuildPlayer(options);
            }

            GUILayout.EndHorizontal();

            if (!SerializeBuildInfo.Inst.HasSerialized)
                return;

            if (_treeView == null)
                return;

            _treeView.OnGUI(GetTreeViewRect());
        }


        private Rect GetTreeViewRect()
        {
            return new Rect(10, 30, position.width - 20, position.height - 10);
        }

        private AssetTreeView _treeView;
        private TreeViewState _treeviewState;
    }
}

