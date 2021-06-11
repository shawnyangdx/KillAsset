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


        private Font m_titleFont;
        private Font m_introFont;
        private GUIStyle m_buildInlabelStyle;
        private GUIStyle m_buildInBoxStyle;
    }
}

