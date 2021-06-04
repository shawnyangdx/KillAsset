using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

namespace KA
{
    internal class DependTreeView : TreeViewWithTreeModel<AssetTreeElement>
    {
        public DependTreeView(TreeViewState state,  TreeModel<AssetTreeElement> model)
            : base(state, model)
        {
            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (20 - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = 2;
        }

    }

}
