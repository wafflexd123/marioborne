using UnityEngine.Events;

public class ActionOnAwake : UnityEventHelper
{
    public UnityEvent awake, start;
    public bool deleteAfterStart = true;

    private void Awake()
    {
        awake.Invoke();
        if (deleteAfterStart && start.GetPersistentEventCount() == 0) Destroy(this);
    }

    private void Start()
    {
        start.Invoke();
        if (deleteAfterStart) Destroy(this);
    }
}
