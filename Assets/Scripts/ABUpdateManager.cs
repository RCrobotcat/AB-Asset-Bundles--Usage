using System.IO;
using System.Net;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections;

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

    private Dictionary<string, ABInfo> localABInfos = new Dictionary<string, ABInfo>(); // 存储本地AB包信息

    /// <summary>
    /// AB包热更新
    /// </summary>
    public void CheckUpdate(Action<bool> overCB, Action<string> updateInfoCB)
    {
        remoteABInfos.Clear();
        localABInfos.Clear();
        downloadList.Clear();

        updateInfoCB("Updating ABs...");
        DownloadABCompareFile((isOver) =>
        {
            if (isOver)
            {
                updateInfoCB("Download AB Comparison File Success!");
                string remoteInfo = File.ReadAllText(Application.persistentDataPath + "/ABComparisonInfo_temp.txt");

                updateInfoCB("Get Remote AB Comparison Info...");
                GetRemoteABCompareInfo(remoteInfo, remoteABInfos);
                updateInfoCB("Get Remote AB Comparison Info Success!");

                GetLocalABCompareFileInfo((isOver) =>
                {
                    if (isOver)
                    {
                        updateInfoCB("Get Local AB Comparison Info Success!");

                        updateInfoCB("Starting Comparison...");
                        // 对比AB包信息 => 下载需要更新的AB包
                        foreach (string abName in remoteABInfos.Keys)
                        {
                            // 本地没有该远程AB包 需要下载
                            if (!localABInfos.ContainsKey(abName))
                                downloadList.Add(abName);
                            else
                            {
                                // 本地AB包MD5码与远程AB包MD5码不一致 需要更新
                                if (localABInfos[abName].MD5Code != remoteABInfos[abName].MD5Code)
                                    downloadList.Add(abName);

                                localABInfos.Remove(abName);
                            }
                        }
                        updateInfoCB("Comparison Over!");

                        updateInfoCB("Starting delete redundant local ABs...");
                        // 移除本地多余的AB包
                        foreach (string abName in localABInfos.Keys)
                        {
                            if (File.Exists(Application.persistentDataPath + "/" + abName))
                                File.Delete(Application.persistentDataPath + "/" + abName);
                        }
                        updateInfoCB("Delete redundant local ABs Over!");

                        updateInfoCB("Starting Download ABs...");
                        // 下载新的AB包
                        DownLoadABFile((isOver) =>
                        {
                            if (isOver) // 下载成功之后 更新本地的AB包对比文件
                            {
                                // 把远端的AB包信息保存到本地
                                File.WriteAllText(Application.persistentDataPath + "/ABComparisonInfo.txt", remoteInfo);
                                updateInfoCB("AB Comparison File is up-to-date!");
                            }

                            overCB(isOver);
                        }, updateInfoCB);
                        updateInfoCB("Download ABs Over!");
                    }
                    else overCB(false);

                });
            }
            else
            {
                overCB(false);
            }
        });
    }

    /// <summary>
    /// 下载AB包比对文件
    /// </summary>
    public async void DownloadABCompareFile(Action<bool> OverCallBack)
    {
        // Debug.Log(Application.persistentDataPath); // 本地可读可写路径

        bool isOver = false;
        int reDownloadMaxNum = 5; // 最大重试次数

        string localPath = Application.persistentDataPath;
        while (!isOver && reDownloadMaxNum > 0)
        {
            await Task.Run(() =>
            {
                isOver = DownloadFile("ABComparisonInfo.txt", localPath + "/ABComparisonInfo_temp.txt");
            });

            reDownloadMaxNum--;
        }

        OverCallBack?.Invoke(isOver);
    }
    public void GetRemoteABCompareInfo(string info, Dictionary<string, ABInfo> abInfosDic)
    {
        // string info = File.ReadAllText(Application.persistentDataPath + "/ABComparisonInfo_temp.txt");
        string[] strs = info.Split('\n');
        string[] infos = null;
        for (int i = 0; i < strs.Length; i++)
        {
            infos = strs[i].Split(" | ");
            ABInfo aBInfo = new ABInfo(infos[0], infos[1], infos[2]);
            // remoteABInfos.Add(infos[0], aBInfo);
            abInfosDic.Add(infos[0], aBInfo);
        }

        Debug.Log("Get AB Comparison File Success!");
    }

    /// <summary>
    /// 加载本地AB包比对文件
    /// </summary>
    public void GetLocalABCompareFileInfo(Action<bool> overCB)
    {
        //如果可读可写文件夹中 存在对比文件 说明之前我们已经下载更新过了
        if (File.Exists(Application.persistentDataPath + "/ABComparisonInfo.txt"))
        {
            StartCoroutine(GetLocalABCompareFileInfo(Application.persistentDataPath + "/ABComparisonInfo.txt", overCB));
        }
        //只有当可读可写中没有对比文件时  才会来加载默认资源（第一次进游戏时才会发生）
        else if (File.Exists(Application.streamingAssetsPath + "/ABComparisonInfo.txt"))
        {
            StartCoroutine(GetLocalABCompareFileInfo(Application.streamingAssetsPath + "/ABComparisonInfo.txt", overCB));
        }
        else // 第一次进入游戏时 且没有默认资源时
        {
            overCB(true);
        }
    }
    IEnumerator GetLocalABCompareFileInfo(string filePath, Action<bool> overCB)
    {
        UnityWebRequest req = UnityWebRequest.Get(filePath);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            GetRemoteABCompareInfo(req.downloadHandler.text, localABInfos);
            overCB(true);
        }
        else overCB(false);
    }

    /// <summary>
    /// 下载AB包文件
    /// </summary>
    public async void DownLoadABFile(Action<bool> overCallBack, Action<string> updateProgress)
    {
        /*foreach (string name in remoteABInfos.Keys)
        {
            downloadList.Add(name);
        }*/

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

                    updateProgress(++downloadOverNum + "/" + downloadMaxNum);
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
            FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://127.0.0.1/AB/PC/" + fileName)) as FtpWebRequest;

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
    public class ABInfo
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
