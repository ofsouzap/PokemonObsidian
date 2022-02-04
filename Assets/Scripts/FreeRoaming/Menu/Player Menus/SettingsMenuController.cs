using Menus;
using System;
using UnityEngine;
using UnityEngine.UI;
using Audio;

namespace FreeRoaming.Menu.PlayerMenus
{
    public class SettingsMenuController : PlayerMenuController
    {

        public Button backButton;

        [Header("Text Speed")]
        public Text textSpeedNameText;
        public Slider textSpeedSlider;

        [Header("Sound")]
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;

        protected override MenuSelectableController[] GetSelectables()
        {

            return new MenuSelectableController[]
            {
                textSpeedSlider.GetComponent<MenuSelectableController>()
            };

        }

        protected override GameObject GetDefaultSelectedGameObject()
             => backButton.gameObject;

        protected override void Start()
        {

            base.Start();

            if (textSpeedSlider.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("No MenuSelectableController on text speed slider");

        }

        protected override void SetUp()
        {

            backButton.onClick.AddListener(() => CloseMenu());

            #region Text Speed

            GameSettings.TextSpeed textSpeed = GameSettings.singleton.textSpeed;

            textSpeedSlider.minValue = 0;
            textSpeedSlider.maxValue = GameSettings.textSpeedOptions.Length - 1;
            textSpeedSlider.wholeNumbers = true;

            textSpeedSlider.value = Array.IndexOf(GameSettings.textSpeedOptions, textSpeed);

            textSpeedSlider.onValueChanged.RemoveAllListeners();
            textSpeedSlider.onValueChanged.AddListener(SetTextSpeedSliderValue);

            SetTextSpeedSliderValue(textSpeedSlider.value);

            #endregion

            musicVolumeSlider.onValueChanged.AddListener(v => SetMusicVolumeValue(v));
            musicVolumeSlider.value = GameSettings.singleton.musicVolume;

            sfxVolumeSlider.onValueChanged.AddListener(v => SetSFXVolumeValue(v));
            sfxVolumeSlider.value = GameSettings.singleton.sfxVolume;

        }

        public void SetTextSpeedSliderValue(float value)
        {

            int index = Mathf.FloorToInt(value);

            GameSettings.TextSpeed textSpeed = GameSettings.textSpeedOptions[index];

            textSpeedNameText.text = textSpeed.name;

            GameSettings.singleton.textSpeed = textSpeed;

        }

        public void SetMusicVolumeValue(float value)
        {

            GameSettings.singleton.musicVolume = value;
            MusicSourceController.singleton.SetVolume(value);

        }

        public void SetSFXVolumeValue(float value)
        {

            GameSettings.singleton.sfxVolume = value;

        }

    }
}