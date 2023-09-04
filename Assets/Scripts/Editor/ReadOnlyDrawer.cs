using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
	{
		switch (prop.propertyType)
		{
			case SerializedPropertyType.Integer:
				EditorGUI.LabelField(position, label.text, prop.intValue.ToString());
				break;
			case SerializedPropertyType.Boolean:
				EditorGUI.LabelField(position, label.text, prop.boolValue.ToString());
				break;
			case SerializedPropertyType.Float:
				EditorGUI.LabelField(position, label.text, prop.floatValue.ToString("0.00"));
				break;
			case SerializedPropertyType.String:
				EditorGUI.LabelField(position, label.text, prop.stringValue);
				break;
			case SerializedPropertyType.Color:
				EditorGUI.LabelField(position, label.text, prop.colorValue.ToString());
				break;
			case SerializedPropertyType.ObjectReference:
				EditorGUI.LabelField(position, label.text, prop.objectReferenceValue != null ? prop.objectReferenceValue.GetType().Name : "none");
				break;
			case SerializedPropertyType.Vector2:
				break;
			case SerializedPropertyType.Vector3:
				break;
			case SerializedPropertyType.Vector4:
				break;
			case SerializedPropertyType.Gradient:
				break;
			case SerializedPropertyType.Quaternion:
				break;
			case SerializedPropertyType.Vector2Int:
				break;
			case SerializedPropertyType.Vector3Int:
				break;
			default:
				EditorGUI.LabelField(position, label.text, "([readonly] not supported)");
				break;
		}
	}
}