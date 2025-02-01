using System.IO;
using System.Net;
using System;
using UnityEngine;
using System.Collections.Generic;

public class ABUpdateManager : MonoBehaviour
{
    // Singleton
    private static ABUpdateManager instance;
    public static ABUpdateManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("ABUpdateManager");
                instance = obj.AddComponent<ABUpdateManager>();
            }
            return instance;
        }
    }

    void OnDestroy()
    {
        instance = null;
    }

    private Dictionary<string, ABInfo> remoteABInfos = new Dictionary<string, ABInfo>(); // 存储AB包信息

    /// <summary>
    /// 下载AB包比对文件
    /// </summary>
    public void DownloadABCompareFile()
    {
        Debug.Log(Application.persistentDataPath); // 本地可读可写路径
        DownloadFile("ABComparisonInfo.txt", Application.persistentDataPath + "/ABComparisonInfo.txt");

        string info = File.ReadAllText(Application.persistentDataPath + "/ABComparisonInfo.txt");
        string[] strs = info.Split('\n');
        string[] infos = null;
        for (int i = 0; i < strs.Length; i++)
        {
            infos = strs[i].Split(" | ");
            ABInfo aBInfo = new ABInfo(infos[0], infos[1], infos[2]);
            remoteABInfos.Add(infos[0], aBInfo);
        }

        Debug.Log("Download AB Comparison File Success!");
    }
    /// <summary>
    /// 从FTP服务器下载文件
    /// </summary>
    void DownloadFile(string fileName, string localPath)
    {
        try
        {
            FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://192.168.110.18/AB/PC/" + fileName)) as FtpWebRequest;

            // 设置凭证
            NetworkCredential n = new NetworkCredential("RCrobotcat", "rcrobot123");
            req.Credentials = n;

            req.Proxy = null; // 不使用代理
            req.KeepAlive = false; // 上传完成后关闭连接
            req.Method = WebRequestMethods.Ftp.DownloadFile; // 设置为下载文件
            req.UseBinary = true; // 二进制传输

            // 下载文件
            FtpWebResponse res = req.GetResponse() as FtpWebResponse;
            Stream downloadStream = res.GetResponseStream(); // 获取下载文件流
            using (FileStream file = File.Create(localPath)) // 打开本地文件流, 往上传文件流中写入数据
            {
                byte[] bytes = new byte[2048]; // 每次读取2KB
                int contentLength = downloadStream.Read(bytes, 0, bytes.Length);

                while (contentLength != 0)
                {
                    file.Write(bytes, 0, contentLength);
                    // 继续读取文件
                    contentLength = file.Read(bytes, 0, bytes.Length);
                }

                file.Close();
                downloadStream.Close();
            }

            Debug.Log(fileName + " Download File Success!");
        }
        catch (Exception e)
        {
            Debug.LogError(fileName + " Download File Failed! Exception: " + e.Message);
        }
    }

    /// <summary>
    /// AB包信息
    /// </summary>
    private class ABInfo
    {
        public string name;
        public long size;
        public string MD5Code;

        public ABInfo(string name, string size, string MD5Code)
        {
            this.name = name;
            this.size = long.Parse(size);
            this.MD5Code = MD5Code;
        }
    }
}
