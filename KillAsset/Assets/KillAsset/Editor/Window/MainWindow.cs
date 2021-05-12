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
                InitIfNeeded();
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

            _treeView.searchString = _SearchField.OnGUI(GetSearchRect(), _treeView.searchString);
            _treeView.OnGUI(GetTreeViewRect());
        }

        private Rect GetSearchRect()
        {
            return new Rect(10, 20, position.width - 20, 30);
        }
        private Rect GetTreeViewRect()
        {
            return new Rect(10, 40, position.width - 20, position.height - 60);
        }

        void InitIfNeeded()
        {
            if (!m_Initialized)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (_treeviewState == null)
                    _treeviewState = new TreeViewState();

                bool firstInit = m_MultiColumnHeaderState == null;
                var headerState = AssetTreeView.CreateDefaultMultiColumnHeaderState(GetTreeViewRect().width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new MyMultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var root = AssetTreeElement.CreateRoot();
                var treeModel = new TreeModel<AssetTreeElement>(SerializeBuildInfo.Inst.useList, root);

                _treeView = new AssetTreeView(_treeviewState, multiColumnHeader, treeModel);
                _treeView.Reload();
                _SearchField = new SearchField();
                //_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

                m_Initialized = true;
            }
        }

        private bool m_Initialized = false;
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        private AssetTreeView _treeView;
        private TreeViewState _treeviewState;
        private SearchField _SearchField;
    }

    internal class MyMultiColumnHeader : MultiColumnHeader
    {
        Mode m_Mode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public MyMultiColumnHeader(MultiColumnHeaderState state)
            : base(state)
        {
            mode = Mode.DefaultHeader;
        }

        public Mode mode
        {
            get
            {
                return m_Mode;
            }
            set
            {
                m_Mode = value;
                switch (m_Mode)
                {
                    case Mode.LargeHeader:
                        canSort = true;
                        height = 37f;
                        break;
                    case Mode.DefaultHeader:
                        canSort = true;
                        height = DefaultGUI.defaultHeight;
                        break;
                    case Mode.MinimumHeaderWithoutSorting:
                        canSort = false;
                        height = DefaultGUI.minimumHeight;
                        break;
                }
            }
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            // Default column header gui
            base.ColumnHeaderGUI(column, headerRect, columnIndex);

            // Add additional info for large header
            if (mode == Mode.LargeHeader)
            {
                // Show example overlay stuff on some of the columns
                if (columnIndex > 2)
                {
                    headerRect.xMax -= 3f;
                    var oldAlignment = EditorStyles.largeLabel.alignment;
                    EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
                    GUI.Label(headerRect, 36 + columnIndex + "%", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.alignment = oldAlignment;
                }
            }
        }
    }
}

