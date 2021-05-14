using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace KA
{
    public class MainWindow : EditorWindow
    {
        #region static method
        public static float LeftExpendWidth = 120;
        public static float LeftDefaultWidth = 10;
        public static float RightExpendOffset = 110;

        private static MainWindow mainWindow;

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
        #endregion

        internal AssetTreeView TreeView { get { return _treeView; } }

        internal bool ShowPipelineExpend { get { return _showPipelineExpend; } }

        private void OnEnable()
        {
            if (_treeviewState == null)
                _treeviewState = new TreeViewState();

            SerializeBuildInfo.Init();
            CollectPipeLines();
            AssetTreeHelper.onCollectDependencies = OnCollectDependenies;
        }

        private void OnDisable()
        {
            AssetTreeHelper.onCollectDependencies = null;
        }

        private void OnGUI()
        {
            DrawPipelineInfo();

            if (_treeView == null)
                return;

            if (_treeView.treeModel.numberOfDataElements == 0)
                return;

            _treeView.searchString = _searchField.OnGUI(GetSearchRect(), _treeView.searchString);
            _treeView.OnGUI(GetTreeViewRect());

            if (GUI.Button(GetExportBtnRect(), "Export", EditorStyles.miniButton))
            {

            }

            if (_lastSelectPipeline != null)
                _lastSelectPipeline.OnGUI(this);

            DebugSelectionInfo();
        }

        private void DrawPipelineInfo()
        {  
            if (_showPipelineExpend)
            {
                GUI.Box(new Rect(5, 5, 110, position.height - 10), "");

                int pipeIndex = 0;
                var e = _pipelines.GetEnumerator();
                while(e.MoveNext())
                {
                    var current = e.Current;
                    GUI.Label(GetPipelineGroupRect(pipeIndex++), current.Key.ToString());
                    for (int i = 0; i < current.Value.Count; i++)
                    {
                        var p = current.Value[i];
                        bool toggleState = GUI.Toggle(GetPipelineButtonRect(pipeIndex++),
                            _pipelineToggleState[p].toggle,
                            _pipelineToggleState[p].name,
                            "Button");

                        if (_pipelineToggleState[p].toggle != toggleState)
                        {
                            if(toggleState)
                            {
                                var list = p.GetObjectPath();
                                list.ForEach(v =>
                                {
                                    AssetTreeElement element = AssetTreeHelper.CreateAssetElement(v, 0);
                                    SerializeBuildInfo.Inst.AddItem(element);

                                    AssetTreeHelper.CollectAssetDependencies(v, 0);
                                });

                                EditorUtility.ClearProgressBar();
                            }
  
                            if (_lastSelectPipeline != null && _lastSelectPipeline != p)
                                _pipelineToggleState[p].toggle = false;

                            _lastSelectPipeline = p;
                            _pipelineToggleState[p].toggle = toggleState;
                            InitIfNeeded();
                        }
                    }

                    GUI.Label(GetPipelineGroupRect(pipeIndex++), "");
                }
 
            }

            _showPipelineExpend = GUI.Toggle(GetIndentButtonRect(), _showPipelineExpend, _showPipelineExpend ? "<" : ">", "Button");
        }

        private void DebugSelectionInfo()
        {
            if (_treeView == null || !_treeView.HasSelection())
                return;

            var objectList = _treeView.SelectionObjects;
            if(objectList.Count == 1)
            {
                var obj = objectList[0];
                if (obj.Icon != null)
                {
                    GUI.DrawTexture(new Rect(GetLeftSpace(), position.height - 105, 80, 80), obj.Icon);
                    GUI.Label(new Rect(GetLeftSpace() + 80, position.height - 105, position.width / 2, 20), "Info:");
                    GUI.Label(new Rect(GetLeftSpace() + 80, position.height - 85, position.width / 2, 20), objectList[0].Path);
                }
                else
                {
                    GUI.Label(new Rect(GetLeftSpace(), position.height - 105, position.width / 2, 20), "Info:");
                    GUI.Label(new Rect(GetLeftSpace(), position.height - 85, position.width / 2, 20), objectList[0].Path);
                }
            }
            else
            {
                string msg = string.Format("Select {{{0}}} Items", objectList.Count);
                GUI.Label(new Rect(GetLeftSpace(), position.height - 105, position.width / 2, 20), msg);
            }
        }

        internal float GetLeftSpace()
        {
            if (_showPipelineExpend)
                return LeftExpendWidth;

            return LeftDefaultWidth;
        }

        private float GetRightSpace()
        {
            if (_showPipelineExpend)
                return position.width - RightExpendOffset;

            return position.width;
        }

        private Rect GetIndentButtonRect()
        {
            return new Rect(10, 5, 20, 20);
        }

        private Rect GetPipelineGroupRect(int index)
        {
            return new Rect(10, 30 + 22 * index, 100, 20);
        }

        private Rect GetPipelineButtonRect(int index)
        {
            return new Rect(15, 30 + 22 * index, 90, 20);
        }

        private Rect GetSearchRect()
        {
            return new Rect(GetLeftSpace(), 30, GetRightSpace() - 20, 30);
        }
        private Rect GetTreeViewRect()
        {
            if (_treeView != null && _treeView.HasSelection())
                return new Rect(GetLeftSpace(), 50, GetRightSpace() - 20, position.height - 160);

            return new Rect(GetLeftSpace(), 50, GetRightSpace() - 20, position.height - 100);
        }

        private Rect GetExportBtnRect()
        {
            return new Rect(GetLeftSpace(), GetRightSpace() - 25, 80, 20);
        }

        private bool HasPipeline()
        {
            return _pipelineToggleState != null;
        }

        private void InitIfNeeded()
        {
            if (!m_Initialized)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (_treeviewState == null)
                    _treeviewState = new TreeViewState();

                bool firstInit = m_MultiColumnHeaderState == null;
                var headerState = AssetTreeHelper.CreateDefaultMultiColumnHeaderState(GetTreeViewRect().width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new MyMultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var root = AssetTreeElement.CreateRoot();
                var treeModel = new TreeModel<AssetTreeElement>(SerializeBuildInfo.Inst.treeList, root);

                _treeView = new AssetTreeView(_treeviewState, multiColumnHeader, treeModel);
                _treeView.Reload();
                _searchField = new SearchField();
                //_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

                m_Initialized = true;
            }
        }

        private void CollectPipeLines()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var target = typeof(IPipeline);


            var list = assembly.GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.Contains("KA"))
                .Where(t => target.IsAssignableFrom(t) && t != target)
                .ToList();

            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                var attr = t.GetCustomAttribute<PipelineAttrAttribute>();
                if(attr == null)
                {
                    if (!_pipelines.TryGetValue(PipelineGroup.Other, out List<IPipeline> ilist))
                    {
                        ilist = new List<IPipeline>();
                        _pipelines.Add(PipelineGroup.Other, ilist);
                    }
                    var inst = (IPipeline)Activator.CreateInstance(t);
                    _pipelineToggleState[inst] = new PipeLineState() { toggle = false, name = t.GetType().Name };
                    ilist.Add(inst);
                }
                else
                {
                    if (!_pipelines.TryGetValue(attr.Group, out List<IPipeline> ilist))
                    {
                        ilist = new List<IPipeline>();
                        _pipelines.Add(attr.Group, ilist);
                    }

                    var inst = (IPipeline)Activator.CreateInstance(t);
                    _pipelineToggleState[inst] = new PipeLineState()
                    {
                        toggle = false,
                        name = string.IsNullOrEmpty(attr.Name) ? t.GetType().Name : attr.Name
                    };

                    ilist.Add(inst);
                }
            }
        }

        private void OnCollectDependenies(string path)
        {
            EditorUtility.DisplayProgressBar("Analyze...", path, 0);
        }

        internal class PipeLineState
        {
            internal bool toggle;
            internal string name;
        }

        private bool m_Initialized = false;
        private bool _showPipelineExpend = false;
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        private AssetTreeView _treeView;
        private TreeViewState _treeviewState;
        private SearchField _searchField;
        private Dictionary<PipelineGroup, List<IPipeline>> _pipelines = new Dictionary<PipelineGroup, List<IPipeline>>();
        private Dictionary<IPipeline, PipeLineState> _pipelineToggleState = new Dictionary<IPipeline, PipeLineState>();
        IPipeline _lastSelectPipeline;
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

