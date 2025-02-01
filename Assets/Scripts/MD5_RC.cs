using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class MD5_RC : MonoBehaviour
{
    void Start()
    {
        string test_md5 = GetMD5(Application.dataPath + "/Arts/AB/PC/lua");
        Debug.Log("MD5: " + test_md5);
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
}
