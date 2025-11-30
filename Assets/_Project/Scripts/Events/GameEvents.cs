using System;
using UnityEngine;
using WheelOfFortune.Data;

namespace WheelOfFortune.Events
{
    public static class GameEvents
    {

        public static event Action OnWheelSpinStarted;

        public static event Action<int> OnWheelSpinCompleted; // sliceIndex

        public static event Action<RewardData> OnRewardCollected;

        public static event Action OnBombHit;

        public static event Action<int> OnZoneChanged; // newZoneNumber

        public static event Action<int> OnSafeZoneEntered; // zoneNumber

        public static event Action<int> OnSuperZoneEntered; // zoneNumber

        public static event Action OnGameOver;

        public static event Action OnGameRestart;

        public static event Action<bool> OnResultPopupClosed; // bool autoSpin

        public static event Action OnGameExit;

        public static event Action OnDeveloperButton;

        public static void TriggerWheelSpinStarted()
        {
            OnWheelSpinStarted?.Invoke();
        }

        public static void TriggerWheelSpinCompleted(int sliceIndex)
        {
            OnWheelSpinCompleted?.Invoke(sliceIndex);
        }

        public static void TriggerRewardCollected(RewardData reward)
        {
            OnRewardCollected?.Invoke(reward);
        }

        public static void TriggerBombHit()
        {
            OnBombHit?.Invoke();
        }

        public static void TriggerZoneChanged(int newZoneNumber)
        {
            OnZoneChanged?.Invoke(newZoneNumber);
        }

        public static void TriggerSafeZoneEntered(int zoneNumber)
        {
            OnSafeZoneEntered?.Invoke(zoneNumber);
        }

        public static void TriggerSuperZoneEntered(int zoneNumber)
        {
            OnSuperZoneEntered?.Invoke(zoneNumber);
        }

        public static void TriggerGameOver()
        {
            OnGameOver?.Invoke();
        }

        public static void TriggerGameRestart()
        {
            OnGameRestart?.Invoke();
        }

        public static void TriggerResultPopupClosed(bool autoSpin = false)
        {
            OnResultPopupClosed?.Invoke(autoSpin);
        }

        public static void TriggerGameExit()
        {
            OnGameExit?.Invoke();
        }

        public static void TriggerDevPopup()
        {
            OnDeveloperButton?.Invoke();
        }

        public static void ClearAllEvents()
        {
            OnWheelSpinStarted = null;
            OnWheelSpinCompleted = null;
            OnRewardCollected = null;
            OnBombHit = null;
            OnZoneChanged = null;
            OnSafeZoneEntered = null;
            OnSuperZoneEntered = null;
            OnGameOver = null;
            OnGameRestart = null;
            OnResultPopupClosed = null;
            OnGameExit = null;
            OnDeveloperButton = null;
        }

    }
}
