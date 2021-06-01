using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace KA
{
    [CustomEditor(typeof(EditorConfig))]
    public class EditorConfigInspector : Editor
    {
        EditorConfig script;
        private ReorderableList ignoreExtList;
        private ReorderableList ignoreFileList;

        private void OnEnable()
        {
            script = (EditorConfig)target;

            ignoreExtList = InitOrderableList(serializedObject, serializedObject.FindProperty("ignoreExtension"), "Ignore Extension");
            ignoreFileList = InitOrderableList(serializedObject, serializedObject.FindProperty("ignoreDirectory"), "Ignore Directory(Relative Path)");
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Root Path", GUILayout.Width(80f)))
            {
                script.RootPath = EditorUtility.OpenFolderPanel("Select Path", script.RootPath, "");
            }

            EditorGUILayout.LabelField(FileUtil.GetProjectRelativePath(script.RootPath).NormalizePath());
            GuiLine();

            ignoreExtList.DoLayoutList();
            EditorGUILayout.Space();
            ignoreFileList.DoLayoutList();
            GuiLine();

            script.exportType = (EditorConfig.ExportType)EditorGUILayout.EnumPopup("ExportType", script.exportType);
            serializedObject.ApplyModifiedProperties();
        }

        ReorderableList InitOrderableList(SerializedObject so, SerializedProperty property, string name = "")
        {
            var list = new ReorderableList(so, property);
            list.draggable = true;
            list.displayRemove = true;
            list.displayAdd = true;

            list.drawHeaderCallback = rect => DrawHeaderCallback(rect, name);
            list.onAddCallback = v => OnAddCallback(v, property);
            list.onRemoveCallback = v => OnRemoveCallback(v, property);
            list.drawElementCallback = (rect, index, isActive, isFocused) => DrawElementCallback(rect, index, isActive, isFocused, property);

            return list;
        }

        void DrawHeaderCallback(Rect rect, string name)
        {
            EditorGUI.LabelField(rect, name);
        }

        void OnAddCallback(ReorderableList list, SerializedProperty property)
        {
            if (property.arraySize == 0)
                property.InsertArrayElementAtIndex(0);
            else
                property.InsertArrayElementAtIndex(property.arraySize);
        }

        void OnRemoveCallback(ReorderableList list, SerializedProperty property)
        {
            if (list.index < 0 || list.index >= property.arraySize)
                return;

            property.DeleteArrayElementAtIndex(list.index);
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused, SerializedProperty property)
        {
            var newRect = rect;
            newRect.height -= 2;
            rect = newRect;

            if (index < 0 || index >= property.arraySize)
                return;

            var p = property.GetArrayElementAtIndex(index);
            if (p.stringValue == null)
                p.stringValue = "";
            p.stringValue = EditorGUI.TextField(rect, p.stringValue);
        }

        void GuiLine(int i_height = 1)
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space();
        }
    }

}
