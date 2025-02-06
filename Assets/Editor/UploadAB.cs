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
    /// 上传文件到FTP服务器
    /// </summary>
    private async static void UploadFileToFtpServer(string filePath, string fileName)
    {
        // 从线程池中获取一个线程处理上传文件
        await Task.Run(() =>
        {
            try
            {
                FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://192.168.3.39/AB/PC/" + fileName)) as FtpWebRequest;

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
