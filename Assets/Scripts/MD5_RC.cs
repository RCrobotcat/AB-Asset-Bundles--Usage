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
    /// ��ȡ�ļ���MD5����
    /// </summary>
    /// <param name="filePath"></param>
    string GetMD5(string filePath)
    {
        using (FileStream file = new FileStream(filePath, FileMode.Open))  // ���ļ���������ʽ��
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5Info = md5.ComputeHash(file);  // �����ļ���MD5����(16���ֽ�)

            file.Close();  // �ر��ļ���

            // ��MD5����ת��Ϊ16�����ַ��� => Ϊ�˼�СMD5����ĳ���
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < md5Info.Length; i++)
            {
                sb.Append(md5Info[i].ToString("x2")); // ת��Ϊ16���� => Сдx2 ��д��ΪX2
            }

            return sb.ToString();
        }
    }
}
