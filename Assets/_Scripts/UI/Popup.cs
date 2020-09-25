using System;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{
    public Action OnClose = null;

    [SerializeField]
    private Text titleText = null;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Show(string title)
    {
        titleText.text = title;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        OnClose?.Invoke();
    }
}
