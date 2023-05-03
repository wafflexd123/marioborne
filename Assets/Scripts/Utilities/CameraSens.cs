using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraSens : MonoBehaviour
{
    public Slider slider;
    public PlayerCamera playerCamera;

    void Start()
    {
        // 设置 slider 控件的默认值为 playerCamera 的 sensitivity 变量
        
        slider.value = playerCamera.sensitivity;

        // 为 slider 的 OnValueChanged 事件添加处理程序
        slider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    public void OnSensitivityChanged(float value)
    {
        // 更改 playerCamera 的 sensitivity 变量
        playerCamera.sensitivity = value;
    }

    
}

