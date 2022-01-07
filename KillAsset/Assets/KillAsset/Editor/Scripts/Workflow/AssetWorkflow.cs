using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

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
            public bool showExport = true;

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
        /// export btn logic.to export target file from Config.
        /// </summary>
        internal virtual void Export()
        {
            AssetSerializeInfo.Inst.Export(TreeView.treeModel.Data as List<AssetTreeElement>, GetType().Name);
        }

        /// <summary>
        /// search filter.
        /// </summary>
        public virtual bool CanSearch(TreeElement t) { return false; }

        /// <summary>
        /// sort base function.  when you click ui sort btn,this function will be run.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="isAscend">is ascend or descend</param>
        public virtual void Sort(int columnIndex, bool isAscend) 
        {
            List<AssetTreeElement> assetList = TreeView.treeModel.Data as List<AssetTreeElement>;
            AssetTreeElement root = assetList[0];

            assetList = assetList.Where(v => v.depth == 0).ToList();
            switch (columnIndex)
            {
                case (int)ColumnType.Icon:
                    {
                        assetList = isAscend ? assetList.OrderBy(v => v.AssetType.ToString()).ToList() :
                                               assetList.OrderByDescending(v => v.AssetType.ToString()).ToList();
                    }
                    break;
                case (int)ColumnType.Name:
                    {
                        assetList = isAscend ? assetList.OrderBy(v => v.name).ToList() :
                                               assetList.OrderByDescending(v => v.name).ToList();
                    }
                    break;
                case (int)ColumnType.Path:
                    {
                        assetList = isAscend ? assetList.OrderBy(v => v.RelativePath).ToList() :
                                               assetList.OrderByDescending(v => v.RelativePath).ToList();
                    }
                    break;
                case (int)ColumnType.Size:
                    {
                        Comparison<AssetTreeElement> sortFunc = (l, r) =>
                        {
                            var dic = AssetSerializeInfo.Inst.guidToAsset;
                            dic.TryGetValue(l.Guid, out AssetTreeElement le);
                            dic.TryGetValue(r.Guid, out AssetTreeElement re);
                            long leftSize = le != null ? le.Size : 0;
                            long rightSize = re != null ? re.Size : 0;
                            return isAscend ? leftSize.CompareTo(rightSize) : -leftSize.CompareTo(rightSize);
                        };

                        assetList.Sort(sortFunc);
                    }
                    break;
                case (int)ColumnType.Ref:
                    {
                        Comparison<AssetTreeElement> sortFunc = (l, r) =>
                        {
                            var dic = AssetSerializeInfo.Inst.guidToRef;
                            int lVal = dic.TryGetValue(l.Guid, out List<string> lr) ? lr.Count : 0;
                            int rVal = dic.TryGetValue(r.Guid, out List<string> rr) ? rr.Count : 0;
                            return isAscend ? lVal.CompareTo(rVal) : -lVal.CompareTo(rVal);
                        };

                        assetList.Sort(sortFunc);
                    }
                    break;
                default:
                    break;
            }

            List<AssetTreeElement> newList = new List<AssetTreeElement>();
            newList.Add(root);
            for (int i = 0; i < assetList.Count; i++)
                RebuildList(assetList[i], newList);

            assetList = newList;
            RefreshTreeView(assetList);
        }

        /// <summary>
        /// rebuild list.if you has all parent element, you can use this to build all list.
        /// </summary>
        public void RebuildList(AssetTreeElement element, List<AssetTreeElement> newList)
        {
            newList.Add(element);
            if (!element.hasChildren)
                return;

            for (int i = 0; i < element.children.Count; i++)
            {
                AssetTreeElement assetElement = element.children[i] as AssetTreeElement;
                RebuildList(assetElement, newList);
            }
        }

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

        protected void ShowProgressBar(string title, string message, float progress)
        {
            EditorUtility.DisplayProgressBar(title, string.Format("{0}:{1}", this.GetType().Name, message), progress);
        }

        protected void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
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

