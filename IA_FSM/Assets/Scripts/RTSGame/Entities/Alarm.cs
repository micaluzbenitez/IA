using System;
using UnityEngine;

namespace RTSGame.Entities
{
    public class Alarm : MonoBehaviour
    {
        public static Action OnStartAlarm;
        public static Action OnStopAlarm;

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