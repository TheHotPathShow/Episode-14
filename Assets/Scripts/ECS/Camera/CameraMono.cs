using UnityEngine;

public class CameraMono : MonoBehaviour
{
    public static CameraMono Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}