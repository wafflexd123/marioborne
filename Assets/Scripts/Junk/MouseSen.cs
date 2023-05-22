using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseSen : MonoBehaviour
{
    public Slider slider;
    public PlayerCamera playerCamera;

    void Start()
    {
       
        slider.value = playerCamera.sensitivity;
        slider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    public void OnSensitivityChanged(float value)
    {
        playerCamera.sensitivity = value;
    }
}