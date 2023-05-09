using UnityEngine;
using TMPro;
using InControl;
using System.Linq;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Movement : MonoBehaviour
{
    [Space(15)]
    /// <summary>
    /// 카메라가 붙어 있는 상위 부모 오브젝트
    /// </summary>
    public GameObject CameraParent;
    /// <summary>
    /// 실제 카메라가 붙어 있는 오브젝트
    /// </summary>
    public GameObject CenterEye;

    // Constants
    /// <summary>
    /// 윈도우 플랫폼에서 적용되는 데드존 변수, Update에서 값을 일정하게 넣어줘야 함. 이유는 입력을 받을 때 Incontrol 서드 파티 에셋을 사용하는데 안 넣어주면 회전값이 제대로 안 나옴 (정확한 이유 확인은 모르겠음)
    /// </summary>
    private const float _lower = 0f;
    /// <summary>
    /// 윈도우 플랫폼에서 적용되는 데드존 변수, Update에서 값을 일정하게 넣어줘야 함. 이유는 입력을 받을 때 Incontrol 서드 파티 에셋을 사용하는데 안 넣어주면 회전값이 제대로 안 나옴 (정확한 이유 확인은 모르겠음)
    /// </summary>
    private const float _upper = 1f;

    // Integers

    /// <summary>
    /// 캔버스에서 부드러움을 5단계로 나누기 위한 Level (실제 디바이스에 적용되는 값은 아님, UI용)
    /// </summary>
    private int _interpoloationLevel = 3;
    /// <summary>
    /// 캔버스에서 전후진 속도를 5단계로 나누기 위한 Level (실제 디바이스에 적용되는 값은 아님, UI용)
    /// </summary>
    private int _speedLevel = 3;
    /// <summary>
    /// 캔버스에서 사이드 이동속도를 5단계로 나누기 위한 Level (실제 디바이스에 적용되는 값은 아님, UI용)
    /// </summary>
    private int _sideSpeedLevel = 2;

    /// <summary>
    /// 컨트롤러 모드, 발레그 전후좌우 입력이 들어가지 않음
    /// </summary>
    private bool _isControlModeButton = false;

    // Properties
    public int InterPolationLevel
    {
        get { return _interpoloationLevel; }

        set
        {
            _interpoloationLevel = value;
            _currentInterPolationTextMeshProUGUI.text = _interpoloationLevel.ToString();
        }
    }
    public int SpeedLevel
    {
        get { return _speedLevel; }
        set
        {
            _speedLevel = value;
            _currentSpeedTextMeshProUGUI.text = _speedLevel.ToString();
        }
    }
    public int SideSpeedLevel
    {
        get { return _sideSpeedLevel; }
        set
        {
            _sideSpeedLevel = value;
            _currentSideSpeedTextMeshProUGUI.text = _sideSpeedLevel.ToString();
        }
    }

    private bool _isValegOn => IsActiveValeg();

    // Floats
    /// <summary>
    /// 회전용 변수
    /// </summary>
    private float _originAngle = 0f;
    /// <summary>
    /// 움직임의 부드러움을 위한 보간용 변수
    /// </summary>
    private float _currentH = 0f;
    /// <summary>
    /// 움직임의 부드러움을 위한 보간용 변수
    /// </summary>
    private float _currentV = 0f;
    /// <summary>
    /// 오큘러스 카메라 높이 Offset 변수
    /// </summary>
    private float _ovrCameraRigHeight = 1.65f;
    /// <summary>
    /// 점프에 사용되는 변수
    /// </summary>
    private float _yVelocity = 0f;
    /// <summary>
    /// 점프 파워
    /// </summary>
    private float _jumpPower = 5;
    /// <summary>
    /// 중력
    /// </summary>
    private float _gravity = -9.8f;

    // Serialized Fields
    [Space(15)]
    /// <summary>
    /// UI 캔버스 Text
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI _currentInterPolationTextMeshProUGUI;
    /// <summary>
    /// UI 캔버스 Text
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI _currentSpeedTextMeshProUGUI;
    /// <summary>
    /// UI 캔버스 Text
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI _currentSideSpeedTextMeshProUGUI;

    // Components
    private CharacterController _characterController;

    /// <summary>
    /// Window, 유니티 Editor용 변수
    /// </summary>
    private InControl.InputDevice inputDevice => InControl.InputManager.ActiveDevice ?? null;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        _currentInterPolationTextMeshProUGUI.text = _interpoloationLevel.ToString();
        _currentSpeedTextMeshProUGUI.text = _speedLevel.ToString();
        _currentSideSpeedTextMeshProUGUI.text = _sideSpeedLevel.ToString();
    }

    void Update()
    {
        if (!_characterController.enabled)
        {
            return;
        }

        // 윈도우 빌드용과 유니티 에디터 전용 (윈도우, 안드로이드 포함)
        // apk로 추출하면 적용되지 않습니다.

        //이렇게 나눈 이유는, 윈도우와 유니티 에디터에서는 발레그 연결시 InControl 서드 파티에셋을 사용하고, apk 파일로 추출 시 사용하지 않기 때문입니다.
        //플랫폼에 따라 다르게 Input이 들어오기에 나눴습니다.
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        if (inputDevice == null)
        {
            Debug.Log("inputDevice가 null입니다");
            return;
        }

        InputManager.ActiveDevice.GetControl(InputControlType.Analog1).LowerDeadZone = _lower;
        InputManager.ActiveDevice.GetControl(InputControlType.Analog1).UpperDeadZone = _upper;

#endif

        // OVRCameraRig가 항상 ForwardBar 위에 위치하게 하기. 
        SetOVRCameraPosition();
        //플레이어 이동
        UpdatePlayerMovement();
        //플레이어 회전
        PlayerRotate();
        //리센터 (발레그의 전진방향으로 전진이 되지 않을때 오큘러스 카메라 방향으로 수동 조정기능)
        Recenter();
    }

    #region 인풋값 메서드
    // 조이스틱 연결 유무
    private bool IsActiveValeg()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return NativeInputDeviceManager.isValegOn;
#else
        List<Gamepad> gamepads = new List<Gamepad>(Gamepad.all);
        return gamepads.Count > 0;
#endif
    }

    // 조이스틱 좌우 입력값
    private float GetHorizontalValue()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        float input = (InputManager.ActiveDevice.GetControl(InputControlType.Analog5).Value);
        return input;
#else
        if (_isControlModeButton)
        {
            float input = Input.GetAxis("Axis_4_Oculus");
            return input;
        }
        else
        {
            float input = Input.GetAxis("Axis_4_Valeg");
            return input;
        }
#endif
    }
    // 조이스틱 전후진 입력값
    private float GetVerticalValue()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        float input = (InputManager.ActiveDevice.GetControl(InputControlType.Analog4).Value);
        return -input;
#else
        if (_isControlModeButton)
        {
            float input = Input.GetAxis("Axis_3_Oculus");
            return -input;
        }
        else
        {
            float input = Input.GetAxis("Axis_3_Valeg");
            return -input;
        }
#endif
    }
    // 조이스틱 회전 입력값
    private float GetRotateValue()
    {
#if UNITY_STANDALONE_WIN|| UNITY_EDITOR_WIN
        float input = (InputManager.ActiveDevice.GetControl(InputControlType.Analog1).Value);
        return (InputManager.ActiveDevice.GetControl(InputControlType.Analog1).Value);
#else
        if (_isControlModeButton)
        {
            float input = Input.GetAxis("Axis_14_Oculus");
            return input;
        }
        else
        {
            float input = Input.GetAxis("Axis_14_Valeg");
            return input;
        }
#endif
    }
    #endregion

    #region UI 캔버스 조절 메서드
    //스무딩 슬라이더 조정
    public void SmoothPlus()
    {
        if (InterPolationLevel == 5)
        {
            return;
        }

        InterPolationLevel++;
    }

    //스무딩 슬라이더 조정
    public void SmoothMinus()
    {
        if (InterPolationLevel == 1)
        {
            return;
        }

        InterPolationLevel--;
    }
    //이동속도 조정
    public void SpeedPlusButton()
    {
        if (SpeedLevel == 5)
        {
            return;
        }
        SpeedLevel++;
    }

    //이동속도 슬라이더 조정
    public void SpeedMinusButton()
    {
        if (SpeedLevel == 1)
        {
            return;
        }
        SpeedLevel--;
    }
    //사이드 이동속도 조정
    public void SideSpeedPlusButton()
    {
        if (SideSpeedLevel == 5)
        {
            return;
        }
        SideSpeedLevel++;
    }

    //사이드 이동속도 조정
    public void SideSpeedMinusButton()
    {
        if (SideSpeedLevel == 1)
        {
            return;
        }
        SideSpeedLevel--;
    }
    #endregion

    #region 카메라 위치 보정
    private void SetOVRCameraPosition()
    {
        CameraParent.transform.position = this.gameObject.transform.position + new Vector3(0, _ovrCameraRigHeight, 0);
    }
    #endregion

    #region 플레이어 이동
    private void UpdatePlayerMovement()
    {
        var interpolation = ConvertInterPolation(InterPolationLevel);
        var speed = ConvertSpeed(SpeedLevel);
        var sideSpeed = ConvertSpeed(SideSpeedLevel);

        var inputHorizontal = GetHorizontalValue();
        var inputVertical = GetVerticalValue();

        _currentH = SmoothInput(_currentH, inputHorizontal, interpolation);
        _currentV = SmoothInput(_currentV, inputVertical, interpolation);

        Vector3 movementDirection = CalculateMovementDirection(_currentH, _currentV, speed, sideSpeed);

        //플레이어 점프
        PlayerJump();

        ApplyPlayerMovement(movementDirection);
    }

    private float ConvertInterPolation(int level)
    {
        switch (level)
        {
            case 1:
                return 3f;
            case 2:
                return 2f;
            case 3:
                return 1f;
            case 4:
                return 0.75f;
            case 5:
                return 0.5f;
            default:
                return 0;
        }
    }

    private float ConvertSpeed(int level)
    {
        switch (level)
        {
            case 1:
                return 1f;
            case 2:
                return 1.5f;
            case 3:
                return 2f;
            case 4:
                return 2.5f;
            case 5:
                return 3f;
            default:
                return 0;
        }
    }

    private float SmoothInput(float currentValue, float targetValue, float interpolation)
    {
        float lerpSpeed = Time.deltaTime * interpolation;
        return Mathf.Lerp(currentValue, targetValue, lerpSpeed);
    }

    private Vector3 CalculateMovementDirection(float horizontal, float vertical, float speed, float sideSpeed)
    {
        float adjustedHorizontal = horizontal * sideSpeed;
        float adjustedVertical = vertical * speed;

        Vector3 direction = new Vector3(adjustedHorizontal, 0, adjustedVertical);
        return direction;
    }

    private void ApplyPlayerMovement(Vector3 movementDirection)
    {
        //중력 계산
        movementDirection.y += _gravity * Time.deltaTime;

        _characterController.Move(transform.TransformDirection(movementDirection) * Time.deltaTime);
    }

    private void PlayerJump()
    {

        //플랫폼에 따라 Input이 달라짐
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        var jumpKeycode = KeyCode.Joystick3Button8;
#else
        var jumpKeycode = KeyCode.Joystick1Button6;
#endif

        if (Input.GetKeyDown(jumpKeycode) && _characterController.isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(_jumpPower * -2f * _gravity);
            _yVelocity = jumpVelocity;
        }

        _yVelocity += _gravity * Time.deltaTime;
        Vector3 jumpDirection = new Vector3(0, _yVelocity, 0);

        _characterController.Move(jumpDirection * Time.deltaTime);
    }
    #endregion

    #region 플레이어 회전
    private void PlayerRotate()
    {
        if (_isValegOn)
        {
            float inputRotate = GetRotateValue();
            float currentYRotation = transform.eulerAngles.y;
            float targetYRotation = _originAngle + (inputRotate * 180f);
            float interpolationValue = Time.deltaTime * 15;

            float newYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, interpolationValue);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, newYRotation, transform.eulerAngles.z);
        }
        else
        {
            _originAngle = transform.eulerAngles.y;
        }
    }
    #endregion

    #region 리센터
    private void Recenter()
    {
        if (IsAButtonPressed())
        {
            float inputRotate = GetRotateValue();
            UpdateOriginAngle(inputRotate);
            AlignWithCamera();
        }
    }

    private bool IsAButtonPressed()
    {
        return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
    }

    private void UpdateOriginAngle(float inputRotate)
    {
        float centerEyeYRotation = CenterEye.transform.eulerAngles.y;
        _originAngle = centerEyeYRotation - (inputRotate * 180);
    }

    private void AlignWithCamera()
    {
        float cameraYRotation = CenterEye.transform.localEulerAngles.y;
        transform.localEulerAngles = new Vector3(0, cameraYRotation, 0);
    }
    #endregion
}
