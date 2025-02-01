using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        ABUpdateManager.Instance.DownloadABCompareFile();
    }
}
