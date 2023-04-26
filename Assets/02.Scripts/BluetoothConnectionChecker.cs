using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using InTheHand.Net.Bluetooth;
using InTheHand.Bluetooth;
using System.Linq;

public class BluetoothConnectionChecker : MonoBehaviour
{
    public TextMeshProUGUI testText;

    void Start()
    {

    }

    private void Update()
    {
#if UNITY_ANDROID
        CheckBluetoothConnectionAndroid();
#elif UNITY_STANDALONE_WIN
        //CheckBluetoothConnectionWindows();
#endif
    }

    void CheckBluetoothConnectionAndroid()
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (var bluetoothHelper = new AndroidJavaObject("com.example.bluetoothhelper.BluetoothHelper", currentActivity))
                {
                    bool isConnected = bluetoothHelper.Call<bool>("isBluetoothConnected");
                    testText.text = isConnected.ToString();
                    Debug.Log("Bluetooth 연결 상태: " + isConnected);
                }
            }
        }
    }

    //void CheckBluetoothConnectionWindows()
    //{
    //    bool isConnected = false;
    //    string targetDeviceName = "QCY AilyPods";

    //    // Bluetooth 클라이언트 생성
    //    BluetoothDevice bluetoothDevice = new BluetoothDevice();
        
    //}

}