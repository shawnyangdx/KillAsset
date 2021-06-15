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
            CollectWorkflows();
            AssetTreeHelper.onCollectDependencies = OnCollectDependenies;
        }

        private void OnDisable()
        {
            AssetTreeHelper.onCollectDependencies = null;
        }

        private void OnDestroy()
        {
            if (_lastWorkflow != null)
                _lastWorkflow.Clear();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_lastWorkflow == null)
            {
                ShowDescription();
                return;
            }

            var baseRect = GetBaseRect();
            _lastWorkflow.GuiOptions.onTopGUICallback?.Invoke(ref baseRect);

            if (_lastWorkflow.GuiOptions.showSearchField)
            {
                var searchRect = baseRect;
                baseRect.y += 1;
                baseRect.height = 30;
                baseRect.width = position.width - HP.RightBoardOffset - (baseRect.x - HP.WorkflowBoxWidth);
                _treeView.searchString = _searchField.OnGUI(baseRect, _treeView.searchString);
                baseRect.y = searchRect.y;
                baseRect.height = searchRect.height;
                baseRect.width = searchRect.width;
                baseRect.y += 24;
                baseRect.height -= 24;
            }

            _lastWorkflow.GuiOptions.onBottomGUICallback?.Invoke(ref baseRect);
            _treeView.OnGUI(baseRect);

            DebugSelectionInfo(ref baseRect);
        }

        private void ShowDescription()
        {
            var style = GUIStyleMgr.Instance.BuildinLabelStyle;
            var defaultFont = style.font;
            int fontSize = style.fontSize;
            var normalColor = style.normal.textColor;
            style.fontSize = 50;
            style.normal.textColor = Helper.Color.titleColor;
            style.font = GUIStyleMgr.Instance.TitleFont;
            Rect rect = new Rect((position.width - HP.ToolBarWidth + 10) / 2, position.height / 3 + 50, 500, 500);
            GUI.Label(rect, "Kill Asset", style);
            style.font = GUIStyleMgr.Instance.IntroFont;
            style.fontSize = fontSize;

            rect.x -= (HP.ToolBarWidth);
            rect.y += 50;
            rect.height = 20;
            rect.width = 600;

            GUI.Label(rect, "Please select one Asset Workflow in the left toolbar", style);
            style.normal.textColor = normalColor;
            rect = new Rect(position.width - 85, position.height - 20, 100, 20);
            GUI.Label(rect, string.Format("Version: {0}", Helper.Version));
        }

        private void DrawToolbar()
        {
            float baseHeight = position.height - 30;
            var boxRect = new Rect(5, 5, HP.ToolBarWidth, baseHeight);
            GUI.Box(boxRect, "");
            var scrollRect = boxRect;
            scrollRect.width += 15;
            var viewRect = boxRect;
            viewRect.height = 22 * m_toolbarCount;
            m_isToolbarShowScrollBar = viewRect.height > scrollRect.height;
            m_workflowToolBarPos = GUI.BeginScrollView(scrollRect, m_workflowToolBarPos, viewRect);
            m_toolbarCount = 0;
            for (int i = 0; i < _workflowes.Count; i++)
            {
                var p = _workflowes[i];
                bool toggleState = GUI.Toggle(GetWorkflowButtonRect(m_toolbarCount++),
                    p.ToggleState, p.Alias, "Button");

                if (p.ToggleState != toggleState)
                {
                    if (toggleState)
                    {
                        InitIfNeeded();
                        p.Run();
                        if (_lastWorkflow != null && _lastWorkflow != p)
                        {
                            _lastWorkflow.Clear();
                            _lastWorkflow.ToggleState = false;
                        }

                        _lastWorkflow = p;
                    }
                    else
                    {
                        if (_lastWorkflow != null)
                        {
                            _lastWorkflow.Clear();
                            _lastWorkflow = null;
                        }
                    }
                    p.ToggleState = toggleState;
                }
            }
            GUI.EndScrollView();

            boxRect.y = baseHeight + 7;
            DrawToolbarBottomBtn(boxRect);
        }

        private void DrawToolbarBottomBtn(Rect rect)
        {
            rect.height = 20;
            rect.width = 20;
            if(GUI.Button(rect, GUIStyleMgr.Instance.SettingContent))
            {
                Selection.activeObject = EditorConfig.Inst;
            }
        }

        private void DebugSelectionInfo(ref Rect baseRect)
        {
            if (_treeView == null || !_treeView.HasSelection())
                return;

            if (_lastWorkflow == null)
                return;

            if(_treeView.LastSelectChanged)
                _selectObjects = new List<TreeElement>(_treeView.SelectionObjects);

            _lastWorkflow.GuiOptions.onSelectionGUICallback(ref baseRect, _selectObjects, _treeView.LastSelectChanged);
            _treeView.LastSelectChanged = false;
        }

        private Rect GetBaseRect()
        {
            var width = m_isToolbarShowScrollBar ? HP.WorkflowBoxWidth + 20 : HP.WorkflowBoxWidth;
            return new Rect(
                width, 5, 
                position.width - HP.RightBoardOffset, 
                position.height - HP.BottomBoardOffset);
        }

        private Rect GetWorkflowButtonRect(int index)
        {
            return new Rect(8, 8 + 22 * index, 102, 20);
        }

        private void InitIfNeeded()
        {
            if (!m_Initialized)
            {
                if (_treeviewState == null)
                    _treeviewState = new TreeViewState();

                bool firstInit = m_MultiColumnHeaderState == null;
                var headerState = AssetTreeHelper.CreateDefaultMultiColumnHeaderState();
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new CustomMultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                multiColumnHeader.sortingChanged += OnSortingChanged;

                var root = AssetTreeElement.CreateRoot();
                var treeModel = new TreeModel<AssetTreeElement>(new List<AssetTreeElement>() { root });
                _treeView = new AssetTreeView(_treeviewState, multiColumnHeader, treeModel);
                _treeView.Reload();
                _treeView.treeModel.modelChanged += () => Repaint();
                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;

                m_Initialized = true;
            }
        }

        private void CollectWorkflows()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var target = typeof(AssetWorkflow);
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
                    var inst = (AssetWorkflow)Activator.CreateInstance(t);
                    inst.Alias = overrideAttr.Name;
                    _workflowes.Add(inst);
                }
                else
                {
                    var inst = (AssetWorkflow)Activator.CreateInstance(t);
                    inst.Alias = t.Name.Replace("Workflow", "");
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

        private void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            if (_lastWorkflow != null)
            {
                bool isAscend = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);
                _lastWorkflow.Sort(multiColumnHeader.sortedColumnIndex, isAscend);
            }
        }

        private bool m_Initialized = false;
        private Vector2 m_workflowToolBarPos;
        private int m_toolbarCount = 0;
        private bool m_isToolbarShowScrollBar = false;
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        private AssetTreeView _treeView;
        private TreeViewState _treeviewState;
        private SearchField _searchField;
        private List<AssetWorkflow> _workflowes = new List<AssetWorkflow>();
        AssetWorkflow _lastWorkflow;
        private List<TreeElement> _selectObjects;
    }


}

