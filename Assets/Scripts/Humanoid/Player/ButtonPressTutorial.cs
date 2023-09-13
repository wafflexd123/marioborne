using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonPressTutorial : MonoBehaviour
{
	TextMeshProUGUI text;

	private void Start()
	{
		text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		gameObject.SetActive(false);
	}

	public void Display(string key)
	{
		gameObject.SetActive(true);
		text.text = key;	
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}
}
