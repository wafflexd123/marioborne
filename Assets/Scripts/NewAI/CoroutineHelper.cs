using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this is the worst guy I have ever made. 
/// </summary>
public class CoroutineHelper : MonoBehaviour
{
    [Header("I am a stupid bastard required for other code to work, please ignore me in inspector.")]
    //private HashSet<Coroutine> coroutines;
    private Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>();
    private Dictionary<string, IEnumerator> coroutineFunctions = new Dictionary<string, IEnumerator>();
    //private Dictionary<string, System.Func<IEnumerator>> coroutineFunctions = new Dictionary<string, System.Func<IEnumerator>>();

    //public void StartOrAddCoroutine(string name, System.Func<IEnumerator> coroutine)
    public void StartOrAddCoroutine(string name, IEnumerator coroutine)
    {
        // I am sure this could be optimised, it looks smelly. 
        if (coroutineFunctions.ContainsKey(name))
        {
            StartKnownCoroutine(name);
        }
        else
        {
            AddCoroutine(name, coroutine);
            StartKnownCoroutine(name);
        }
    }

    //public void AddCoroutine(string name, System.Func<IEnumerator> coroutine)
    public void AddCoroutine(string name, IEnumerator coroutine)
    {
        coroutineFunctions.Add(name, coroutine);
    }

    public void StartKnownCoroutine(string name)
    {
        //if (coroutineFunctions.TryGetValue(name, out System.Func<IEnumerator> func))
        if (coroutineFunctions.TryGetValue(name, out IEnumerator func))
        {
            coroutines.Add(name, StartCoroutine(func));
        }
        else
        {
            Debug.LogWarning($"Coroutine Helper asked to start a coroutine it does not know. Asked to start: {name}");
        }
    }

    public void CancelCoroutine(string name)
    {
        if (coroutines.TryGetValue(name, out Coroutine coroutine))
        {
            // calling this every frame is probably really inefficient but I don't know how else to do this as of this moment
            StopCoroutine(coroutine);
        }
        else
        {
            Debug.LogWarning($"Coroutine Helper asked to stop a coroutine it does not know. Asked to stop: {name}");
        }
    }
}