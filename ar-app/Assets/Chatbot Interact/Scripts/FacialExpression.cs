using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlendShapeWeight {
    public int index;
    public float weight;

    public BlendShapeWeight(int index, float weight) {
        this.index = index;
        this.weight = weight;
    }
}

[CreateAssetMenu(fileName = "FacialExp", menuName = "SO/FacialExpression")]
public class FacialExpression : ScriptableObject {
    public List<BlendShapeWeight> BlendShapeWeights = new List<BlendShapeWeight>();
}