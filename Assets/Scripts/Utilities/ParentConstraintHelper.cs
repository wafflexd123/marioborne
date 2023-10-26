using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ParentConstraintHelper : MonoBehaviour
{
    [SerializeField] private Vector3 pos;
    [SerializeField] private Vector3 rot;

    private ParentConstraint parentConstraint;
    void Start()
    {
        parentConstraint = GetComponent<ParentConstraint>();
        //var a = parentConstraint.translationOffsets;
        //for (int i = 0; i < a.Length; i++)
        //    print($"i: {i}, \tv: {a[i]}");
        parentConstraint.SetTranslationOffset(0, pos);
        parentConstraint.SetRotationOffset(0, rot);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
