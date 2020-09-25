using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectAF.UI
{
    public class InputPopup : ConfirmPopup
    {
        public class StringEvent : UnityEvent<string>
        {
        }

        [SerializeField]
        private InputField inputField = null;

        public new Action<string> OnSubmit;

        protected override void Awake()
        {
            submitButton.onClick.AddListener(() =>
            {
                OnSubmit?.Invoke(inputField.text);
                Hide();
            });
            cancelButton.onClick.AddListener(() =>
            {
                OnCancel?.Invoke();
                Hide();
            });
        }

        public void Show(Action<string> onSubmit, Action onCancel)
        {
            gameObject.SetActive(true);
            inputField.text = null;
            OnSubmit = onSubmit;
            OnCancel = onCancel;
        }
    }
}
