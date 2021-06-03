﻿
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace KA
{
    public static class Helper
    {
        public static class WindowParam
        {
            public static float WorkflowBoxWidth = 120;
            public static float RightExpendOffset = 110;
            public static float RightBoardOffset = 130;
            public static float BottomBoardOffset = 10;
            public static float ReportWindowMaxLine = 20;
        }

        public static class Path
        {
            public static string GetSize(long bytes)
            {
                if (bytes <= 0)
                    return "";

                if (bytes > 0 && bytes < 1024 * 1024)
                {
                    float size = bytes / 1024.0f;
                    return string.Format("{0:F1}kb", size);
                }
                else
                {
                    float size = bytes / 1024.0f / 1024.0f;
                    return string.Format("{0:F1}mb", size);
                }
            }

            public static List<string> CollectAssetPaths(string rootPath)
            {
               return Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
                    .Where(v => !AssetTreeHelper.IgnoreExtension(v))
                    .Select(v => FileUtil.GetProjectRelativePath(v).NormalizePath())
                    .Where(v => !AssetTreeHelper.IgnoreDirectory(v))
                    .ToList();
            }
        }
    }
}

