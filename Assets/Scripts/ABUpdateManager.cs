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

    private Dictionary<string, ABInfo> remoteABInfos = new Dictionary<string, ABInfo>(); // �洢AB����Ϣ
    private List<string> downloadList = new List<string>(); // �洢��Ҫ���ص�AB��(�洢AB����)

    /// <summary>
    /// ����AB���ȶ��ļ�
    /// </summary>
    public void DownloadABCompareFile()
    {
        Debug.Log(Application.persistentDataPath); // ���ؿɶ���д·��
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
    /// ����AB���ļ�
    /// </summary>
    public async void DownLoadABFile(Action<bool> overCallBack, Action<int, int> updateProgress)
    {
        foreach (string name in remoteABInfos.Keys)
        {
            downloadList.Add(name);
        }

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
    /// ��FTP�����������ļ�
    /// </summary>
    bool DownloadFile(string fileName, string localPath)
    {
        try
        {
            FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://192.168.110.18/AB/PC/" + fileName)) as FtpWebRequest;

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
