using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace KA
{
    public class WorkflowOverrideAttribute : Attribute
    {
        public string Name { get; private set; } = "";
        public int SortIndex { get; private set; } = -1;

        public WorkflowOverrideAttribute(string name)
        {
            this.Name = name;
        }

        public WorkflowOverrideAttribute(string name, int sortIndex)
        {
            this.Name = name;
            this.SortIndex = sortIndex;
        }
    }

    public class WorkflowIgnoreAttribute : Attribute { }

    public abstract class AssetWorkflow
    {
        //when select workflow , this function will excute.
        public abstract void Run();
        //when you change another worlflow or main window is disposed.
        public abstract void Clear();

        /// <summary>
        /// gui option class.
        /// </summary>
        public class GUIOptions
        {
            public bool showSearchField = true;

            public OnCustomGUI onTopGUICallback;
            public OnCustomGUI onBottomGUICallback;
            public OnSelectGUI<TreeElement> onSelectionGUICallback;

            public delegate void OnCustomGUI(ref Rect rect);
            public delegate void OnSelectGUI<T>(ref Rect rect, List<T> elements, bool lastChanged) where T : TreeElement;
        }

        /// <summary>
        /// gui options.you can override this property to custom your specific gui.
        /// </summary>
        internal virtual GUIOptions GuiOptions { get; } = new GUIOptions();

        /// <summary>
        /// search filter.
        /// </summary>
        public virtual bool CanSearch(TreeElement t) { return false; }

        /// <summary>
        /// sort function.when you click ui sort btn,this function will be run.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="isAscend">is ascend or descend</param>
        public virtual void Sort(int columnIndex, bool isAscend) { }

        /// <summary>
        /// refresh tree view. 
        /// refresh the tree view list if you need.
        /// </summary>
        /// <param name="assetList">asset list</param>
        public void RefreshTreeView(List<AssetTreeElement> assetList)
        {
            if (TreeView == null)
            {
                return;
            }

            _lastList = assetList;
            TreeView.treeModel.SetData(assetList);
            TreeView.Reload();
        }

        /// <summary>
        /// refresh tree view by path list. 
        /// refresh the tree view list if you need.
        /// </summary>
        /// <param name="pathList">path list</param>
        public void RefreshTreeView(List<string>  pathList)
        {
            List<AssetTreeElement> assetList = new List<AssetTreeElement>();
            AssetTreeHelper.ListToTree(pathList, assetList);
            RefreshTreeView(assetList);
        }

        #region priviate

        List<AssetTreeElement> _lastList;
        #endregion

        #region use for editor window
        //use for recording mainwindow toggle state
        internal bool ToggleState { get; set; }

        //alias name
        internal string Alias { get; set; }

        //sort index in toolbar.
        internal int SortIndex { get; set; } = -1;

        //tree view
        internal AssetTreeView TreeView { get; set; }
        #endregion
    }
}

