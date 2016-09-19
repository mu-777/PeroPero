using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;
using WebSocketSharp.Net;

//Ref http://qiita.com/oishihiroaki/items/bb2977c72052f5dd5bd9


[System.Serializable]
public class SmartphoneSensorData {
    public float pitch { get; private set; }
    public float azimuth { get; private set; }
    public float roll { get; private set; }

    private JSONObject _receivedJson = new JSONObject();

    public SmartphoneSensorData() {

    }

    public SmartphoneSensorData UpdateData(string jsonData) {
        _receivedJson.Clear();
        _receivedJson.Add(jsonData);
        SetData();
        return this;
    }

    private void SetData() {
        pitch = (float)_receivedJson.GetField("pitch").n;
        azimuth = (float)_receivedJson.GetField("azimuth").n;
        roll = (float)_receivedJson.GetField("roll").n;
    }
}


[System.Serializable]
public class UpdateSmartphoneSensorData : UnityEvent<SmartphoneSensorData> { }

public class ServerForSmartphone : MonoBehaviour {

    public string _wsAddress = "ws://0.0.0.0:3000";
    public UpdateSmartphoneSensorData updateSmartphoneSensorData;


    private WebSocket _ws;
    private SmartphoneSensorData smartphoneSensorData = new SmartphoneSensorData();


    void Awake() {
        Connect();
    }

    void Start() {
    }

    void Update() {
    }

    private void Connect() {
        _ws = new WebSocket(_wsAddress);
        _ws.OnOpen += this.OnOpen;
        _ws.OnMessage += this.OnMessage;
        _ws.OnError += this.OnError;
        _ws.OnClose += this.OnClose;
        _ws.Connect();
    }

    private void OnOpen(System.Object sender, System.EventArgs args) {
        Debug.Log("WebSocket Open");
    }

    private void OnMessage(System.Object sender, MessageEventArgs args) {
        updateSmartphoneSensorData.Invoke(smartphoneSensorData.UpdateData(args.Data));
    }

    private void OnError(System.Object sender, ErrorEventArgs args) {
        Debug.Log("WebSocket Error Message: " + args.Message);
    }

    private void OnClose(System.Object sender, CloseEventArgs args) {
        Debug.Log("WebSocket Close");
    }

    public void Disconnect() {
        _ws.Close();
        _ws = null;
    }

    public void Send(string message) {
        _ws.Send(message);
    }

}
