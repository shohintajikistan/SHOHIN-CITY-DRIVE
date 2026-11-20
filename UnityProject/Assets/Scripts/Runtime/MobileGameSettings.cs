using UnityEngine;

namespace SHOHIN.CityDrive
{
    public sealed class MobileGameSettings : MonoBehaviour
    {
        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Input.multiTouchEnabled = true;
        }
    }
}
