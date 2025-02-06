using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        Debug.Log(Application.persistentDataPath);
        ABUpdateManager.Instance.CheckUpdate((isOver) =>
        {
            if (isOver)
            {
                Debug.Log("Update AB Success!");
            }
            else
            {
                Debug.Log("Update AB Failed! Check your network connections.");
            }
        },
        (str) =>
        {
            Debug.Log(str);
        });
    }
}
