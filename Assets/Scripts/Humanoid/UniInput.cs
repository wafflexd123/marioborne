using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputType { OnPress, OnHold, OnRelease }
public enum InputAxes { Attack, Drop, Throw }

public class UniInput
{
    List<InputAxis> inputAxes = new List<InputAxis>();

    public UniInput(MonoBehaviour monoBehaviour)
    {
        foreach (string name in Enum.GetNames(typeof(InputAxes))) inputAxes.Add(new InputAxis(name, monoBehaviour));
    }

    public InputAction AddListener(string axis, InputType type, Action<float> action)
    {
        return new InputAction(FindAxis(axis), type, action);
    }

    /// <summary>
    /// Presses axis with a direction of 1 and holds for 1 frame 
    ///  --- Make sure the axis is added to the InputAxes enum (in this class)!!!
    /// </summary>
    /// <param name="axis">Name of axis</param>
    public void Press(string axis)
    {
        FindAxis(axis).Press();
    }

    /// /// <summary>
    /// Presses this button with a direction of [value], and holds it while [hold] is true.
    ///  --- Make sure the axis is added to the InputAxes enum (in this class)!!!
    /// </summary>
    /// <param name="axis">Name of axis</param>
    /// <param name="value">Direction of axis</param>
    /// <param name="hold">While this func returns true, the button is held down</param>
    public void Press(string axis, Func<float> value, Func<bool> hold)
    {
        InputAxis button = FindAxis(axis);
        if (button == null) Debug.LogWarning($"Could not find axis {axis}, make sure it is in the InputAxes enum and isn't misspelt");
        else button.Press(value, hold);
    }

    InputAxis FindAxis(string axis)
    {
        return inputAxes.Find((InputAxis match) => match.axis == axis);
    }

    public class InputAction
    {
        public readonly InputAxis axis;
        readonly InputType type;
        readonly Action<float> action;

        public InputAction(InputAxis axis, InputType type, Action<float> action)
        {
            this.axis = axis;
            this.type = type;
            this.action = action;
            axis.actions[(int)type].Add(action);
        }

        public void RemoveListener()
        {
            if (!axis.actions[(int)type].Remove(action)) Debug.LogWarning("Could not remove input listener!");
        }
    }

    public class InputAxis
    {
        public readonly string axis;
        public readonly List<List<Action<float>>> actions = new List<List<Action<float>>>();
        public float direction;
        readonly MonoBehaviour monoBehaviour;
        Coroutine crtHold, crtEndPress;

        public InputAxis(string axis, MonoBehaviour monoBehaviour)
        {
            this.axis = axis;
            this.monoBehaviour = monoBehaviour;
            for (int i = 0; i < 3; i++) actions.Add(new List<Action<float>>());//add OnPress, OnHold and OnRelease lists
        }

        /// <summary>
        /// Presses axis with a direction of 1 and holds for 1 frame
        /// </summary>
        public void Press()
        {
            direction = 1f;
            foreach (Action<float> item in new List<Action<float>>(actions[(int)InputType.OnPress])) item(1);
            foreach (Action<float> item in new List<Action<float>>(actions[(int)InputType.OnHold])) item(1);
            foreach (Action<float> item in new List<Action<float>>(actions[(int)InputType.OnRelease])) item(1);

            if (crtEndPress != null) monoBehaviour.StopCoroutine(crtEndPress);
            crtEndPress = monoBehaviour.StartCoroutine(EndPress());
            IEnumerator EndPress()
            {
                yield return null;
                direction = 0f;
                crtEndPress = null;
            }
        }

        /// <summary>
        /// Presses this button with a direction of [value], and holds it while [hold] is true.
        /// Note: every OnHold listener will be called at least once.
        /// </summary>
        /// <param name="value">Direction of axis</param>
        /// <param name="hold">While this func returns true, the button is held down</param>
        public void Press(Func<float> value, Func<bool> hold)
        {
            if (crtHold != null) monoBehaviour.StopCoroutine(crtHold);
            crtHold = monoBehaviour.StartCoroutine(Hold());
            IEnumerator Hold()
            {
                direction = value();
                foreach (Action<float> item in new List<Action<float>>(actions[(int)InputType.OnPress])) item(value());
                do
                {
                    foreach (Action<float> item in new List<Action<float>>(actions[(int)InputType.OnHold])) item(value());
                    direction = value();
                    yield return null;
                } while (hold());
                foreach (Action<float> item in new List<Action<float>>(actions[(int)InputType.OnRelease])) item(value());
                direction = 0f;
                crtHold = null;
            }
        }
    }
}
