using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

namespace KA
{
    public class ReportWindow : EditorWindow
    {
        private static ReportWindow reportWindow;

        public static void Open()
        {
            reportWindow = GetWindow<ReportWindow>();
            reportWindow.titleContent = new GUIContent("Report");

            Vector2 size = new Vector2(
                MainWindow.Inst.position.width / 2, 
                MainWindow.Inst.position.height / 2);

            reportWindow.position = new Rect(
                MainWindow.Inst.position.x + MainWindow.Inst.position.width / 4,
                MainWindow.Inst.position.y + MainWindow.Inst.position.height / 4,
                size.x,
                size.y);

            reportWindow.maxSize = size;
            reportWindow.minSize = size;
        }

        private void OnEnable()
        {
            reportInfos.Clear();
            AnalizeReport();
        }

        private void OnGUI()
        {
            int index = 0;
            scrollPane = GUI.BeginScrollView(GetScrollPosRect(), scrollPane, GetScrollViewRect());
            var e = reportInfos.GetEnumerator();

            GUI.Label(GetLabelRect(index++), "Total:", EditorStyles.boldLabel);
            GUI.Label(GetLabelRect(index++), string.Format("Files:{0}, Size:{1}", totalFiles, Helper.Path.GetSize(totalSize)));
            GUI.Label(GetLabelRect(index++), "");

            GUI.Label(GetLabelRect(index++), "Detail:", EditorStyles.boldLabel);
            while (e.MoveNext())
            {
                var current = e.Current;
                string msg = string.Format("{0}: Size:{1}, File Num:{2}", 
                    current.Key, 
                    Helper.Path.GetSize(current.Value.size), 
                    current.Value.fileNum);
                GUI.Label(GetLabelRect(index++), msg);
            }

            if(GUI.Button(GetExportBtn(index), "Export"))
            {
                AssetSerializeInfo.Inst.Export();
            }

            GUI.EndScrollView();
        }

        Rect GetLabelRect(int index)
        {
            return new Rect(5, 5 + 20 * index, position.width - 20, 200);
        }

        Rect GetExportBtn(int index)
        {
            return new Rect(5, 5 + 20 * index, 100, 20);
        }

        Rect GetScrollPosRect()
        {
            return new Rect(5, 5, position.width - 5, position.height);
        }

        Rect GetScrollViewRect()
        {
            return new Rect(0, 0, position.width - 20, position.height + 20 * (reportInfos.Count - Helper.WindowParam.ReportWindowMaxLine));
        }

        void AnalizeReport()
        {
            var guidToAsset = AssetSerializeInfo.Inst.guidToAsset;
            foreach (var item in guidToAsset)
            {
                var asset = item.Value;
                string directoryName = Path.GetDirectoryName(asset.Path);
                if(!reportInfos.TryGetValue(directoryName, out ReportInfo info))
                {
                    info = new ReportInfo();
                    info.size = asset.Size;
                    info.fileNum = 1;
                    reportInfos.Add(directoryName, info);
                    totalFiles++;
                    totalSize += asset.Size;
                }
                else
                {
                    info.size += asset.Size;
                    info.fileNum++;
                    reportInfos[directoryName] = info;
                    totalFiles++;
                    totalSize += asset.Size;
                }
            }

            List<KeyValuePair<string, ReportInfo>> dicList = reportInfos.ToList();

            dicList.Sort((KeyValuePair<string, ReportInfo> pair1,
                            KeyValuePair<string, ReportInfo> pair2) =>
                 {
                     return pair1.Key.CompareTo(pair2.Key);
                 }
             );

            reportInfos = dicList.ToDictionary(v => v.Key, v => v.Value);
        }

        public struct ReportInfo
        {
            public long size;
            public int fileNum;
        }

        Dictionary<string, ReportInfo> reportInfos = new Dictionary<string, ReportInfo>();
        int totalFiles = 0;
        long totalSize = 0;
        Vector2 scrollPane = Vector2.zero;
    }
}
