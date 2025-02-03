using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        ABUpdateManager.Instance.DownloadABCompareFile((isOver) =>
        {
            if (isOver)
            {
                ABUpdateManager.Instance.GetRemoteABCompareInfo();
                ABUpdateManager.Instance.DownLoadABFile(
                (isOver) =>
                {
                    if (isOver)
                    {
                        Debug.Log("Download all AB File Success!");
                    }
                    else
                    {
                        Debug.LogError("Download all AB File Failed! Please check your network connection!");
                    }
                },
                (current, total) =>
                    {
                        Debug.Log("Download Progress: " + current + " / " + total);
                    });
            }
            else
                Debug.LogError("Download AB Comparison File Failed! Please check your network connection!");
        });


    }
}
