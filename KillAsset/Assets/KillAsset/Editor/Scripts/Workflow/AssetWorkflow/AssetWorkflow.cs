using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KA
{
    [WorkflowIgnore]
    public class AssetWorkflow : Workflow
    {
        public override void Run() { }
        public override void OnGUI(MainWindow window) { }

    }

    [WorkflowOverride("无用资源清理")]
    public class UnuselessWorkflow : AssetWorkflow
    {
        public override void Run()
        {
            AssetSerializeInfo.Inst.AllAssetPaths.ForEach(v =>
            {
                AssetTreeElement element = AssetTreeHelper.CreateAssetElement(v, 0);
                AssetSerializeInfo.Inst.AddItem(element);

                AssetTreeHelper.CollectAssetDependencies(v, 0);
            });

            EditorUtility.ClearProgressBar();
        }

        public override void OnGUI(MainWindow window)
        {
            _window = window;
            int selected = GUI.Toolbar(GetToolBarRect(window), _toolbarSelected, Enum.GetNames(typeof(AssetShowMode)));
            if (_toolbarSelected != selected)
            {
                _toolbarSelected = selected;
                var assetList = GetAssetList();
                RefreshTreeView(assetList);
            }

            //DrawDeleteBtnInfo(window);
        }

        public override bool CanSearch(TreeElement t)
        {
            if (_toolbarSelected == (int)AssetShowMode.Summary)
            {
                if (!t.parent.IsRoot)
                    return false;
            }

            return true;
        }

        public override void Sort(int columnIndex, bool isAscend)
        {
            List<AssetTreeElement> assetList = _window.TreeView.treeModel.Data as List<AssetTreeElement>;
            AssetTreeElement root = assetList[0];

            assetList = assetList.Where(v => v.depth == 0).ToList();
            if (columnIndex == (int)ColumnType.Icon1)
            {
                if (isAscend)
                    assetList = assetList.OrderBy(v => v.AssetType.ToString()).ToList();
                else
                    assetList = assetList.OrderByDescending(v => v.AssetType.ToString()).ToList();
            }
            else if (columnIndex == (int)ColumnType.Name)
            {
                if (isAscend)
                    assetList = assetList.OrderBy(v => v.name).ToList();
                else
                    assetList = assetList.OrderByDescending(v => v.name).ToList();
            }
            else if (columnIndex == (int)ColumnType.Size)
            {
                if (isAscend)
                {
                    assetList.Sort((l, r) =>
                        {
                            var dic = AssetSerializeInfo.Inst.guidToAsset;
                            dic.TryGetValue(l.Guid, out AssetTreeElement le);
                            dic.TryGetValue(r.Guid, out AssetTreeElement re);
                            long leftSize = le != null ? le.Size : 0;
                            long rightSize = re != null ? re.Size : 0;
                            return leftSize.CompareTo(rightSize);
                        }
                    );
                }
                else
                {
                    assetList.Sort((l, r) =>
                        {
                            var dic = AssetSerializeInfo.Inst.guidToAsset;
                            dic.TryGetValue(l.Guid, out AssetTreeElement le);
                            dic.TryGetValue(r.Guid, out AssetTreeElement re);
                            long leftSize = le != null ? le.Size : 0;
                            long rightSize = re != null ? re.Size : 0;
                            return -leftSize.CompareTo(rightSize);
                        }
                   );
                }
            }
            else if (columnIndex == (int)ColumnType.Ref)
            {
                if (isAscend)
                {
                    assetList.Sort((l, r) =>
                    {
                        var dic = AssetSerializeInfo.Inst.guidToRef;
                        dic.TryGetValue(l.Guid, out int lr);
                        dic.TryGetValue(r.Guid, out int rr);
                        return lr.CompareTo(rr);
                    }
                    );
                }
                else
                {
                    assetList.Sort((l, r) =>
                    {
                        var dic = AssetSerializeInfo.Inst.guidToRef;
                        dic.TryGetValue(l.Guid, out int lr);
                        dic.TryGetValue(r.Guid, out int rr);
                        return -lr.CompareTo(rr);
                    }
                   );
                }
            }

            List<AssetTreeElement> newList = new List<AssetTreeElement>();
            newList.Add(root);
            for (int i = 0; i < assetList.Count; i++)
                RebuildList(assetList[i], newList);

            assetList = newList;
            RefreshTreeView(assetList);
        }

        void RebuildList(AssetTreeElement element, List<AssetTreeElement> newList)
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

        private void RefreshTreeView(List<AssetTreeElement> assetList)
        {
            _window.TreeView.treeModel.SetData(assetList);
            _window.TreeView.Reload();
        }

        private Rect GetToolBarRect(MainWindow window)
        {
            return new Rect(Helper.WindowParam.WorkflowBoxWidth + 30, 5, 300, 20);
        }

        private Rect GetDeleteBtnRect(Rect position)
        {
            return new Rect(position.width - Helper.WindowParam.RightExpendOffset, position.height - 105, 100, 30);
        }

        private void GetUselessAssets()
        {
            var allAssets = AssetSerializeInfo.Inst.AllAssetPaths;
        }

        private List<AssetTreeElement> GetAssetList()
        {
            List<AssetTreeElement> elements = new List<AssetTreeElement>();
            if (_toolbarSelected == (int)AssetShowMode.Summary)
            {
                elements = AssetSerializeInfo.Inst.treeList;
            }
            else if (_toolbarSelected == (int)AssetShowMode.All)
            {
                AssetTreeHelper.ListToTree(AssetSerializeInfo.Inst.AllAssetPaths, elements);
            }
            else if (_toolbarSelected == (int)AssetShowMode.Used)
            {
                var useList = AssetSerializeInfo.Inst.treeList
                    .Where(v => !string.IsNullOrEmpty(v.Guid))
                    .Where(v => v.parent != null && !v.parent.IsRoot)
                    .Select(v => v.Path).Distinct().ToList();

                AssetTreeHelper.ListToTree(useList, elements);
            }
            else
            {
                List<AssetTreeElement> newList = AssetSerializeInfo.Inst.treeList;

                var usedGuidList = newList
                .Where(v => !string.IsNullOrEmpty(v.Guid))
                .Where(v => v.parent != null && !v.parent.IsRoot)
                .Select(v => v.Guid)
                .Distinct()
                .ToList();

                List<string> unuseList = new List<string>();
                foreach (var item in AssetSerializeInfo.Inst.guidToAsset)
                {
                    if (string.IsNullOrEmpty(item.Value.Path))
                        continue;

                    if (usedGuidList.Contains(item.Key))
                        continue;

                    unuseList.Add(item.Value.Path);
                }

                AssetTreeHelper.ListToTree(unuseList, elements);
            }

            return elements;
        }

        private int _toolbarSelected = 0;
        private MainWindow _window;
    }
}


