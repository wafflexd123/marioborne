using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GrenadeObject : MonoBehaviour
{
    [Header("In Hand")]
    [SerializeField] private float rotationSpeed;
    private Quaternion localRot;
    private Quaternion frameRotation;
    [SerializeField] private float amplitude;
    [SerializeField] private float period;
    private Vector3 standardOffset = Vector3.zero;
    //public static bool IsInHand = true;

    [Header("Shader")]
    [SerializeField] private float handGridSize = 64f;
    [SerializeField] private float airGridSize  = 32f;
    private Material mat;

    private Rigidbody rb;

    private void Awake()
    {
        localRot = transform.localRotation;
        standardOffset = transform.position - transform.parent.position;
    }
    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        //lazyFollower = GetComponent<LazyFollower>();
        //if (lazyFollower != null)
        //    lazyFollower.offsetFunc = new LazyFollower.offsetDelegate(FrameOffset);
        //else Debug.LogWarning("Grenade could not find lazy follower, please be upset");
    }

    public void Thrown()
    {
        mat.SetFloat("_GridSize", airGridSize);
        gameObject.SetActive(false);
    }

    public void Reappear(float startTime)
    {
        mat.SetFloat("_GridSize", handGridSize);
        mat.SetFloat("_Completness", 0.5f); // temporary debug
        // settings for regenerating
    }

    private Vector3 FrameOffset()
    {
        return Vector3.up * (amplitude * Mathf.Sin(UnityEngine.Time.time * period));
    }

    void Update()
    {
        frameRotation = Quaternion.Euler(0f, rotationSpeed * Time.deltaTime, 0f);
        localRot *= frameRotation;
        transform.localRotation = localRot;

        transform.localPosition = standardOffset + FrameOffset();
    }

    //private void OnEnable() { if (lazyFollower != null) lazyFollower.enabled = true; }
    //private void OnDisable() { if (lazyFollower != null) lazyFollower.enabled = false; }
}
