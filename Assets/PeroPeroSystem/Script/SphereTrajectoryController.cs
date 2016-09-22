using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Ref http://qiita.com/oishihiroaki/items/bb2977c72052f5dd5bd9

public class SphereTrajectoryController : MonoBehaviour {

    public bool IsKeyBoardSimulation = true;
    public GameObject _center;
    public float _radius;

    private float _pitch = 0f;
    private float _azimuth = 0f;
    private float _pitchVec = 50f;
    private float _azimuthVec = 50f;

    private GameObject _centerClone;

    void Start() {
        _radius = System.Math.Abs(this.transform.position.z);
        _centerClone = new GameObject(this.name + "_Center");
        UpdateCenterState();
    }

    void Update() {
        UpdateKeyBoardSim(IsKeyBoardSimulation);
        UpdateCenterState();
        UpdateCameraPos(_pitch, _azimuth);
    }

    private void UpdateCenterState() {
        _centerClone.transform.position = _center.transform.position;
        _centerClone.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void UpdateCameraPos(float pitch, float azimuth) {
        var localPos = new Vector3(-_radius * (float)System.Math.Cos(pitch * Mathf.Deg2Rad) * (float)System.Math.Sin(azimuth * Mathf.Deg2Rad),
                                   _radius * (float)System.Math.Sin(pitch * Mathf.Deg2Rad),
                                   _radius * (float)System.Math.Cos(pitch * Mathf.Deg2Rad) * (float)System.Math.Cos(azimuth * Mathf.Deg2Rad));

        this.transform.position = _center.transform.position + localPos;

        this.transform.LookAt(_center.transform.position);
    }

    public void UpdateSmartphoneSensorData(SmartphoneSensorData sensorData) {
        print(sensorData.ori.roll);
        _pitch = MyUtils.lowpassFilter(sensorData.ori.roll + 90f, _pitch, 0.9f);
        _azimuth = MyUtils.lowpassFilter(sensorData.ori.azimuth, _azimuth, 0.9f);
    }

    public void UpdateKeyBoardSim(bool isActive) {
        if (!isActive) {
            return;
        }

        if (Input.GetKey(KeyCode.UpArrow)) {
            _pitch = Mathf.Clamp(_pitch + _pitchVec * Time.deltaTime, -89f, 89f);

        } else if (Input.GetKey(KeyCode.DownArrow)) {
            _pitch = Mathf.Clamp(_pitch - _pitchVec * Time.deltaTime, -89f, 89f);
        }

        if (Input.GetKey(KeyCode.RightArrow)) {
            _azimuth = Mathf.Clamp(_azimuth + _azimuthVec * Time.deltaTime, -Mathf.Infinity, Mathf.Infinity);
        } else if (Input.GetKey(KeyCode.LeftArrow)) {
            _azimuth = Mathf.Clamp(_azimuth - _azimuthVec * Time.deltaTime, -Mathf.Infinity, Mathf.Infinity);
        }
    }

}
