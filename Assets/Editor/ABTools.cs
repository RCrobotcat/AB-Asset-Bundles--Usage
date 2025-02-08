using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Object = UnityEngine.Object;
using System.Security.Cryptography;
using System.Text;

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
            CreateABCompareFile();
        }

        // ����Ĭ����Դ�Ա��ļ���StreamingAssetsĿ¼��ť
        if (GUI.Button(new Rect(10, 140, 200, 20), "Save AB To StreamingAssets"))
        {
            MoveABToStreamingAssets();
        }

        // �ϴ�AB������Դ��������ť
        if (GUI.Button(new Rect(10, 170, 150, 20), "Upload AB to Server"))
        {
            UploadAllABFile();
        }
    }

    /// <summary>
    /// ����AB���Ա��ļ�
    /// </summary>
    void CreateABCompareFile()
    {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/Arts/AB/" + targetSelectedStrings[currentSelectedIndex] + "/");

        // ��ȡ�ļ����µ������ļ���Ϣ
        FileInfo[] fileInfos = directory.GetFiles();

        string abCompareInfo = ""; // ���ڴ洢AB���ıȶ���Ϣ

        foreach (FileInfo info in fileInfos)
        {
            // û����չ�����ļ�����AB��
            if (info.Extension == "")
            {
                Debug.Log("File Name: " + info.Name);
                abCompareInfo += info.Name + " | " + info.Length + " | " + GetMD5(info.FullName) + "\n";
            }

            /*Debug.Log("|---------------------------------|");
            Debug.Log("File Name: " + info.Name);
            Debug.Log("File Path: " + info.FullName); // �ļ�·��
            Debug.Log("File Extension" + info.Extension); // �ļ���չ��
            Debug.Log("File Length: " + info.Length);*/
        }

        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1); // ȥ�����һ�����з�

        File.WriteAllText(Application.dataPath + "/Arts/AB/" + targetSelectedStrings[currentSelectedIndex] + "/ABComparisonInfo.txt", abCompareInfo); // ��AB���ıȶ���Ϣд���ļ�

        AssetDatabase.Refresh(); // ˢ����Դ
        Debug.Log("Create AB Comparison file Success!");
    }
    /// <summary>
    /// ��ȡ�ļ���MD5����
    /// </summary>
    /// <param name="filePath"></param>
    string GetMD5(string filePath)
    {
        using (FileStream file = new FileStream(filePath, FileMode.Open))  // ���ļ���������ʽ��
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5Info = md5.ComputeHash(file);  // �����ļ���MD5����(16���ֽ�)

            file.Close();  // �ر��ļ���

            // ��MD5����ת��Ϊ16�����ַ��� => Ϊ�˼�СMD5����ĳ���
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < md5Info.Length; i++)
            {
                sb.Append(md5Info[i].ToString("x2")); // ת��Ϊ16���� => Сдx2 ��д��ΪX2
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// ��ѡ�е�AB���ƶ���StreamingAssets�ļ�����
    /// </summary>
    void MoveABToStreamingAssets()
    {
        Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        if (selectedAssets.Length == 0)
            return;

        string abCompareInfo = "";
        foreach (Object asset in selectedAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset); // ��ȡ��Դ·��
            string fileName = assetPath.Substring(assetPath.LastIndexOf("/")); // ��ȡ�ļ���
            // Debug.Log(fileName);

            if (fileName.IndexOf(".") != -1) // ����ļ����к�׺������������AB����
                continue;

            AssetDatabase.CopyAsset(assetPath, "Assets/StreamingAssets" + fileName);

            FileInfo fileInfo = new FileInfo(Application.streamingAssetsPath + fileName); // �õ��ļ���Ϣ
            abCompareInfo += fileInfo.Name + " | " + fileInfo.Length + " | " + CreateABCompare.GetMD5(fileInfo.FullName) + "\n";
        }

        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1); // ȥ�����һ�����з�
        File.WriteAllText(Application.streamingAssetsPath + "/ABComparisonInfo.txt", abCompareInfo); // ��AB���ıȶ���Ϣд���ļ�

        AssetDatabase.Refresh(); // ˢ����Դ
        Debug.Log("Move AB To Streaming Assets Success!");
    }

    /// <summary>
    /// �ϴ�����AB����FTP������
    /// </summary>
    void UploadAllABFile()
    {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/Arts/AB/" + targetSelectedStrings[currentSelectedIndex] + "/");

        // ��ȡ�ļ����µ������ļ���Ϣ
        FileInfo[] fileInfos = directory.GetFiles();

        foreach (FileInfo info in fileInfos)
        {
            if (info.Extension == "" || info.Extension == ".txt")
            {
                // �ϴ��ļ�
                UploadFileToFtpServer(info.FullName, info.Name);
            }
        }
    }
    /// <summary>
    /// �첽�ϴ��ļ���FTP������
    /// </summary>
    private async void UploadFileToFtpServer(string filePath, string fileName)
    {
        // ���̳߳��л�ȡһ���̴߳����ϴ��ļ�
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest req = FtpWebRequest.Create(new Uri(serverIP + "/AB/" + targetSelectedStrings[currentSelectedIndex] + "/" + fileName)) as FtpWebRequest;

                // ����ƾ֤
                NetworkCredential n = new NetworkCredential("RCrobotcat", "rcrobot123");
                req.Credentials = n;

                req.Proxy = null; // ��ʹ�ô���
                req.KeepAlive = false; // �ϴ���ɺ�ر�����
                req.Method = WebRequestMethods.Ftp.UploadFile; // ����Ϊ�ϴ��ļ�
                req.UseBinary = true; // �����ƴ���

                // �ϴ��ļ�
                Stream uploadStream = req.GetRequestStream(); // ��ȡ�ϴ��ļ���
                using (FileStream file = File.OpenRead(filePath)) // �򿪱����ļ���, ���ϴ��ļ�����д������
                {
                    byte[] bytes = new byte[2048]; // ÿ�ζ�ȡ2KB
                    int contentLength = file.Read(bytes, 0, bytes.Length);

                    while (contentLength != 0)
                    {
                        uploadStream.Write(bytes, 0, contentLength);
                        // ������ȡ�ļ�
                        contentLength = file.Read(bytes, 0, bytes.Length);
                    }

                    file.Close();
                    uploadStream.Close();
                }

                Debug.Log(fileName + " Upload File Success!");
            }
            catch (Exception e)
            {
                Debug.LogError(fileName + " Upload File Failed! Exception: " + e.Message);
            }
        });
    }
}
