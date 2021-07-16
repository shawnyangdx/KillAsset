using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KA
{
    public class GUIStyleMgr
    {
        private static GUIStyleMgr _instance;
        public static GUIStyleMgr Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GUIStyleMgr();

                return _instance;
            }
        }

        public Font TitleFont
        {
            get
            {
                if (m_titleFont == null)
                    m_titleFont = Resources.Load<Font>("Font/STCAIYUN");

                return m_titleFont;
            }
        }

        public Font IntroFont
        {
            get
            {
                if (m_introFont == null)
                    m_introFont = Resources.Load<Font>("Font/VERDANA");

                return m_introFont;
            }
        }

        public GUIStyle BuildinLabelStyle
        {
            get
            {
                if (m_buildInlabelStyle == null)
                {
                    var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
                    m_buildInlabelStyle = new GUIStyle(skin.label);
                }

                return m_buildInlabelStyle;
            }
        }

        public GUIStyle BuildinBoxStyle
        {
            get
            {
                if (m_buildInBoxStyle == null)
                {
                    var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
                    m_buildInBoxStyle = new GUIStyle(skin.box);
                }

                return m_buildInBoxStyle;
            }
        }

        public GUIContent TreeEditorRefresh
        {
            get
            {
                if (m_TreeEditorRfresh == null)
                    m_TreeEditorRfresh = EditorGUIUtility.IconContent("TreeEditor.Refresh");

                return m_TreeEditorRfresh;
            }
        }


        public GUIContent SettingContent
        {
            get
            {
                if (m_SettingContent == null)
                {
                    var tex = Resources.Load<Texture>("Texture/SettingIcon");
                    m_SettingContent = new GUIContent(tex);
                    m_SettingContent.tooltip = "Show Config In Inspector.";
                }

                return m_SettingContent;
            }
        }

        private Font m_titleFont;
        private Font m_introFont;
        private GUIStyle m_buildInlabelStyle;
        private GUIStyle m_buildInBoxStyle;

        private GUIContent m_TreeEditorRfresh;
        private GUIContent m_SettingContent;
    }
}

