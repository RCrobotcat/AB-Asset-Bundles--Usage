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

    private Dictionary<string, ABInfo> remoteABInfos = new Dictionary<string, ABInfo>(); // �洢AB����Ϣ

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
    /// ��FTP�����������ļ�
    /// </summary>
    void DownloadFile(string fileName, string localPath)
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

            Debug.Log(fileName + " Download File Success!");
        }
        catch (Exception e)
        {
            Debug.LogError(fileName + " Download File Failed! Exception: " + e.Message);
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
