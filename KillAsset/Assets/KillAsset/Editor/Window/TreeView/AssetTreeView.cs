using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace KA
{
    enum ColumnType
    {
        Icon1,
        Name,
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
            //multicolumnHeader.sortingChanged += OnSortingChanged;

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
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case ColumnType.Icon1:
                    {
                        GUI.DrawTexture(cellRect, item.data.Icon, ScaleMode.ScaleToFit);
                    }
                    break;
                case ColumnType.Name:
                    {
                        // Do toggle
                        // Default icon and label
                        //var nameRect = cellRect;
                        //nameRect.x += GetContentIndent(item);
                        args.rowRect = cellRect;
                        base.RowGUI(args);
                    }
                    break;
            }
        }


        private List<AssetTreeElement> _selectionObjects = new List<AssetTreeElement>();
    }

}
