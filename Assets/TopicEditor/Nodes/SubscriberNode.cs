﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class SubscriberNode : Node {
    public float test;
    public string topic;
    [Output] public Vector3 vector;

    public override object GetValue(NodePort port)
    {
        vector.x = GetInputValue<float>("x", this.x);
        vector.y = GetInputValue<float>("y", this.y);
        vector.z = GetInputValue<float>("z", this.z);
        return vector;
    }
}