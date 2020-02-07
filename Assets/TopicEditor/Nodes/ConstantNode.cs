using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class ConstantNode : Node {
    public float value;
    [Output] public float constant;

    public override object GetValue(NodePort port)
    {
        constant = GetInputValue("value", this.value);
        return constant;
    }
}