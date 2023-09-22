using System;
using UnityEngine;

namespace RTSGame
{
    public class Alarm : MonoBehaviour
    {
        public Action OnStartAlarm;
        public Action OnStopAlarm;

        public void StartAlarm()
        {
            OnStartAlarm?.Invoke();
        }

        public void StopAlarm()
        {
            OnStopAlarm?.Invoke();
        }
    }
}