using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPanelController : MonoBehaviour
{
    public delegate void StartPanelDelegate();
    public event StartPanelDelegate OnStartButtonClick;

    public void OnClickStartButton()
    {
        OnStartButtonClick?.Invoke();
    }
}
