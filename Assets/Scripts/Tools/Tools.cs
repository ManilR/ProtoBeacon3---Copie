using UnityEngine;

namespace Beacon
{
    public class Tools
    {
        public static void LOG(Component component, string msg)
        {
            if (PlayerPrefs.GetInt("DebugMode", 1) == 1)
                Debug.Log("[" + component.name + "] " + msg);
        }
    }
}
