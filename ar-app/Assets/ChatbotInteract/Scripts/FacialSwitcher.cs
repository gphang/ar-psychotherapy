using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FacialSwitcher : MonoBehaviour {
    
    private GameObject avatar;
    private SkinnedMeshRenderer skin;
    private Dictionary<int, float> currentWeights = new Dictionary<int, float>();
    private Coroutine transition;

    [SerializeField] private float transitionSpeed = 1.0f;

    // Emotion states from classifier: fear, love, instability, disgust, disappointment, shame, anger, jealous, sadness, envy, joy, guilt
    // NOTE: future direction: degree of emotion?

    // [SerializeField] private FacialExpression idle;
    // [SerializeField] private FacialExpression happy;
    // [SerializeField] private FacialExpression sad;
    // [SerializeField] private FacialExpression angry;
    // [SerializeField] private FacialExpression fearful;
    // [SerializeField] private FacialExpression disgusted;
    // [SerializeField] private FacialExpression surprise;


    [SerializeField] private FacialExpression idle;
    [SerializeField] private FacialExpression fear;
    [SerializeField] private FacialExpression love;
    [SerializeField] private FacialExpression instability;
    [SerializeField] private FacialExpression disgust;
    [SerializeField] private FacialExpression disappointment;
    [SerializeField] private FacialExpression shame;
    [SerializeField] private FacialExpression anger;
    [SerializeField] private FacialExpression jealous;
    [SerializeField] private FacialExpression sadness;
    [SerializeField] private FacialExpression envy;
    [SerializeField] private FacialExpression joy;
    [SerializeField] private FacialExpression guilt;
    
    public GameObject Avatar {
        private get => avatar;
        set {
            avatar = value;
            skin = avatar.transform.Find("AvatarHead").gameObject.GetComponent<SkinnedMeshRenderer>();
        }
    }

    private Coroutine Transition {
        get => transition;
        set {
            if (transition == null) transition = value;
            else {
                StopCoroutine(transition);
                ResetFace();
                transition = value;
            }
        }
    }
    
    public void ResetFace() {
        if (!skin) return;
        
        int shapeCount = skin.sharedMesh.blendShapeCount;
        for (int i = 0; i < shapeCount; i++) {
            skin.SetBlendShapeWeight(i, 0f);
        }
    }

    // public void SetIdle() {
    //     if (!skin) return;
    //     Transition = StartCoroutine(TransitionExpression(idle));
    // }
    
    // public void SetHappy() {
    //     if (!skin) return;
    //     Transition = StartCoroutine(IdleThenTransitionExpression(happy));
        
    // }

    // public void SetSad() {
    //     if (!skin) return;
    //     Transition = StartCoroutine(IdleThenTransitionExpression(sad));
    // }

    // public void SetAngry() {
    //     if (!skin) return;
    //     Transition = StartCoroutine(IdleThenTransitionExpression(angry));
    // }
    
    // public void SetFearful() {
    //     if (!skin) return;
    //     Transition = StartCoroutine(IdleThenTransitionExpression(fearful));
    // }
    
    // public void SetDisgusted() {
    //     if (!skin) return;
    //     Transition = StartCoroutine(IdleThenTransitionExpression(disgusted));
    // }
    
    // public void SetSurprised() {
    //     if (!skin) return;
    //     Transition = StartCoroutine(IdleThenTransitionExpression(surprise));
    // }



    // TODO: temp animations, need to find better expressions for each emotion class

    public void SetIdle() {
        if (!skin) return;
        Transition = StartCoroutine(TransitionExpression(idle));
    }
    
    public void SetFear() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(fear));
        
    }

    public void SetLove() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(love));
    }

    public void SetInstability() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(instability));
    }
    
    public void SetDisgust() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(disgust));
    }
    
    public void SetDisappointment() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(disappointment));
    }
    
    public void SetShame() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(shame));
    }

     public void SetAnger() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(anger));
        
    }

    public void SetJealous() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(jealous));
    }

    public void SetSadness() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(sadness));
    }
    
    public void SetEnvy() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(envy));
    }
    
    public void SetJoy() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(joy));
    }
    
    public void SetGuilt() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(guilt));
    }


    #region Internal

    private void GetCurrentWeights() {
        if (!skin) return;
        
        for (int i = 0; i < skin.sharedMesh.blendShapeCount; i++)
        {
            currentWeights[i] = skin.GetBlendShapeWeight(i);
        }
    }

    private IEnumerator TransitionExpression(FacialExpression exp) {
        
        GetCurrentWeights();
        
        float progress = 0.0f;
        Dictionary<int, float> targetWeights = new Dictionary<int, float>(currentWeights);

        foreach (BlendShapeWeight weight in exp.BlendShapeWeights) {
            targetWeights[weight.index] = weight.weight;
        }
        
        while (progress < 1.0f) {
            progress += Time.deltaTime * transitionSpeed;
            foreach (KeyValuePair<int,float> targetWeight in targetWeights) {
                int index = targetWeight.Key;
                float currentWeight = Mathf.Lerp(currentWeights[index], targetWeight.Value, progress);
                skin.SetBlendShapeWeight(index, currentWeight);
            }
            yield return null;
        }
    }

    private IEnumerator IdleThenTransitionExpression(FacialExpression exp) {
        yield return TransitionExpression(idle);
        yield return TransitionExpression(exp);
    }

    #endregion
}
