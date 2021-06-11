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
        Icon,
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
        protected override bool CanSearch(string search, AssetTreeElement t)
        {
            return (base.CanSearch(search, t) || IsValidRegex(t.Path, search)) && t.depth == 0;
        }

        protected override void OnSelectChanged()
        {
            if (SelectionObjects.Count == 1)
            {
                AssetTreeElement element = SelectionObjects[0];
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(element.Path);
            }

            LastSelectChanged = true;
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
        internal bool LastSelectChanged { get; set; }
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
                case ColumnType.Icon:
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
                        args.label = Path.GetDirectoryName(item.data.Path).NormalizePath();
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
                        AssetSerializeInfo.Inst.guidToRef.TryGetValue(item.data.Guid, out List<string> refList);
                        args.label = (refList == null || refList.Count == 0) ? "" : refList.Count.ToString();
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

            if (SelectionObjects.Count > 0)
            {
                if (SelectionObjects.Count == 1)
                {
                    menu.AddItem(new GUIContent("Delete"), false, Delete);

                    menu.AddItem(new GUIContent("Show In Explorer"), false, () => ShowInExlorer(SelectionObjects[0].Path));
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

                    AssetDatabase.DeleteAsset(curElement.Path);
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
    }

}
