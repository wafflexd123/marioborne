using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JMEvents : MonoBehaviour
{
    public static JMEvents Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }

    public event Action OnPlayerDeflect;
    public void PlayerDeflect() { if (OnPlayerDeflect != null) OnPlayerDeflect(); }
}
//3:45
