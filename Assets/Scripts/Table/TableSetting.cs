using UnityEngine;

public class TableSetting : ScriptableObject
{
    private static TableSetting s_instance;
    public static TableSetting instance
    {
        get
        {
            if (s_instance == null)
                s_instance = Resources.Load<TableSetting>("TableSetting");
            return s_instance;
        }
    }

    [SerializeField]
    private string m_filePath = "Assets/Resources/Table";

    public string GetFilePath()
    {
        return m_filePath;
    }
}
