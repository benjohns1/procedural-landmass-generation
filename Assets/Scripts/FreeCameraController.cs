using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float speed = 30f;
    public float fastSpeed = 150f;
    public float rollSpeed = 80f;
    public float fastRollSpeed = 150f;
    public Vector2 mouseSensitivity = new Vector2(100f, 100f);

    [Header("Key Bindings")]
    public string moveRightAxis = "Horizontal";
    public string moveForwardAxis = "Vertical";
    public KeyCode moveUpKey = KeyCode.Space;
    public KeyCode moveDownKey = KeyCode.C;
    public string lookRightAxis = "Mouse X";
    public string lookUpAxis = "Mouse Y";
    public KeyCode rollLeft = KeyCode.Q;
    public KeyCode rollRight = KeyCode.E;
    public KeyCode fastKey = KeyCode.LeftShift;
    public KeyCode fastLockKey = KeyCode.CapsLock;
    public KeyCode haltKey = KeyCode.Return;

    private bool fastLock = false;
    private bool halt = false;

    private void Update()
    {
        // Modifiers
        if (Input.GetKeyDown(haltKey))
        {
            halt = !halt;
        }
        if (halt)
        {
            return;
        }
        if (Input.GetKeyDown(fastLockKey))
        {
            fastLock = !fastLock;
        }
        bool fast = fastLock ^ Input.GetKey(fastKey);

        // Mouse look and roll rotation
        transform.rotation *= GetRotationChange(fast ? fastRollSpeed : rollSpeed);

        // Movement
        transform.Translate(GetMovementChange(fast ? fastSpeed : speed));
    }

    private Quaternion GetRotationChange(float rollSpeed)
    {
        float yaw = Input.GetAxisRaw(lookRightAxis) * mouseSensitivity.x;
        float pitch = -1 * Input.GetAxisRaw(lookUpAxis) * mouseSensitivity.y;
        float roll = ((Input.GetKey(rollLeft) ? 1 : 0) + (Input.GetKey(rollRight) ? -1 : 0)) * rollSpeed;
        return Quaternion.Euler(new Vector3(pitch, yaw, roll) * Time.deltaTime);
    }

    private Vector3 GetMovementChange(float speed)
    {
        float up = ((Input.GetKey(moveUpKey) ? 1 : 0) + (Input.GetKey(moveDownKey) ? -1 : 0));
        float right = Input.GetAxisRaw(moveRightAxis);
        float forward = Input.GetAxisRaw(moveForwardAxis);
        return new Vector3(right, up, forward).normalized * Time.deltaTime * speed;
    }
}
