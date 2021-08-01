using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Menus;

namespace StartUp.ChoosePlayerData
{
    public abstract class GenericImageOptionController<T> : MenuSelectableController
    {

        public UnityEvent OnClick = new UnityEvent();

        public Image image;
        public Button button;
        public T instanceId;

        protected override void Start()
        {

            base.Start();

            RefreshImage();

            button.onClick.AddListener(() => OnClick.Invoke());

        }

        public abstract void RefreshImage();

    }
}