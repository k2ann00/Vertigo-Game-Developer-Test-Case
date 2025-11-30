using UnityEngine;
using TMPro;
using UnityEngine.UI;
using WheelOfFortune.Events;
using System;
using WheelOfFortune.UI.Popups;

namespace WheelOfFortune.UI.Screens
{
    public class RightSidebarUI : UIPanel
    {

        [Header("Developer Button")]
        [SerializeField] private GameObject devButton;
        [SerializeField] private DeveloperPopup devPopup;



        protected override void Awake()
        {
            base.Awake();
            
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        // Event Subscriptions

        private void SubscribeToEvents()
        {
            GameEvents.OnDeveloperButton += OpenDeveloperPopup;
        }

        

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnDeveloperButton -= OpenDeveloperPopup;
        }

        private void OpenDeveloperPopup()
        {
            Debug.Log("[RightSidebarUI] OpenDeveloperPopup called.");
            if (devPopup != null)
            {
                devPopup.Show();
            }
            else
            {
                Debug.LogWarning("[RightSidebarUI] DeveloperPopup reference is missing.");
            }
        }

    }
}
