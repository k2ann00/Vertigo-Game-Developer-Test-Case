using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Components;
using WheelOfFortune.Reward;
using WheelOfFortune.Events;
using WheelOfFortune.Wheel;

namespace WheelOfFortune.UI.Popups
{
    public class DeveloperPopup : UIPanel
    {

        [Header("Buttons")]
        [SerializeField] private UIButton portfolioButton;
        [SerializeField] private UIButton exitButton;


        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanupButtons();
        }

        private void CleanupButtons()
        {
            if (portfolioButton != null)
            {
                portfolioButton.RemoveListener(OnPortfolioClicked);
            }

            if (exitButton != null)
            {
                exitButton.RemoveListener(OnExitClicked);
            }
        }

        private void SetupButtons()
        {
            if (portfolioButton != null)
            {
                portfolioButton.AddListener(OnPortfolioClicked);
            }

            if (exitButton != null)
            {
                exitButton.AddListener(OnExitClicked);
            }
        }

        private void OnExitClicked()
        {
            Hide(instant: false);
        }

        private void OnPortfolioClicked()
        {
            Application.OpenURL("https://linktr.ee/k2ann00");
        }
    

        public void ShowDeveloperPopup()
        {
            Show(animated: true);
        }
    }
}
