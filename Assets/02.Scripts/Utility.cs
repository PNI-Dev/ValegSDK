using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static void CheckKey()
    {
        // ��� Ű�� �˻�
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            // Ű�� ������ ��
            if (Input.GetKey(keyCode))
            {
                // Ű�� �̸� ���
                Debug.Log("Key Pressed: " + keyCode.ToString());
            }
        }
    }
}
