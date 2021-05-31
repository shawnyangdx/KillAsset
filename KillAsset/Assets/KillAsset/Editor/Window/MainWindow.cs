using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using System.Reflection;
using System;
using System.Collections.Generic;
using HP = KA.Helper.WindowParam;

namespace KA
{
    public class MainWindow : EditorWindow
    {
        #region static method
        private static MainWindow mainWindow;

        public static MainWindow Inst { get { return mainWindow; } }

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

        private void OnEnable()
        {
            if (_treeviewState == null)
                _treeviewState = new TreeViewState();

            AssetSerializeInfo.Init();
            CollectWorkflows();
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

            if (_lastSelectWorkflow != null)
                _lastSelectWorkflow.OnGUI(this);

            DebugSelectionInfo();
        }

        private void DrawPipelineInfo()
        {
            GUI.Box(new Rect(5, 5, 110, position.height - 10), "");
            int pipeIndex = 0;
            for (int i = 0; i < _workflowes.Count; i++)
            {
                var p = _workflowes[i];
                bool toggleState = GUI.Toggle(GetWorkflowButtonRect(pipeIndex++),
                    _workflowUIData[p].toggle,
                    _workflowUIData[p].name,
                    "Button");

                if (_workflowUIData[p].toggle != toggleState)
                {
                    if (toggleState)
                        p.Run();

                    if (_lastSelectWorkflow != null && _lastSelectWorkflow != p)
                        _workflowUIData[p].toggle = false;

                    _lastSelectWorkflow = p;
                    _workflowUIData[p].toggle = toggleState;
                    InitIfNeeded();
                }
            }
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
                    GUI.DrawTexture(new Rect(HP.WorkflowBoxWidth, position.height - 105, 80, 80), obj.Icon);
                    GUI.Label(new Rect(HP.WorkflowBoxWidth + 80, position.height - 105, position.width / 2, 20), "Info:");
                    GUI.Label(new Rect(HP.WorkflowBoxWidth + 80, position.height - 85, position.width / 2, 20), objectList[0].Path);
                }
                else
                {
                    GUI.Label(new Rect(HP.WorkflowBoxWidth, position.height - 105, position.width / 2, 20), "Info:");
                    GUI.Label(new Rect(HP.WorkflowBoxWidth, position.height - 85, position.width / 2, 20), objectList[0].Path);
                }
            }
            else
            {
                string msg = string.Format("Select {{{0}}} Items", objectList.Count);
                GUI.Label(new Rect(HP.WorkflowBoxWidth, position.height - 105, position.width / 2, 20), msg);
            }
        }

        private Rect GetIndentButtonRect()
        {
            return new Rect(10, 5, 20, 20);
        }

        private Rect GetPipelineGroupRect(int index)
        {
            return new Rect(10, 30 + 22 * index, 100, 20);
        }

        private Rect GetWorkflowButtonRect(int index)
        {
            return new Rect(8, 8 + 22 * index, 102, 20);
        }

        private Rect GetSearchRect()
        {
            return new Rect(HP.WorkflowBoxWidth, 30, position.width - HP.RightBoardOffset, 30);
        }
 
        private Rect GetTreeViewRect()
        {
            if (_treeView != null && _treeView.HasSelection())
                return new Rect(HP.WorkflowBoxWidth, 50, position.width - HP.RightBoardOffset, position.height - 160);

            return new Rect(HP.WorkflowBoxWidth, 50, position.width - HP.RightBoardOffset, position.height - 100);
        }

        private Rect GetExportBtnRect()
        {
            return new Rect(HP.WorkflowBoxWidth, position.width - 25, 80, 20);
        }

        private bool HasWorkflow()
        {
            return _workflowUIData != null;
        }

        private void InitIfNeeded()
        {
            if (!m_Initialized)
            {
                if (_treeviewState == null)
                    _treeviewState = new TreeViewState();

                bool firstInit = m_MultiColumnHeaderState == null;
                var headerState = AssetTreeHelper.CreateDefaultMultiColumnHeaderState(GetTreeViewRect().width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new CustomMultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                multiColumnHeader.sortingChanged += OnSortingChanged;
                var root = AssetTreeElement.CreateRoot();
                var treeModel = new TreeModel<AssetTreeElement>(AssetSerializeInfo.Inst.treeList, root);

                _treeView = new AssetTreeView(_treeviewState, multiColumnHeader, treeModel);
                _treeView.Reload();
                _treeView.onCanSearchDelegate = CanSearchDelegate;
                _treeView.treeModel.modelChanged += () => Repaint();
                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;

                m_Initialized = true;
            }
        }

        private void CollectWorkflows()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var target = typeof(Workflow);
            var list = assembly.GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.Contains("KA"))
                .Where(t => target.IsAssignableFrom(t) && t != target)
                .ToList();

            for (int i = 0; i < list.Count; i++)
            {
                Type t = list[i];
                var ignoreAttr = t.GetCustomAttribute<WorkflowIgnoreAttribute>(false);
                if(ignoreAttr != null)
                {
                    continue;
                }

                var overrideAttr = t.GetCustomAttribute<WorkflowOverrideAttribute>(false);
                if (overrideAttr != null)
                {
                    var inst = (Workflow)Activator.CreateInstance(t);
                    _workflowUIData[inst] = new WorkflowState()
                    {
                        toggle = false,
                        name = overrideAttr.Name
                    };
                    _workflowes.Add(inst);
                }
                else
                {
                    var inst = (Workflow)Activator.CreateInstance(t);
                    _workflowUIData[inst] = new WorkflowState()
                    {
                        toggle = false,
                        name = t.Name.Replace("Workflow", "")
                    };
                    _workflowes.Add(inst);
                }
            }
        }

        private void OnCollectDependenies(string path, int depth)
        {
            if(depth == 0)
            {
                EditorUtility.DisplayProgressBar("Analyze...", path, 0);
            }
        }

        private bool CanSearchDelegate(TreeElement element)
        {
            if(_lastSelectWorkflow != null)
            {
                return _lastSelectWorkflow.CanSearch(element);
            }
            return true;
        }

        private void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            if (_lastSelectWorkflow != null)
            {
                bool isAscend = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);
                _lastSelectWorkflow.Sort(multiColumnHeader.sortedColumnIndex, isAscend);
            }
        }

        internal class WorkflowState
        {
            internal bool toggle;
            internal string name;
        }

        private bool m_Initialized = false;
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        private AssetTreeView _treeView;
        private TreeViewState _treeviewState;
        private SearchField _searchField;
        private List<Workflow> _workflowes = new List<Workflow>();
        private Dictionary<Workflow, WorkflowState> _workflowUIData = new Dictionary<Workflow, WorkflowState>();
        Workflow _lastSelectWorkflow;
    }

    internal class CustomMultiColumnHeader : MultiColumnHeader
    {
        Mode m_Mode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public CustomMultiColumnHeader(MultiColumnHeaderState state)
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

