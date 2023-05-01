using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraSen : MonoBehaviour
{
    public PlayerCamera playerCamera;
    public Slider slider;

    private void Start()
    {
        // 获取全局 Player Camera 对象
        playerCamera = GameObject.FindObjectOfType<PlayerCamera>();
        // 初始化 slider 的值
        slider.value = playerCamera.sensitivity;
        // 注册 slider 的监听事件
        slider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });
    }

    void OnSliderValueChanged()
    {
        // 设置 Player Camera 的 sensitivity 值
        playerCamera.sensitivity = slider.value;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
