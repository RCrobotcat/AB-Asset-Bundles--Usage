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

    private Dictionary<string, ABInfo> remoteABInfos = new Dictionary<string, ABInfo>(); // �洢AB����Ϣ
    private List<string> downloadList = new List<string>(); // �洢��Ҫ���ص�AB��(�洢AB����)

    private Dictionary<string, ABInfo> localABInfos = new Dictionary<string, ABInfo>(); // �洢����AB����Ϣ

    /// <summary>
    /// AB���ȸ���
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
                        // �Ա�AB����Ϣ => ������Ҫ���µ�AB��
                        foreach (string abName in remoteABInfos.Keys)
                        {
                            // ����û�и�Զ��AB�� ��Ҫ����
                            if (!localABInfos.ContainsKey(abName))
                                downloadList.Add(abName);
                            else
                            {
                                // ����AB��MD5����Զ��AB��MD5�벻һ�� ��Ҫ����
                                if (localABInfos[abName].MD5Code != remoteABInfos[abName].MD5Code)
                                    downloadList.Add(abName);

                                localABInfos.Remove(abName);
                            }
                        }
                        updateInfoCB("Comparison Over!");

                        updateInfoCB("Starting delete redundant local ABs...");
                        // �Ƴ����ض����AB��
                        foreach (string abName in localABInfos.Keys)
                        {
                            if (File.Exists(Application.persistentDataPath + "/" + abName))
                                File.Delete(Application.persistentDataPath + "/" + abName);
                        }
                        updateInfoCB("Delete redundant local ABs Over!");

                        updateInfoCB("Starting Download ABs...");
                        // �����µ�AB��
                        DownLoadABFile((isOver) =>
                        {
                            if (isOver) // ���سɹ�֮�� ���±��ص�AB���Ա��ļ�
                            {
                                // ��Զ�˵�AB����Ϣ���浽����
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
    /// ����AB���ȶ��ļ�
    /// </summary>
    public async void DownloadABCompareFile(Action<bool> OverCallBack)
    {
        // Debug.Log(Application.persistentDataPath); // ���ؿɶ���д·��

        bool isOver = false;
        int reDownloadMaxNum = 5; // ������Դ���

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
    /// ���ر���AB���ȶ��ļ�
    /// </summary>
    public void GetLocalABCompareFileInfo(Action<bool> overCB)
    {
        //����ɶ���д�ļ����� ���ڶԱ��ļ� ˵��֮ǰ�����Ѿ����ظ��¹���
        if (File.Exists(Application.persistentDataPath + "/ABComparisonInfo.txt"))
        {
            StartCoroutine(GetLocalABCompareFileInfo(Application.persistentDataPath + "/ABComparisonInfo.txt", overCB));
        }
        //ֻ�е��ɶ���д��û�жԱ��ļ�ʱ  �Ż�������Ĭ����Դ����һ�ν���Ϸʱ�Żᷢ����
        else if (File.Exists(Application.streamingAssetsPath + "/ABComparisonInfo.txt"))
        {
            StartCoroutine(GetLocalABCompareFileInfo(Application.streamingAssetsPath + "/ABComparisonInfo.txt", overCB));
        }
        else // ��һ�ν�����Ϸʱ ��û��Ĭ����Դʱ
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
    /// ����AB���ļ�
    /// </summary>
    public async void DownLoadABFile(Action<bool> overCallBack, Action<string> updateProgress)
    {
        /*foreach (string name in remoteABInfos.Keys)
        {
            downloadList.Add(name);
        }*/

        // ���ڶ��߳��޷���ȡUnity�����̵���Դ, ������Ҫ�����ص�AB���洢������·��
        string localPath = Application.persistentDataPath + "/";

        bool isOver = false;
        List<string> tempList = new List<string>();

        int reDownloadMaxNum = 5; // ������Դ���
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
    /// ��FTP�����������ļ�
    /// </summary>
    bool DownloadFile(string fileName, string localPath)
    {
        try
        {
            FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://127.0.0.1/AB/PC/" + fileName)) as FtpWebRequest;

            // ����ƾ֤
            NetworkCredential n = new NetworkCredential("RCrobotcat", "rcrobot123");
            req.Credentials = n;

            req.Proxy = null; // ��ʹ�ô���
            req.KeepAlive = false; // �ϴ���ɺ�ر�����
            req.Method = WebRequestMethods.Ftp.DownloadFile; // ����Ϊ�����ļ�
            req.UseBinary = true; // �����ƴ���

            // �����ļ�
            FtpWebResponse res = req.GetResponse() as FtpWebResponse;
            Stream downloadStream = res.GetResponseStream(); // ��ȡ�����ļ���
            using (FileStream file = File.Create(localPath)) // �򿪱����ļ���, ���ϴ��ļ�����д������
            {
                byte[] bytes = new byte[2048]; // ÿ�ζ�ȡ2KB
                int contentLength = downloadStream.Read(bytes, 0, bytes.Length);

                while (contentLength != 0)
                {
                    file.Write(bytes, 0, contentLength);
                    // ������ȡ�ļ�
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
    /// AB����Ϣ
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
