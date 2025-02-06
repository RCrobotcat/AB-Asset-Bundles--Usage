using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class UploadAB
{
    [MenuItem("AB Tools/Upload AB Files")]
    private static void UploadAllABFile()
    {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/Arts/AB/PC/");

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
    /// �ϴ��ļ���FTP������
    /// </summary>
    private async static void UploadFileToFtpServer(string filePath, string fileName)
    {
        // ���̳߳��л�ȡһ���̴߳����ϴ��ļ�
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://192.168.3.39/AB/PC/" + fileName)) as FtpWebRequest;

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
