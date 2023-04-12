using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static void CheckKey()
    {
        // 모든 키를 검사
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            // 키가 눌렸을 때
            if (Input.GetKey(keyCode))
            {
                // 키의 이름 출력
                Debug.Log("Key Pressed: " + keyCode.ToString());
            }
        }
    }
}
