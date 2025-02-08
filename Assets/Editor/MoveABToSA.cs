using UnityEngine;
using UnityEditor;
using System.IO;

public class MoveABToSA
{
    /// <summary>
    /// 将选中的AB包移动到StreamingAssets文件夹下
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
            string assetPath = AssetDatabase.GetAssetPath(asset); // 获取资源路径
            string fileName = assetPath.Substring(assetPath.LastIndexOf("/")); // 获取文件名
            // Debug.Log(fileName);

            if (fileName.IndexOf(".") != -1) // 如果文件名有后缀则跳过（不是AB包）
                continue;

            AssetDatabase.CopyAsset(assetPath, "Assets/StreamingAssets" + fileName);

            FileInfo fileInfo = new FileInfo(Application.streamingAssetsPath + fileName); // 得到文件信息
            abCompareInfo += fileInfo.Name + " | " + fileInfo.Length + " | " + CreateABCompare.GetMD5(fileInfo.FullName) + "\n";
        }

        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1); // 去掉最后一个换行符
        File.WriteAllText(Application.streamingAssetsPath + "/ABComparisonInfo.txt", abCompareInfo); // 将AB包的比对信息写入文件

        AssetDatabase.Refresh(); // 刷新资源
        Debug.Log("Move AB To Streaming Assets Success!");
    }
}
