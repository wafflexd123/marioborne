using System.Collections;
using UnityEngine;

public class Rotater : MonoBehaviour
{
    public Vector3 rotateAmount;
    public float timeToRotate;
    public bool rotateOnAwake;

    private IEnumerator Start()
    {
        if (rotateOnAwake)
        {
            yield return null;
            StartRotation();
        }
    }

    public void StartRotation()
    {
        Vector3 start = transform.eulerAngles;
        Vector3 end = rotateAmount + start;
        float timer = 0, percent;
        StartCoroutine(Rotate());
        IEnumerator Rotate()
        {
            do
            {
                timer += Time.fixedDeltaTime;
                percent = timer / timeToRotate;
                transform.eulerAngles = new Vector3(Mathf.Lerp(start.x, end.x, percent), Mathf.Lerp(start.y, end.y, percent), Mathf.Lerp(start.z, end.z, percent));
                yield return new WaitForFixedUpdate();
            } while (percent < 1);
        }
    }
}
