using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this is the worst guy I have ever made. 
/// </summary>
public class CoroutineHelper : MonoBehaviour
{
    //private HashSet<Coroutine> coroutines;
    //private Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>();
    //private Dictionary<string, IEnumerator> coroutineFunctions = new Dictionary<string, IEnumerator>();
    //private Dictionary<string, System.Func<IEnumerator>> coroutineFunctions = new Dictionary<string, System.Func<IEnumerator>>();
    private Dictionary<string, CorouCollection> routines = new Dictionary<string, CorouCollection>();

    //public void StartOrAddCoroutine(string name, System.Func<IEnumerator> coroutine)
    public void StartOrAddCoroutine(string name, IEnumerator coroutine)
    {
        if (routines.ContainsKey(name))
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
    public void AddCoroutine(string name, IEnumerator coroutine_)
    {
        CorouCollection newCollection = new CorouCollection
        {
            running = false,
            coroutine = null,
            coroutineFunction = coroutine_,
        };
        if (routines.ContainsKey(name))
        {
            if (routines[name].coroutine != null)
                StopCoroutine(routines[name].coroutine);
            routines[name] = newCollection;
        }
        else
            routines.Add(name, newCollection);
    }

    public void StartKnownCoroutine(string name)
    {
        if (routines.TryGetValue(name, out CorouCollection collection)) 
        {
            if (collection.running) return;
            collection.running = true;
            collection.coroutine = StartCoroutine(collection.coroutineFunction);
        }
        else
        {
            Debug.LogWarning($"Coroutine Helper asked to start a coroutine it does not know. Asked to start: {name}");
        }
    }

    public void CancelCoroutine(string name)
    {
        if (routines.TryGetValue(name, out CorouCollection collection))
        {
            if (!collection.running) return;
            collection.running = false;
            StopCoroutine(collection.coroutine);
        }
        else
        {
            Debug.LogWarning($"Coroutine Helper asked to stop a coroutine it does not know. Asked to stop: {name}");
        }
    }

    public void StartTimer(string name, float duration, ExternalControlTransition externalControlTransition)
    {
        var timer = StartCoroutine(StandardTimer(duration, externalControlTransition));
        CorouCollection newCollection = new CorouCollection
        {
            running = true,
            coroutine = timer,
            coroutineFunction = null,
        };

        if (routines.ContainsKey(name))
        {
            StopCoroutine(routines[name].coroutine);
            routines[name] = newCollection;
        }
        else
            routines.Add(name, newCollection);
    }

    public void CancelTimer(string name)
    {
        if (routines.TryGetValue(name, out CorouCollection collection))
        {
            if (!collection.running) return;
            collection.running = false;
            StopCoroutine(collection.coroutine);
        }
        else
            Debug.LogWarning($"Coroutine Helper asked to stop a coroutine it does not know. Asked to stop: {name}");
    }

    private struct CorouCollection
    {
        public Coroutine coroutine;
        public IEnumerator coroutineFunction;
        public bool running;
    }

    private IEnumerator StandardTimer(float duration, ExternalControlTransition externalControlTransition)
    {
        yield return new WaitForSeconds(duration);
        externalControlTransition.trigger = true;
    }
}