using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectAF.UI
{
    public class ConfirmPopup : MonoBehaviour
    {
        [SerializeField]
        protected Button submitButton = null;

        [SerializeField]
        protected Button cancelButton = null;

        public Action OnSubmit;

        public Action OnCancel;

        protected virtual void Awake()
        {
            submitButton.onClick.AddListener(() =>
            {
                OnSubmit?.Invoke();
                Hide();
            });
            cancelButton.onClick.AddListener(() =>
            {
                OnCancel?.Invoke();
                Hide();
            });
        }

        public void Show(Action onSubmit, Action onCancel)
        {
            gameObject.SetActive(true);
            OnSubmit = onSubmit;
            OnCancel = onCancel;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
