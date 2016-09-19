using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class PeroPeroStateSwither : MonoBehaviour {

    private enum PoseState {
        Idle = 0,
        Front = 1,
        LeftBack = 2,
        RightBack = 3
    }


    [System.Serializable]
    public class faceAnimations {
        public AnimationClip normal;
        public AnimationClip angryWeek;
        public AnimationClip angryStrong;
    }

    public Animator _targetAnimator;
    public GameObject _targetModel;
    public float _heightThreshold = -0.03f;
    public faceAnimations _faceAnimations;

    private Transform _referenceTransform;
    private string _currFaceClipName;
    private Vector3 _initLocalCamPos;
    // Use this for initialization
    void Start() {
        _referenceTransform = new GameObject().transform;
        _referenceTransform.position = _targetModel.transform.position;
        _referenceTransform.rotation = Quaternion.identity;
        _referenceTransform.parent = _targetModel.transform;

        _targetAnimator.SetLayerWeight(1, 1);
        _targetAnimator.CrossFade(_faceAnimations.normal.name, 0);
        _currFaceClipName = _faceAnimations.normal.name;

        _initLocalCamPos = _referenceTransform.InverseTransformPoint(Camera.main.transform.position);
    }

    // Update is called once per frame
    void Update() {
        var localCamPos = _referenceTransform.InverseTransformPoint(Camera.main.transform.position);
        UpdatePose(localCamPos);
        UpdateFace(localCamPos);
    }

    private void UpdateFace(Vector3 targetPos) {
        Func<string, float, bool> ChangeFace = delegate(string faceClipName, float transitionDulation) {
            if (faceClipName == _currFaceClipName) {
                return true;
            }
            _targetAnimator.CrossFade(faceClipName, transitionDulation);
            _currFaceClipName = faceClipName;
            return true;
        };

        if (targetPos.y < _heightThreshold + _initLocalCamPos.y) {
            ChangeFace(_faceAnimations.angryStrong.name, 0.4f);
        } else if (targetPos.y < _heightThreshold * 0.2 + _initLocalCamPos.y) {
            ChangeFace(_faceAnimations.angryWeek.name, 1f);
        } else {
            ChangeFace(_faceAnimations.normal.name, 1f);
        }
    }

    private void UpdatePose(Vector3 targetPos) {
        Func<PoseState, bool> SetPose = delegate(PoseState poseState) {
            _targetAnimator.SetInteger("PoseState",(int)poseState);
            return true;
        };

        if (targetPos.y < _heightThreshold + _initLocalCamPos.y) {
            if (targetPos.z >= 0) {
                SetPose(PoseState.Front);
            } else {
                if (targetPos.x >= 0) {
                    SetPose(PoseState.RightBack);
                } else {
                    SetPose(PoseState.LeftBack);
                }
            }
        } else {
            SetPose(PoseState.Idle);
            // Switch pose
        }
    }
}