using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Lerper)), CanEditMultipleObjects]
public class LerperEditor : Editor
{
	SerializedProperty easingCurve, easingFunction, onFinish, onUndoFinish, loopType, useTransform, lerpPos, lerpRot, lerpSca, lerpToTransform, useLocal;
	Lerper lerper;

	void OnEnable()
	{
		lerper = (Lerper)target;
		easingCurve = serializedObject.FindProperty("easingCurve");
		easingFunction = serializedObject.FindProperty("easingFunction");
		loopType = serializedObject.FindProperty("loopType");
		onFinish = serializedObject.FindProperty("onFinish");
		onUndoFinish = serializedObject.FindProperty("onUndoFinish");
		useTransform = serializedObject.FindProperty("useTransform");
		lerpPos = serializedObject.FindProperty("lerpPos");
		lerpRot = serializedObject.FindProperty("lerpRot");
		lerpSca = serializedObject.FindProperty("lerpSca");
		lerpToTransform = serializedObject.FindProperty("lerpToTransform");
		useLocal = serializedObject.FindProperty("useLocal");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		DrawDefaultInspector();

		EditorGUILayout.PropertyField(easingFunction);
		if (lerper.easingFunction == EasingFunction.Enum.AnimationCurve)
		{
			EditorGUILayout.PropertyField(easingCurve);
		}

		EditorGUILayout.PropertyField(loopType);
		if (lerper.loopType == Lerper.LoopType.None)
		{
			EditorGUILayout.PropertyField(onFinish);
			EditorGUILayout.PropertyField(onUndoFinish);
		}

		EditorGUILayout.PropertyField(useTransform);
		if (lerper.useTransform)
		{
			EditorGUILayout.PropertyField(lerpToTransform);
		}
		else
		{
			EditorGUILayout.PropertyField(useLocal, new GUIContent("Local Position"));
			EditorGUILayout.PropertyField(lerpPos, new GUIContent("Lerp To Position"));
			EditorGUILayout.PropertyField(lerpRot, new GUIContent("Lerp To Rotation"));
			EditorGUILayout.PropertyField(lerpSca, new GUIContent("Lerp To Scale"));
		}

		serializedObject.ApplyModifiedProperties();
	}
}
