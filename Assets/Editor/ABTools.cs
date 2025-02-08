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
        // 选择平台
        GUI.Label(new Rect(10, 10, 150, 15), "Platform Selection");
        currentSelectedIndex = GUI.Toolbar(new Rect(10, 30, 250, 20), currentSelectedIndex, targetSelectedStrings);

        // 资源服务器IP地址设置
        GUI.Label(new Rect(10, 60, 150, 15), "AssetBundle Server IP");
        serverIP = GUI.TextField(new Rect(10, 80, 150, 20), serverIP);

        // 创建对比文件按钮
        if (GUI.Button(new Rect(10, 110, 200, 20), "Create AB Comparison File"))
        {
            CreateABCompareFile();
        }

        // 保存默认资源对比文件到StreamingAssets目录按钮
        if (GUI.Button(new Rect(10, 140, 200, 20), "Save AB To StreamingAssets"))
        {
            MoveABToStreamingAssets();
        }

        // 上传AB包到资源服务器按钮
        if (GUI.Button(new Rect(10, 170, 150, 20), "Upload AB to Server"))
        {
            UploadAllABFile();
        }
    }

    /// <summary>
    /// 创建AB包对比文件
    /// </summary>
    void CreateABCompareFile()
    {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/Arts/AB/" + targetSelectedStrings[currentSelectedIndex] + "/");

        // 获取文件夹下的所有文件信息
        FileInfo[] fileInfos = directory.GetFiles();

        string abCompareInfo = ""; // 用于存储AB包的比对信息

        foreach (FileInfo info in fileInfos)
        {
            // 没有扩展名的文件才是AB包
            if (info.Extension == "")
            {
                Debug.Log("File Name: " + info.Name);
                abCompareInfo += info.Name + " | " + info.Length + " | " + GetMD5(info.FullName) + "\n";
            }

            /*Debug.Log("|---------------------------------|");
            Debug.Log("File Name: " + info.Name);
            Debug.Log("File Path: " + info.FullName); // 文件路径
            Debug.Log("File Extension" + info.Extension); // 文件扩展名
            Debug.Log("File Length: " + info.Length);*/
        }

        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1); // 去掉最后一个换行符

        File.WriteAllText(Application.dataPath + "/Arts/AB/" + targetSelectedStrings[currentSelectedIndex] + "/ABComparisonInfo.txt", abCompareInfo); // 将AB包的比对信息写入文件

        AssetDatabase.Refresh(); // 刷新资源
        Debug.Log("Create AB Comparison file Success!");
    }
    /// <summary>
    /// 获取文件的MD5编码
    /// </summary>
    /// <param name="filePath"></param>
    string GetMD5(string filePath)
    {
        using (FileStream file = new FileStream(filePath, FileMode.Open))  // 将文件以流的形式打开
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5Info = md5.ComputeHash(file);  // 计算文件的MD5编码(16个字节)

            file.Close();  // 关闭文件流

            // 将MD5编码转换为16进制字符串 => 为了减小MD5编码的长度
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < md5Info.Length; i++)
            {
                sb.Append(md5Info[i].ToString("x2")); // 转换为16进制 => 小写x2 大写则为X2
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// 将选中的AB包移动到StreamingAssets文件夹下
    /// </summary>
    void MoveABToStreamingAssets()
    {
        Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        if (selectedAssets.Length == 0)
            return;

        string abCompareInfo = "";
        foreach (Object asset in selectedAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset); // 获取资源路径
            string fileName = assetPath.Substring(assetPath.LastIndexOf("/")); // 获取文件名
            // Debug.Log(fileName);

            if (fileName.IndexOf(".") != -1) // 如果文件名有后缀则跳过（不是AB包）
                continue;

            AssetDatabase.CopyAsset(assetPath, "Assets/StreamingAssets" + fileName);

            FileInfo fileInfo = new FileInfo(Application.streamingAssetsPath + fileName); // 得到文件信息
            abCompareInfo += fileInfo.Name + " | " + fileInfo.Length + " | " + CreateABCompare.GetMD5(fileInfo.FullName) + "\n";
        }

        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1); // 去掉最后一个换行符
        File.WriteAllText(Application.streamingAssetsPath + "/ABComparisonInfo.txt", abCompareInfo); // 将AB包的比对信息写入文件

        AssetDatabase.Refresh(); // 刷新资源
        Debug.Log("Move AB To Streaming Assets Success!");
    }

    /// <summary>
    /// 上传所有AB包到FTP服务器
    /// </summary>
    void UploadAllABFile()
    {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/Arts/AB/" + targetSelectedStrings[currentSelectedIndex] + "/");

        // 获取文件夹下的所有文件信息
        FileInfo[] fileInfos = directory.GetFiles();

        foreach (FileInfo info in fileInfos)
        {
            if (info.Extension == "" || info.Extension == ".txt")
            {
                // 上传文件
                UploadFileToFtpServer(info.FullName, info.Name);
            }
        }
    }
    /// <summary>
    /// 异步上传文件到FTP服务器
    /// </summary>
    private async void UploadFileToFtpServer(string filePath, string fileName)
    {
        // 从线程池中获取一个线程处理上传文件
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest req = FtpWebRequest.Create(new Uri(serverIP + "/AB/" + targetSelectedStrings[currentSelectedIndex] + "/" + fileName)) as FtpWebRequest;

                // 设置凭证
                NetworkCredential n = new NetworkCredential("RCrobotcat", "rcrobot123");
                req.Credentials = n;

                req.Proxy = null; // 不使用代理
                req.KeepAlive = false; // 上传完成后关闭连接
                req.Method = WebRequestMethods.Ftp.UploadFile; // 设置为上传文件
                req.UseBinary = true; // 二进制传输

                // 上传文件
                Stream uploadStream = req.GetRequestStream(); // 获取上传文件流
                using (FileStream file = File.OpenRead(filePath)) // 打开本地文件流, 往上传文件流中写入数据
                {
                    byte[] bytes = new byte[2048]; // 每次读取2KB
                    int contentLength = file.Read(bytes, 0, bytes.Length);

                    while (contentLength != 0)
                    {
                        uploadStream.Write(bytes, 0, contentLength);
                        // 继续读取文件
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
