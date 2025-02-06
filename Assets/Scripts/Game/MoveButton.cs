using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveButton : MonoBehaviour
{
    public delegate void MoveButtonDelegate();
    public event MoveButtonDelegate OnMoveButtonDown;
    
    private bool _isDown;

    private void Update()
    {
        if (_isDown)
        {
            OnMoveButtonDown?.Invoke();
        }
    }

    public void ButtonDown()
    {
        _isDown = true;
    }

    public void ButtonUp()
    {
        _isDown = false;
    }
}