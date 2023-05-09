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
    /// ī�޶� �پ� �ִ� ���� �θ� ������Ʈ
    /// </summary>
    public GameObject CameraParent;
    /// <summary>
    /// ���� ī�޶� �پ� �ִ� ������Ʈ
    /// </summary>
    public GameObject CenterEye;

    // Constants
    /// <summary>
    /// ������ �÷������� ����Ǵ� ������ ����, Update���� ���� �����ϰ� �־���� ��. ������ �Է��� ���� �� Incontrol ���� ��Ƽ ������ ����ϴµ� �� �־��ָ� ȸ������ ����� �� ���� (��Ȯ�� ���� Ȯ���� �𸣰���)
    /// </summary>
    private const float _lower = 0f;
    /// <summary>
    /// ������ �÷������� ����Ǵ� ������ ����, Update���� ���� �����ϰ� �־���� ��. ������ �Է��� ���� �� Incontrol ���� ��Ƽ ������ ����ϴµ� �� �־��ָ� ȸ������ ����� �� ���� (��Ȯ�� ���� Ȯ���� �𸣰���)
    /// </summary>
    private const float _upper = 1f;

    // Integers

    /// <summary>
    /// ĵ�������� �ε巯���� 5�ܰ�� ������ ���� Level (���� ����̽��� ����Ǵ� ���� �ƴ�, UI��)
    /// </summary>
    private int _interpoloationLevel = 3;
    /// <summary>
    /// ĵ�������� ������ �ӵ��� 5�ܰ�� ������ ���� Level (���� ����̽��� ����Ǵ� ���� �ƴ�, UI��)
    /// </summary>
    private int _speedLevel = 3;
    /// <summary>
    /// ĵ�������� ���̵� �̵��ӵ��� 5�ܰ�� ������ ���� Level (���� ����̽��� ����Ǵ� ���� �ƴ�, UI��)
    /// </summary>
    private int _sideSpeedLevel = 2;

    /// <summary>
    /// ��Ʈ�ѷ� ���, �߷��� �����¿� �Է��� ���� ����
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
    /// ȸ���� ����
    /// </summary>
    private float _originAngle = 0f;
    /// <summary>
    /// �������� �ε巯���� ���� ������ ����
    /// </summary>
    private float _currentH = 0f;
    /// <summary>
    /// �������� �ε巯���� ���� ������ ����
    /// </summary>
    private float _currentV = 0f;
    /// <summary>
    /// ��ŧ���� ī�޶� ���� Offset ����
    /// </summary>
    private float _ovrCameraRigHeight = 1.65f;
    /// <summary>
    /// ������ ���Ǵ� ����
    /// </summary>
    private float _yVelocity = 0f;
    /// <summary>
    /// ���� �Ŀ�
    /// </summary>
    private float _jumpPower = 5;
    /// <summary>
    /// �߷�
    /// </summary>
    private float _gravity = -9.8f;

    // Serialized Fields
    [Space(15)]
    /// <summary>
    /// UI ĵ���� Text
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI _currentInterPolationTextMeshProUGUI;
    /// <summary>
    /// UI ĵ���� Text
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI _currentSpeedTextMeshProUGUI;
    /// <summary>
    /// UI ĵ���� Text
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI _currentSideSpeedTextMeshProUGUI;

    // Components
    private CharacterController _characterController;

    /// <summary>
    /// Window, ����Ƽ Editor�� ����
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

        // ������ ������ ����Ƽ ������ ���� (������, �ȵ���̵� ����)
        // apk�� �����ϸ� ������� �ʽ��ϴ�.

        //�̷��� ���� ������, ������� ����Ƽ �����Ϳ����� �߷��� ����� InControl ���� ��Ƽ������ ����ϰ�, apk ���Ϸ� ���� �� ������� �ʱ� �����Դϴ�.
        //�÷����� ���� �ٸ��� Input�� �����⿡ �������ϴ�.
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        if (inputDevice == null)
        {
            Debug.Log("inputDevice�� null�Դϴ�");
            return;
        }

        InputManager.ActiveDevice.GetControl(InputControlType.Analog1).LowerDeadZone = _lower;
        InputManager.ActiveDevice.GetControl(InputControlType.Analog1).UpperDeadZone = _upper;

#endif

        // OVRCameraRig�� �׻� ForwardBar ���� ��ġ�ϰ� �ϱ�. 
        SetOVRCameraPosition();
        //�÷��̾� �̵�
        UpdatePlayerMovement();
        //�÷��̾� ȸ��
        PlayerRotate();
        //������ (�߷����� ������������ ������ ���� ������ ��ŧ���� ī�޶� �������� ���� �������)
        Recenter();
    }

    #region ��ǲ�� �޼���
    // ���̽�ƽ ���� ����
    private bool IsActiveValeg()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return NativeInputDeviceManager.isValegOn;
#else
        List<Gamepad> gamepads = new List<Gamepad>(Gamepad.all);
        return gamepads.Count > 0;
#endif
    }

    // ���̽�ƽ �¿� �Է°�
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
    // ���̽�ƽ ������ �Է°�
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
    // ���̽�ƽ ȸ�� �Է°�
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

    #region UI ĵ���� ���� �޼���
    //������ �����̴� ����
    public void SmoothPlus()
    {
        if (InterPolationLevel == 5)
        {
            return;
        }

        InterPolationLevel++;
    }

    //������ �����̴� ����
    public void SmoothMinus()
    {
        if (InterPolationLevel == 1)
        {
            return;
        }

        InterPolationLevel--;
    }
    //�̵��ӵ� ����
    public void SpeedPlusButton()
    {
        if (SpeedLevel == 5)
        {
            return;
        }
        SpeedLevel++;
    }

    //�̵��ӵ� �����̴� ����
    public void SpeedMinusButton()
    {
        if (SpeedLevel == 1)
        {
            return;
        }
        SpeedLevel--;
    }
    //���̵� �̵��ӵ� ����
    public void SideSpeedPlusButton()
    {
        if (SideSpeedLevel == 5)
        {
            return;
        }
        SideSpeedLevel++;
    }

    //���̵� �̵��ӵ� ����
    public void SideSpeedMinusButton()
    {
        if (SideSpeedLevel == 1)
        {
            return;
        }
        SideSpeedLevel--;
    }
    #endregion

    #region ī�޶� ��ġ ����
    private void SetOVRCameraPosition()
    {
        CameraParent.transform.position = this.gameObject.transform.position + new Vector3(0, _ovrCameraRigHeight, 0);
    }
    #endregion

    #region �÷��̾� �̵�
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

        //�÷��̾� ����
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
        //�߷� ���
        movementDirection.y += _gravity * Time.deltaTime;

        _characterController.Move(transform.TransformDirection(movementDirection) * Time.deltaTime);
    }

    private void PlayerJump()
    {

        //�÷����� ���� Input�� �޶���
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

    #region �÷��̾� ȸ��
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

    #region ������
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
