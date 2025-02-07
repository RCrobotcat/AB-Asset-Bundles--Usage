using UnityEngine;
using UnityEditor;

public class ABTools : EditorWindow
{
    int currentSelectedIndex = 0;
    string[] targetSelectedStrings = new string[]
    {
        "PC",
        "Android",
        "IOS",
    };

    string serverIP = "ftp://127.0.0.1";

    [MenuItem("AB Tools/Open AB Tools Window")]
    private static void OpenWindow()
    {
        ABTools anTools = EditorWindow.GetWindowWithRect(typeof(ABTools), new Rect(0, 0, 300, 200)) as ABTools;
        anTools.Show();
    }

    private void OnGUI()
    {
        // ѡ��ƽ̨
        GUI.Label(new Rect(10, 10, 150, 15), "Platform Selection");
        currentSelectedIndex = GUI.Toolbar(new Rect(10, 30, 250, 20), currentSelectedIndex, targetSelectedStrings);

        // ��Դ������IP��ַ����
        GUI.Label(new Rect(10, 60, 150, 15), "AssetBundle Server IP");
        serverIP = GUI.TextField(new Rect(10, 80, 150, 20), serverIP);

        // �����Ա��ļ���ť
        if (GUI.Button(new Rect(10, 110, 200, 20), "Create AB Comparison File"))
        {

        }

        // ����Ĭ����Դ�Ա��ļ���StreamingAssetsĿ¼��ť
        if (GUI.Button(new Rect(10, 140, 200, 20), "Save AB To StreamingAssets"))
        {

        }

        // �ϴ�AB������Դ��������ť
        if (GUI.Button(new Rect(10, 170, 150, 20), "Upload AB to Server"))
        {

        }
    }
}
