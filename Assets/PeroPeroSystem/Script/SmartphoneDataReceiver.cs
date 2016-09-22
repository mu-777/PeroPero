using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;
using WebSocketSharp.Net;


//Ref http://qiita.com/oishihiroaki/items/bb2977c72052f5dd5bd9


[System.Serializable]
public class SmartphoneSensorData {

    [System.Serializable]
    public class Orientation {
        public float pitch;
        public float azimuth;
        public float roll;
    }

    public Orientation ori;

    public SmartphoneSensorData() {

    }
    public static SmartphoneSensorData FromJSON(string jsonString) {
        return JsonUtility.FromJson<SmartphoneSensorData>(jsonString);
    }

}

[System.Serializable]
public class UpdateSmartphoneSensorData : UnityEvent<SmartphoneSensorData> { }

public class SmartphoneDataReceiver : MonoBehaviour {

    // http://tips.hecomi.com/entry/2014/04/15/011255
    public class NodejsServerKicker {

        private static readonly string _nodeBinPath = System.IO.Path.Combine(Application.streamingAssetsPath, "node.exe");
        private System.Diagnostics.Process _process;
        private string _scriptPath = System.IO.Path.Combine(Application.streamingAssetsPath, "wsserver.js");
        private string _ip, _port;

        public bool isRunning { get; private set; }

        public NodejsServerKicker(string ip, string port) {
            isRunning = false;
            _ip = ip;
            _port = port;
        }

        public void Run() {
            _process = new System.Diagnostics.Process();
            //_process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            _process.StartInfo.FileName = _nodeBinPath;
            _process.StartInfo.Arguments = _scriptPath + " " + _ip + " " + _port;
            _process.EnableRaisingEvents = true;
            _process.Exited += OnExit;
            _process.Start();
            isRunning = true;
        }
        public void OnExit(object sender, System.EventArgs e) {
            isRunning = false;
            if (_process.ExitCode != 0) {
                Debug.LogError("Error! Exit Code: " + _process.ExitCode);
            }
        }

        public void KillProcess() {
            if (_process != null && !_process.HasExited) {
                _process.Kill();
                _process.Dispose();
            }
        }
    }

    public string _ip = "localhost";
    public string _port = "3000";
    private Uri _uri;
    public UpdateSmartphoneSensorData updateSmartphoneSensorData;

    private WebSocket _ws;
    private NodejsServerKicker _serverKicker;
    private SmartphoneSensorData _data;
    private bool _isReceived = false;

    void Start() {
        _uri = new Uri("ws://" + _ip + ":" + _port);
        _ws = new WebSocket(_uri.ToString());
        _data = new SmartphoneSensorData();
        _serverKicker = new NodejsServerKicker(_ip, _port);
        _serverKicker.Run();
        Connect();
    }

    private void Connect() {
        _ws.OnOpen += OnWSOpen;
        _ws.OnMessage += OnWSMessage;
        _ws.OnError += OnWSError;
        _ws.OnClose += OnWSClose;

        _ws.Connect();
    }

    private void OnWSOpen(object sender, EventArgs e) {
        print("WebSocket Open");
    }

    private void OnWSMessage(object sender, MessageEventArgs e) {
        _data = SmartphoneSensorData.FromJSON(e.Data);
        _isReceived = true;
    }

    private void OnWSError(object sender, ErrorEventArgs e) {
        print("WebSocket Error Message: " + e.Message);
    }

    private void OnWSClose(object sender, CloseEventArgs e) {
        print("WebSocket Close");
    }


    void Update() {
        if (_isReceived) {
            updateSmartphoneSensorData.Invoke(_data);
            _isReceived = false;
        }
    }

    void OnDestroy() {
        _ws.Close();
        _ws = null;
        _serverKicker.KillProcess();
    }
}

