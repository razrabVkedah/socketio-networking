using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ContentVerticalSize : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform[] messages;

    private async void Start()
    {
        await Task.Delay(1901);
        UpdateSize();
    }

    public void UpdateSize()
    {
        var height = 0.0f;
        foreach (var message in messages)
        {
            height += message.rect.height;
            Debug.Log(message.rect);
        }

        content.sizeDelta = new Vector2(content.sizeDelta.x, height);
    }
}
