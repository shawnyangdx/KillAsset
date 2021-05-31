using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace KA
{
    public enum ColumnType
    {
        Icon1,
        Name,
        Path,
        Size,
        Ref,
    }

    enum AssetShowMode
    {
        Summary,
        Unuse,
        Used,
        All
    }

    internal class AssetTreeView : TreeViewWithTreeModel<AssetTreeElement>
    {
        static float _spaceLabel = 2f;

        #region construct
        public AssetTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<AssetTreeElement> model)
    : base(state, multicolumnHeader, model)
        {
            rowHeight = 20;
            columnIndexForTreeFoldouts = 1;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (20 - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = _spaceLabel;
        }

        #endregion

        #region override method
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 0)
                return;

            List<AssetTreeElement> elements = selectedIds.Select(v => treeModel.Find(v)).ToList();
            if (elements.Count == 0)
                return;

            _selectionObjects = elements;
            if (elements.Count == 1)
            {
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(elements[0].Path);
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<AssetTreeElement>)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (ColumnType)args.GetColumn(i), ref args);
            }
        }
        #endregion

        #region public method

        public List<AssetTreeElement> SelectionObjects { get { return _selectionObjects; } }

        #endregion

        void CellGUI(Rect cellRect, TreeViewItem<AssetTreeElement> item, ColumnType column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            Event current = Event.current;
            if (cellRect.Contains(current.mousePosition) && current.type == EventType.ContextClick)
            {
                BuildGenerticMemu(item);
                current.Use();
            }

            switch (column)
            {
                case ColumnType.Icon1:
                    {
                        GUI.DrawTexture(cellRect, item.data.Icon, ScaleMode.ScaleToFit);
                    }
                    break;
                case ColumnType.Name:
                    {
                        args.rowRect = cellRect;
                        base.RowGUI(args);
                    }
                    break;
                case ColumnType.Path:
                    {
                        args.rowRect = cellRect;
                        args.label = Path.GetDirectoryName(item.data.Path);
                        base.RowGUI(args);
                    }
                    break;
                case ColumnType.Size:
                    {
                        args.rowRect = cellRect;
                        AssetSerializeInfo.Inst.guidToAsset.TryGetValue(item.data.Guid, out AssetTreeElement ele);
                        args.label = Helper.Path.GetSize(ele != null ? ele.Size : item.data.Size);
                        base.RowGUI(args);
                    }
                    break;
                case ColumnType.Ref:
                    {
                        args.rowRect = cellRect;
                        AssetSerializeInfo.Inst.guidToRef.TryGetValue(item.data.Guid, out int refCount);
                        args.label = refCount == 0 ? "" : refCount.ToString();
                        base.RowGUI(args);
                    }
                    break;
            }
        }

        void BuildGenerticMemu(TreeViewItem<AssetTreeElement> item)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy Path"), false, () => 
            {
                EditorGUIUtility.systemCopyBuffer = item.data.Path;
            });
            menu.AddSeparator("");

            if (_selectionObjects.Count > 0)
            {
                if (_selectionObjects.Count == 1)
                {
                    menu.AddItem(new GUIContent("Delete"), false, Delete);
                    menu.AddItem(new GUIContent("Show In Explorer"), false, () => ShowInExlorer(_selectionObjects[0].Path));
                }
                else
                {
                    menu.AddItem(new GUIContent("Delete All"), false, Delete);
                    menu.AddDisabledItem(new GUIContent("Show In Explorer"));
                }
            }

            menu.ShowAsContext();

        }

        void Delete()
        {
            bool isOK = EditorUtility.DisplayDialog("Warning",
                "Cannot revert after delete, it's recommended to delete after backup.",
                "OK",
                "Cancel");

            if (!isOK)
                return;

            AssetTreeElement curElement = null;
            try
            {
                List<AssetTreeElement> assetElements = new List<AssetTreeElement>();
                for (int i = 0; i < SelectionObjects.Count; i++)
                {
                    curElement = SelectionObjects[i];
                    var items = AssetSerializeInfo.Inst.treeList.FindAll(v => string.CompareOrdinal(v.Guid , curElement.Guid) == 0);
                    if (items != null && items.Count > 0)
                    {
                        var treeData = treeModel.Data.Where(v => string.CompareOrdinal(v.Guid, curElement.Guid) == 0).ToList();
                        if (treeData != null)
                            treeModel.RemoveElements(treeData);

                        for (int j = 0; j < items.Count; j++)
                        {
                            AssetSerializeInfo.Inst.treeList.Remove(items[j]);
                            AssetSerializeInfo.Inst.AllAssetPaths.Remove(items[j].Path);
                        }

                        if (AssetSerializeInfo.Inst.guidToAsset.ContainsKey(curElement.Guid))
                            AssetSerializeInfo.Inst.guidToAsset.Remove(curElement.Guid);

                        if (AssetSerializeInfo.Inst.guidToRef.ContainsKey(curElement.Guid))
                            AssetSerializeInfo.Inst.guidToRef.Remove(curElement.Guid);
                    }

                    AssetDatabase.DeleteAsset(SelectionObjects[i].Path);
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Deleteing have mistake:{0}, Path : {1}", e.Message, curElement.Path);
                throw e;
            }
        }

        void ShowInExlorer(string filePath)
        {
            EditorUtility.RevealInFinder(filePath);
        }

        private List<AssetTreeElement> _selectionObjects = new List<AssetTreeElement>();
    }

}
