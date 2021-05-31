using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace KA
{
    public class ReportWindow : EditorWindow
    {
        private static ReportWindow reportWindow;

        public static void Open()
        {
            reportWindow = GetWindow<ReportWindow>();
            reportWindow.titleContent = new GUIContent("Report");
            reportWindow.position = new Rect(
                MainWindow.Inst.position.x + MainWindow.Inst.position.width / 4,
                MainWindow.Inst.position.y + MainWindow.Inst.position.height / 4,
                MainWindow.Inst.position.width / 2,
                MainWindow.Inst.position.height / 2);

            reportWindow.AnalizeReport();
        }


        private void OnGUI()
        {
            //var e = reportInfos.GetEnumerator();
            //int index = 0;
            //while(e.MoveNext())
            //{
            //    var current = e.Current;
            //    GUI.Label(GetLabelRect(index++), current.pa)
            //}
        }

        Rect GetLabelRect(int index)
        {
            return new Rect(5, 5 + 20 * index, 300, 200);
        }

        void AnalizeReport()
        {
            var guidToAsset = AssetSerializeInfo.Inst.guidToAsset;
            foreach (var item in guidToAsset)
            {
                var asset = item.Value;
                if(!reportInfos.TryGetValue(asset.Path, out ReportInfo info))
                {
                    info = new ReportInfo();
                    info.size = asset.Size;
                    info.fileNum = 1;

                    reportInfos.Add(asset.Path, info);
                }
                else
                {
                    info.size += asset.Size;
                    info.fileNum++;
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
    }
}
