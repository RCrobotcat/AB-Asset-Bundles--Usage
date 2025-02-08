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

        // ��ȡ�ļ����µ������ļ���Ϣ
        FileInfo[] fileInfos = directory.GetFiles();

        string abCompareInfo = ""; // ���ڴ洢AB���ıȶ���Ϣ

        foreach (FileInfo info in fileInfos)
        {
            // û����չ�����ļ�����AB��
            if (info.Extension == "")
            {
                Debug.Log("File Name: " + info.Name);
                abCompareInfo += info.Name + " | " + info.Length + " | " + GetMD5(info.FullName) + "\n";
            }

            /*Debug.Log("|---------------------------------|");
            Debug.Log("File Name: " + info.Name);
            Debug.Log("File Path: " + info.FullName); // �ļ�·��
            Debug.Log("File Extension" + info.Extension); // �ļ���չ��
            Debug.Log("File Length: " + info.Length);*/
        }

        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1); // ȥ�����һ�����з�

        File.WriteAllText(Application.dataPath + "/Arts/AB/PC/ABComparisonInfo.txt", abCompareInfo); // ��AB���ıȶ���Ϣд���ļ�

        AssetDatabase.Refresh(); // ˢ����Դ
        Debug.Log("Create AB Comparison file Success!");
    }
    /// <summary>
    /// ��ȡ�ļ���MD5����
    /// </summary>
    /// <param name="filePath"></param>
    public static string GetMD5(string filePath)
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