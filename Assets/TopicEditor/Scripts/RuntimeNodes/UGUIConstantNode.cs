using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;

public class UGUIConstantNode : UGUIMathBaseNode
{
	public InputField value;
	private ConstantNode constantNode;

	public override void Start()
	{
		base.Start();
		constantNode = node as ConstantNode;

		value.onValueChanged.AddListener(OnChangeValue);
		UpdateGUI();
	}

	private void OnChangeValue(string val)
	{
		constantNode.value = float.Parse(value.text);
	}
}