using UnityEngine;
using TMPro;
using InControl;

public class Movement : MonoBehaviour
{
    // Public variables
    [Tooltip("점프 파워 조절")]
    public float JumpPower = 5;
    public GameObject OVRCamera;
    public GameObject CenterEye;

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

    // Constants
    private const float _lower = 0;
    private const float _upper = 1;

    // Integers
    private int _interpoloationLevel = 3;
    private int _speedLevel = 3;
    private int _sideSpeedLevel = 2;

    // Floats
    private float _originAngle = 0f;
    private float _currentH;
    private float _currentV;

    // Booleans

    // Serialized Fields
    [SerializeField]
    private TextMeshProUGUI _currentInterPolationTextMeshProUGUI;
    [SerializeField]
    private TextMeshProUGUI _currentSpeedTextMeshProUGUI;
    [SerializeField]
    private TextMeshProUGUI _currentSideSpeedTextMeshProUGUI;

    // Components
    private CharacterController _characterController;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        _currentInterPolationTextMeshProUGUI.text = _interpoloationLevel.ToString();
        _currentSpeedTextMeshProUGUI.text = _speedLevel.ToString();
        _currentSideSpeedTextMeshProUGUI.text = _sideSpeedLevel.ToString();
    }

    #region 인풋값 메서드
    // 조이스틱 연결 유무
    private bool IsActiveControl()
    {
        return (Input.GetJoystickNames().Length > 0);
    }
    // 조이스틱 좌우 입력값
    private float GetHorizontalValue()
    {
#if UNITY_ANDROID
        float input = Input.GetAxis("Axis_4");
        return input;
#else
        float input= (InputManager.ActiveDevice.GetControl(InputControlType.Analog5).Value);
        return input;
#endif
    }
    // 조이스틱 전후진 입력값
    private float GetVerticalValue()
    {
#if UNITY_ANDROID
        float input = Input.GetAxis("Axis_3");
        return -input;
#else
        float input = (InputManager.ActiveDevice.GetControl(InputControlType.Analog4).Value);
        return -input;
#endif
    }
    // 조이스틱 회전 입력값
    private float GetRotateValue()
    {
#if UNITY_ANDROID
        var v = Input.GetAxis("Axis_14");
        return -v;
#else
        float input = (InputManager.ActiveDevice.GetControl(InputControlType.Analog1).Value);
        return (InputManager.ActiveDevice.GetControl(InputControlType.Analog1).Value);
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

    void Update()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // Window에서 실행할 코드 작성
        InputDevice inputDevice = InputManager.ActiveDevice;

        if (inputDevice == null || !_characterController.enabled)
        {
            return;
        }

        InputManager.ActiveDevice.GetControl(InputControlType.Analog1).LowerDeadZone = _lower;
        InputManager.ActiveDevice.GetControl(InputControlType.Analog1).UpperDeadZone = _upper;
#endif
        // OVRCameraRig가 항상 ForwardBar 위에 위치하게 하기. 
        OVRCamera.transform.position = this.gameObject.transform.position + new Vector3(0, 1.65f, 0);

        //플레이어 이동
        PlayerMove();
        //플레이어 회전
        PlayerRotate();
        //리센터 (발레그의 전진방향으로 전진이 되지 않을때 오큘러스 카메라 방향으로 수동 조정기능)
        Recenter();
    }


    #region 플레이어 이동
    private void PlayerMove()
    {
        var interpolation = ConvertInterPolation(InterPolationLevel);
        var speed = ConvertSpeed(SpeedLevel);
        var sideSpeed = ConvertSpeed(SideSpeedLevel);

        var inputHorizontal = GetHorizontalValue();
        var inputVertical = GetVerticalValue();

        _currentH = SmoothInput(_currentH, inputHorizontal, interpolation);
        _currentV = SmoothInput(_currentV, inputVertical, interpolation);

        Vector3 movementDirection = CalculateMovementDirection(_currentH, _currentV, speed, sideSpeed);

        MovePlayer(movementDirection);
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
        return Mathf.Lerp(currentValue, targetValue, Time.deltaTime * interpolation);
    }

    private Vector3 CalculateMovementDirection(float horizontal, float vertical, float speed, float sideSpeed)
    {
        Vector3 direction = new Vector3(horizontal * 0.7f * sideSpeed, 0, vertical * speed);
        return Vector3.Lerp(Vector3.zero, direction, 1);
    }

    private void MovePlayer(Vector3 movementDirection)
    {
        _characterController.Move(transform.TransformDirection(movementDirection) * Time.deltaTime);
    }
    #endregion

    #region 플레이어 회전
    private void PlayerRotate()
    {
        if (IsActiveControl())
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
        return OVRInput.GetDown(OVRInput.Button.One);
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
