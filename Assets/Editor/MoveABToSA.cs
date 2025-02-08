using UnityEngine;
using UnityEditor;
using System.IO;

public class MoveABToSA
{
    /// <summary>
    /// ��ѡ�е�AB���ƶ���StreamingAssets�ļ�����
    /// </summary>
    // [MenuItem("AB Tools/Move selected AB To Streaming Assets")]
    private static void MoveABToStreamingAssets()
    {
        Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        if (selectedAssets.Length == 0)
            return;

        string abCompareInfo = "";
        foreach (Object asset in selectedAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset); // ��ȡ��Դ·��
            string fileName = assetPath.Substring(assetPath.LastIndexOf("/")); // ��ȡ�ļ���
            // Debug.Log(fileName);

            if (fileName.IndexOf(".") != -1) // ����ļ����к�׺������������AB����
                continue;

            AssetDatabase.CopyAsset(assetPath, "Assets/StreamingAssets" + fileName);

            FileInfo fileInfo = new FileInfo(Application.streamingAssetsPath + fileName); // �õ��ļ���Ϣ
            abCompareInfo += fileInfo.Name + " | " + fileInfo.Length + " | " + CreateABCompare.GetMD5(fileInfo.FullName) + "\n";
        }

        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1); // ȥ�����һ�����з�
        File.WriteAllText(Application.streamingAssetsPath + "/ABComparisonInfo.txt", abCompareInfo); // ��AB���ıȶ���Ϣд���ļ�

        AssetDatabase.Refresh(); // ˢ����Դ
        Debug.Log("Move AB To Streaming Assets Success!");
    }
}
