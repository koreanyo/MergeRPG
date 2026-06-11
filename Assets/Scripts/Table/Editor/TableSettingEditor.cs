using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

[CustomEditor(typeof(TableSetting))]
public class TableSettingEditor : Editor
{
    private TableSetting m_instance;

    string GetLastFolderKey()
    {
        return "LastFolderKey";
    }

    public override void OnInspectorGUI()
    {
        m_instance = (TableSetting)target;
        base.DrawDefaultInspector();

        GUILayout.Space(20);

        if (GUILayout.Button(new GUIContent("Import Table", ""), GUILayout.Width(200)))
        {
            string lastFolderName = PlayerPrefs.GetString(GetLastFolderKey(), "");

            string path = EditorUtility.OpenFilePanel("테이블", lastFolderName, "xlsx");
            //string path = EditorUtility.OpenFolderPanel("선택", lastFolderName, "");

            if (string.IsNullOrEmpty(path) == false)
            {
                ExcelConverter.ToJson(path);

                

                /*
                string directoryName = System.IO.Path.GetDirectoryName(path);

                string[] excelFiles = System.IO.Directory.GetFiles(directoryName);

                bool isCheck = false;

                if(excelFiles != null && excelFiles.Length > 0)
                {
                    for(int i = 0; i < excelFiles.Length; i++)
                    {
                        if(excelFiles[i].ToLower().Contains("table") == true)
                        {
                            isCheck = true;
                        }
                    }
                }

                if (isCheck == true)
                {
                    // Excel 파일을 넣어 주자.
                    for (int i = 0; i < excelFiles.Length; i++)
                    {
                        string str = ExcelConverter.ToJson(excelFiles[i]);

                        string curFileDirectory = TableSetting.instance.GetFilePath();

                        System.IO.StreamWriter file = null;
                        file = new System.IO.StreamWriter(curFileDirectory + "/" + System.IO.Path.GetFileNameWithoutExtension(excelFiles[i]) + ".txt");
                        file.Write(str.ToCharArray());
                        file.Close();
                    }

                    string temp = System.IO.Path.GetDirectoryName(path);

                    PlayerPrefs.SetString(GetLastFolderKey(), temp);
                    PlayerPrefs.Save();

                    AssetDatabase.Refresh();
                }
                else
                {
                    EditorUtility.DisplayDialog("경고", "Excel Table 파일이 존재 하지 않습니다.", "확인");

                    PlayerPrefs.SetString(GetLastFolderKey(), "");
                    PlayerPrefs.Save();
                }
                */
            }
        }
    }

    string OpenFilePanelProperty(string title, string openFolder, string[] openFilters, string inStr)
    {
        string outStr = "";
        GUILayout.BeginHorizontal();

        GUILayout.Label(title);

        outStr = GUILayout.TextArea(inStr); // multi line 못끄나...

        if (GUILayout.Button("Select", GUILayout.MaxWidth(50)))
        {
            string folderName = EditorUtility.OpenFolderPanel("선택", "", "");
            
            if (string.IsNullOrEmpty(folderName) == false)
            {
                outStr = folderName;

                EditorUtility.SetDirty(m_instance);
            }
        }

        GUILayout.EndHorizontal();

        return outStr;
    }
}
