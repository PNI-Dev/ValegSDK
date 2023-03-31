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
    [Tooltip("부드러운 이동 조절")]
    [Range(0.1f, 10f)]
    public float m_Interpolation = 1f;


    // 이동, 회전 입력값 변수들
    float _horizontal;
    float _vertical;
    float _yaw;

    float currentH = 0;
    float currentV = 0;

    Vector3 direction;
    Vector3 movement;

    [Tooltip("점프 파워 조절")]
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
    [Tooltip("플레이어 몸")]
    public GameObject player;
    [Tooltip("플레이어 시야")]
    public GameObject centerEye;

    CharacterController cc;

    [Space(15)]
    [Tooltip("CharacterController이기 때문에 중력 따로 설정 필요")]
    public float m_Gravity = -9.8f;
    [Tooltip("이동 속도")]
    public float m_Speed = 2f;
    [Tooltip("옆걸음 속도")]
    public float m_SideSpeed = 1.5f;
    [Tooltip("회전 속도")]
    public float m_LerpSpeed = 3f;

    [Space(15)]
    [Tooltip("각도 초기값")]
    public float m_OriginAngle = 0f;

    [Tooltip("[각도 리센터 사용 유무] \n사용시 머신이 껐다 켜져도 마지막 값에서 다시 돌아가고, \n미사용시 머신이 껐다 켜지면 0도로 돌아간다.")]
    public bool isAngleRecenter;

    public float _lower = 0;
    public float _upper = 1;

    bool _isCheck;
    float _valegOffset;
    float _oculusOffset;

    public bool AllowRecenter = true;


    #region 입력 값 들어오는 메서드
    // 조이스틱 연결 유무
    bool IsActiveControl()
    {
        return (Input.GetJoystickNames().Length > 0);
    }

    // 조이스틱 좌우 입력값
    float getHorizontalValue()
    {
#if UNITY_ANDROID
        return Input.GetAxis("Axis_4");
#else
        return (InputManager.ActiveDevice.GetControl(InputControlType.Analog5).Value);
#endif
    }

    // 조이스틱 상하 입력값
    float getVerticalValue()
    {
#if UNITY_ANDROID
        return Input.GetAxis("Axis_3");
#else
        return (InputManager.ActiveDevice.GetControl(InputControlType.Analog4).Value);
#endif
    }

    // 조이스틱 회전 입력값
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

    #region 스무딩,사이드이동속도,이동속도 슬라이더 함수
    //스무딩 슬라이더 조정
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

    //스무딩 슬라이더 조정
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
    //이동속도 조정
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

    //이동속도 슬라이더 조정
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
    //사이드 이동속도 조정
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

    //사이드 이동속도 조정
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
        // OVRCameraRig가 항상 ForwardBar 위에 위치하게 하기. 
        player.transform.position = this.gameObject.transform.position + new Vector3(0, 1.65f, 0);

#if UNITY_ANDROID
        // 상하좌우 입력값 받기
        androidX = getHorizontalValue();
        androidY = getVerticalValue();

        // m_Interpolation 변수 값을 조절하여 부드러움을 조절
        currentH = Mathf.Lerp(currentH, androidX, Time.deltaTime * m_Interpolation);
        currentV = Mathf.Lerp(currentV, androidY, Time.deltaTime * m_Interpolation);
        direction = new Vector3(currentH * m_SideSpeed, 0, -currentV * m_Speed);
        movement = Vector3.Lerp(Vector3.zero, direction, 1);

        // 점프
        if (isJump == false && Input.GetKeyDown(KeyCode.JoystickButton8))
        {
            isJump = true;
            movementY = m_JumpPower;
        }
        // 땅에 닿았을 때
        else if (cc.isGrounded == true)
        {
            isJump = false;
        }
        // 점프있을 때, 중력 적용
        movementY += m_Gravity * Time.deltaTime;
        movement.y = movementY;

        cc.Move(transform.TransformDirection(movement) * Time.deltaTime);

        // 회전 입력값 받기
        androidRot = getRotateValue();
        if (IsActiveControl())
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.LerpAngle(transform.eulerAngles.y, m_OriginAngle + (androidRot * 180f), Time.deltaTime * m_LerpSpeed * 5), transform.eulerAngles.z);
        }
        else
        {
            m_OriginAngle = transform.eulerAngles.y;
        }

        // OVRCameraRig가 바라보는 방향과 VALEG의 전진방향이 다를 때, 맞춰주기.
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
        // 막대기, 리센터
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            m_OriginAngle = centerEye.transform.eulerAngles.y - (_yaw * 180);
            transform.localEulerAngles = new Vector3(0, centerEye.transform.localEulerAngles.y, 0);
        }

        //오큘러스 오프셋 설정
        if (OVRInput.GetDown(OVRInput.Button.Two) && !PlayerPrefs.HasKey("isCompleteOculusOffset"))
        {
            _oculusOffset = _valegOffset;
            PlayerPrefs.SetInt("isCompleteOculusOffset", 1);
            PlayerPrefs.SetFloat("oculusOffset", _oculusOffset);
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
        {
            PlayerPrefs.DeleteAll();
            print("PlayerPrefs에 있는 데이터 모두를 삭제했습니다");
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
