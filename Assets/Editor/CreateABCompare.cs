using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class CreateABCompare
{
    // [MenuItem("AB Tools/Create Comparison File")]
    public static void CreateABCompareFile()
    {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/Arts/AB/PC/");

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

        File.WriteAllText(Application.dataPath + "/Arts/AB/PC/ABComparisonInfo.txt", abCompareInfo); // 将AB包的比对信息写入文件

        AssetDatabase.Refresh(); // 刷新资源
        Debug.Log("Create AB Comparison file Success!");
    }
    /// <summary>
    /// 获取文件的MD5编码
    /// </summary>
    /// <param name="filePath"></param>
    public static string GetMD5(string filePath)
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
}