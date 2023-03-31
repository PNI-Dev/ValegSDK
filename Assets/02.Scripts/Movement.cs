using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using InControl;
using System;

public class Movement : MonoBehaviour
{
    [Header("Only For Android Platform")]
    [Tooltip("�ε巯�� �̵� ����")]
    [Range(0.1f, 10f)]
    public float m_Interpolation = 1f;


    // �̵�, ȸ�� �Է°� ������
    float _horizontal;
    float _vertical;
    float _yaw;

    float currentH = 0;
    float currentV = 0;

    Vector3 direction;
    Vector3 movement;

    [Tooltip("���� �Ŀ� ����")]
    public float m_JumpPower = 5;
    bool isJump;
    float movementY;

    float smoothingvalue = 4f;
    float velocityvalue = 3f;
    float sidevelocityvalue = 2f;
    public TextMeshProUGUI currentInterPolation;
    public TextMeshProUGUI currentvelocity;
    public TextMeshProUGUI currentsidevelocity;

    [Header("Common Platform")]
    [Tooltip("�÷��̾� ��")]
    public GameObject player;
    [Tooltip("�÷��̾� �þ�")]
    public GameObject centerEye;

    CharacterController cc;

    [Space(15)]
    [Tooltip("CharacterController�̱� ������ �߷� ���� ���� �ʿ�")]
    public float m_Gravity = -9.8f;
    [Tooltip("�̵� �ӵ�")]
    public float m_Speed = 2f;
    [Tooltip("������ �ӵ�")]
    public float m_SideSpeed = 1.5f;
    [Tooltip("ȸ�� �ӵ�")]
    public float m_LerpSpeed = 3f;

    [Space(15)]
    [Tooltip("���� �ʱⰪ")]
    public float m_OriginAngle = 0f;

    [Tooltip("[���� ������ ��� ����] \n���� �ӽ��� ���� ������ ������ ������ �ٽ� ���ư���, \n�̻��� �ӽ��� ���� ������ 0���� ���ư���.")]
    public bool isAngleRecenter;

    public float _lower = 0;
    public float _upper = 1;

    bool _isCheck;
    float _valegOffset;
    float _oculusOffset;

    public bool AllowRecenter = true;


    #region �Է� �� ������ �޼���
    // ���̽�ƽ ���� ����
    bool IsActiveControl()
    {
        return (Input.GetJoystickNames().Length > 0);
    }

    // ���̽�ƽ �¿� �Է°�
    float getHorizontalValue()
    {
#if UNITY_ANDROID
        return Input.GetAxis("Axis_4");
#else
        return (InputManager.ActiveDevice.GetControl(InputControlType.Analog5).Value);
#endif
    }

    // ���̽�ƽ ���� �Է°�
    float getVerticalValue()
    {
#if UNITY_ANDROID
        return Input.GetAxis("Axis_3");
#else
        return (InputManager.ActiveDevice.GetControl(InputControlType.Analog4).Value);
#endif
    }

    // ���̽�ƽ ȸ�� �Է°�
    float getRotateValue()
    {
#if UNITY_ANDROID
        var v = Input.GetAxis("Axis_14");
        return v;
#else
        return (InputManager.ActiveDevice.GetControl(InputControlType.Analog1).Value);
#endif
    }
    #endregion

    #region ������,���̵��̵��ӵ�,�̵��ӵ� �����̴� �Լ�
    //������ �����̴� ����
    public virtual void ButtonPlus()
    {
        m_Interpolation -= 1f;
        smoothingvalue += 1f;

        if (m_Interpolation < 0.55f && smoothingvalue >= 5f)
        {
            m_Interpolation = 0.55f;
            smoothingvalue = 5f;
        }
        currentInterPolation.text = smoothingvalue.ToString();
    }

    //������ �����̴� ����
    public virtual void ButtonMinus()
    {
        if (m_Interpolation == 0.55f)
        {
            m_Interpolation = 1f;
            smoothingvalue = 4f;
        }
        else if (smoothingvalue == 1f)
        {
            smoothingvalue = 1f;
        }
        else
        {
            m_Interpolation += 1f;
            smoothingvalue -= 1f;
            if (m_Interpolation > 5f)
            {
                m_Interpolation = 5f;
            }
        }
        currentInterPolation.text = smoothingvalue.ToString();
    }
    //�̵��ӵ� ����
    public virtual void VelocityButtonPlus()
    {
        if (m_Speed == 3f && velocityvalue >= 5f)
        {
            m_Speed = 3f;
            velocityvalue = 5f;
            return;
        }
        else
        {
            m_Speed += 0.5f;
            velocityvalue += 1f;
        }
        currentvelocity.text = velocityvalue.ToString();
    }

    //�̵��ӵ� �����̴� ����
    public virtual void VelocityButtonMinus()
    {
        if (m_Speed == 1f && velocityvalue == 1f)
        {
            m_Speed = 1f;
            velocityvalue = 1f;
            return;
        }
        else
        {
            m_Speed -= 0.5f;
            velocityvalue -= 1;
        }
        currentvelocity.text = velocityvalue.ToString();
    }
    //���̵� �̵��ӵ� ����
    public virtual void SideVelocityButtonPlus()
    {
        if (m_SideSpeed == 3f && sidevelocityvalue >= 5f)
        {
            m_SideSpeed = 3f;
            sidevelocityvalue = 5f;
            return;
        }
        else
        {
            m_SideSpeed += 0.5f;
            sidevelocityvalue += 1f;
        }
        currentsidevelocity.text = sidevelocityvalue.ToString();

    }

    //���̵� �̵��ӵ� ����
    public virtual void SideVelocityButtonMinus()
    {
        if (m_SideSpeed == 1f && sidevelocityvalue == 1f)
        {
            m_SideSpeed = 1f;
            sidevelocityvalue = 1f;
            return;
        }
        else
        {
            m_SideSpeed -= 0.5f;
            sidevelocityvalue -= 1;
        }
        currentsidevelocity.text = sidevelocityvalue.ToString();
    }


    #endregion

 

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        isJump = false;
        currentInterPolation.text = smoothingvalue.ToString();
        currentvelocity.text = velocityvalue.ToString();
        currentsidevelocity.text = sidevelocityvalue.ToString();
        _isCheck = false;
    }

    private void OnApplicationQuit()
    {
        print(centerEye.transform.eulerAngles.y);
    }

    void Update()
    {
        // OVRCameraRig�� �׻� ForwardBar ���� ��ġ�ϰ� �ϱ�. 
        player.transform.position = this.gameObject.transform.position + new Vector3(0, 1.65f, 0);

#if UNITY_ANDROID
        // �����¿� �Է°� �ޱ�
        androidX = getHorizontalValue();
        androidY = getVerticalValue();

        // m_Interpolation ���� ���� �����Ͽ� �ε巯���� ����
        currentH = Mathf.Lerp(currentH, androidX, Time.deltaTime * m_Interpolation);
        currentV = Mathf.Lerp(currentV, androidY, Time.deltaTime * m_Interpolation);
        direction = new Vector3(currentH * m_SideSpeed, 0, -currentV * m_Speed);
        movement = Vector3.Lerp(Vector3.zero, direction, 1);

        // ����
        if (isJump == false && Input.GetKeyDown(KeyCode.JoystickButton8))
        {
            isJump = true;
            movementY = m_JumpPower;
        }
        // ���� ����� ��
        else if (cc.isGrounded == true)
        {
            isJump = false;
        }
        // �������� ��, �߷� ����
        movementY += m_Gravity * Time.deltaTime;
        movement.y = movementY;

        cc.Move(transform.TransformDirection(movement) * Time.deltaTime);

        // ȸ�� �Է°� �ޱ�
        androidRot = getRotateValue();
        if (IsActiveControl())
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.LerpAngle(transform.eulerAngles.y, m_OriginAngle + (androidRot * 180f), Time.deltaTime * m_LerpSpeed * 5), transform.eulerAngles.z);
        }
        else
        {
            m_OriginAngle = transform.eulerAngles.y;
        }

        // OVRCameraRig�� �ٶ󺸴� ����� VALEG�� ���������� �ٸ� ��, �����ֱ�.
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            m_OriginAngle = centerEye.transform.eulerAngles.y - (androidRot * 180f);
            transform.localEulerAngles = new Vector3(0, centerEye.transform.localEulerAngles.y, 0);
        }

#else
        InputDevice inputDevice = InputManager.ActiveDevice;

        if (inputDevice == null)
        {
            return;
        }

        InputManager.ActiveDevice.GetControl(InputControlType.Analog1).LowerDeadZone = _lower;
        InputManager.ActiveDevice.GetControl(InputControlType.Analog1).UpperDeadZone = _upper;

        _horizontal = getHorizontalValue();
        _vertical = getVerticalValue();
        _yaw = getRotateValue();
        print(_yaw);

        if (cc.enabled == false)
        {
            return;
        }

        PlayerMove();
        PlayerRotate();
        //PlayerCamera();
        // �����, ������
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            m_OriginAngle = centerEye.transform.eulerAngles.y - (_yaw * 180);
            transform.localEulerAngles = new Vector3(0, centerEye.transform.localEulerAngles.y, 0);
        }

        //��ŧ���� ������ ����
        if (OVRInput.GetDown(OVRInput.Button.Two) && !PlayerPrefs.HasKey("isCompleteOculusOffset"))
        {
            _oculusOffset = _valegOffset;
            PlayerPrefs.SetInt("isCompleteOculusOffset", 1);
            PlayerPrefs.SetFloat("oculusOffset", _oculusOffset);
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
        {
            PlayerPrefs.DeleteAll();
            print("PlayerPrefs�� �ִ� ������ ��θ� �����߽��ϴ�");
        }
#endif
    }


    private void PlayerMove()
    {
        currentH = Mathf.Lerp(currentH, _horizontal, Time.deltaTime * m_Interpolation);
        currentV = Mathf.Lerp(currentV, _vertical, Time.deltaTime * m_Interpolation);
        direction = new Vector3(currentH * 0.7f, 0, -currentV);
        movement = Vector3.Lerp(Vector3.zero, direction, 1);
        cc.Move(transform.TransformDirection(movement) * m_Speed * Time.deltaTime);
    }
    private void PlayerRotate()
    {
        if (IsActiveControl())
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.LerpAngle(transform.eulerAngles.y, isAngleRecenter == true ? m_OriginAngle + (_yaw * 180f) - _valegOffset : _yaw * 180f, Time.deltaTime * m_LerpSpeed * 5), transform.eulerAngles.z);
        }
        else
        {
            m_OriginAngle = transform.eulerAngles.y;
        }

        if (transform.position.x != 0 && !_isCheck)
        {
            DoRecenter();
            _isCheck = true;
        }

    }
    private void PlayerCamera()
    {
        if (PlayerPrefs.HasKey("isCompleteOculusOffset"))
        {
            float oculusOffset = PlayerPrefs.GetFloat("oculusOffset");
            player.transform.eulerAngles = transform.eulerAngles + new Vector3(0, -oculusOffset, 0);
        }
    }
    void DoRecenter()
    {
        if (AllowRecenter == false)
        {
            return;
        }
        AllowRecenter = true;
        _valegOffset = m_OriginAngle + (_yaw * 180f);
        Debug.LogError("offset " + _valegOffset);
        //transform.eulerAngles -= new Vector3(0, _valegOffset, 0);
        //player.transform.eulerAngles -= new Vector3(0, _valegOffset, 0);
    }

}
