using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StartUp.ChoosePlayerData
{
    public abstract class GenericImageOptionsChoiceController<T> : ChoosePlayerDataController
    {

        public GameObject defaultEventSystemSelection;

        public GenericImageOptionController<T>[] options;

        protected bool optionSelected = false;
        protected T selectedOptionId = default;

        protected override IEnumerator MainCoroutine()
        {

            if (defaultEventSystemSelection != null)
                EventSystem.current.SetSelectedGameObject(defaultEventSystemSelection);

            optionSelected = false;

            foreach (GenericImageOptionController<T> controller in options)
            {

                T optionId = controller.instanceId;

                controller.OnClick.AddListener(() =>
                {
                    optionSelected = true;
                    selectedOptionId = optionId;
                });

            }

            yield return new WaitUntil(() => optionSelected);

            SetPlayerDataValue(selectedOptionId);

            CloseScene();

        }

        protected abstract void SetPlayerDataValue(T value);

    }
}