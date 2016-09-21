using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class LookingAt : MonoBehaviour {

    public Transform _lookAtObject;
    public float _lookAtWeight = 1.0f;

    private Animator _targetAnimator;

    // Use this for initialization
    void Start() {
        _targetAnimator = this.GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex) {
        if (_targetAnimator == null) {
            return;
        }
        _targetAnimator.SetLookAtWeight(_lookAtWeight, 0.3f, 0.6f, 1.0f, 0.25f);
        if (_lookAtObject != null) {
            _targetAnimator.SetLookAtPosition(_lookAtObject.position);
        }
    }


    // Update is called once per frame
    void Update() {

    }
}
