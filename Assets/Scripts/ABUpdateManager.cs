using System.IO;
using System.Net;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    private List<string> downloadList = new List<string>(); // 存储需要下载的AB包(存储AB包名)

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
    /// 下载AB包文件
    /// </summary>
    public async void DownLoadABFile(Action<bool> overCallBack, Action<int, int> updateProgress)
    {
        foreach (string name in remoteABInfos.Keys)
        {
            downloadList.Add(name);
        }

        // 由于多线程无法读取Unity主进程的资源, 所以需要将下载的AB包存储到本地路径
        string localPath = Application.persistentDataPath + "/";

        bool isOver = false;
        List<string> tempList = new List<string>();

        int reDownloadMaxNum = 5; // 最大重试次数
        int downloadOverNum = 0;
        int downloadMaxNum = downloadList.Count;
        while (downloadList.Count > 0 && reDownloadMaxNum > 0)
        {
            for (int i = 0; i < downloadList.Count; i++)
            {

                await Task.Run(() =>
                {
                    isOver = DownloadFile(downloadList[i], localPath + downloadList[i]);
                });

                if (isOver)
                {
                    // Debug.Log("Download Progress: " + ++downloadOverNum + "/" + downloadList.Count);
                    // Debug.Log("Download AB File " + downloadList[i] + " Success!");

                    updateProgress(++downloadOverNum, downloadMaxNum);
                    tempList.Add(downloadList[i]);
                }
            }

            for (int i = 0; i < tempList.Count; i++)
                downloadList.Remove(tempList[i]);

            reDownloadMaxNum--;
        }

        overCallBack(downloadList.Count == 0);
    }

    /// <summary>
    /// 从FTP服务器下载文件
    /// </summary>
    bool DownloadFile(string fileName, string localPath)
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

            // Debug.Log(fileName + " Download File Success!");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(fileName + " Download File Failed! Exception: " + e.Message);
            return false;
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
