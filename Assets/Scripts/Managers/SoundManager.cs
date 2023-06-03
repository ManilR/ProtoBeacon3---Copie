using UnityEngine;

namespace Beacon 
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

    }
}
