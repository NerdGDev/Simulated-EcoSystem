using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExplorer : MonoBehaviour
{
    [Header("Handling")]
    public float thrustStrength = 20;
    public float rotSpeed = 5;
    public float rollSpeed = 30;
    public float rotSmoothSpeed = 10;
    public bool lockCursor;

    public GameObject CameraArm;

    Rigidbody rb;
    Quaternion targetRot;
    Quaternion smoothedRot;
    Quaternion cameraRot;

    Vector3 thrusterInput;
    int numCollisionTouches;

    KeyCode ascendKey = KeyCode.Space;
    KeyCode descendKey = KeyCode.LeftShift;
    KeyCode rollCounterKey = KeyCode.Q;
    KeyCode rollClockwiseKey = KeyCode.E;
    KeyCode forwardKey = KeyCode.W;
    KeyCode backwardKey = KeyCode.S;
    KeyCode leftKey = KeyCode.A;
    KeyCode rightKey = KeyCode.D;

    private void Awake()
    {
        targetRot = transform.rotation;
        smoothedRot = transform.rotation;
        cameraRot = CameraArm.transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        DebugHelper.HandleEditorInput(lockCursor);
        // Thruster input
        int thrustInputX = GetInputAxis(leftKey, rightKey);
        int thrustInputY = GetInputAxis(descendKey, ascendKey);
        int thrustInputZ = GetInputAxis(backwardKey, forwardKey);
        thrusterInput = new Vector3(thrustInputX, thrustInputY, thrustInputZ);

        // Rotation input
        float yawInput = Input.GetAxisRaw("Mouse X") * rotSpeed;
        float pitchInput = Input.GetAxisRaw("Mouse Y") * rotSpeed;
        float rollInput = GetInputAxis(rollCounterKey, rollClockwiseKey) * rollSpeed * Time.deltaTime;

        Debug.Log(yawInput);
        Debug.Log(pitchInput);
        Debug.Log(numCollisionTouches);

        // Calculate rotation
        if (numCollisionTouches == 0)
        {
            var yaw = Quaternion.AngleAxis(yawInput, transform.up);
            var pitch = Quaternion.AngleAxis(-pitchInput, transform.right);
            var roll = Quaternion.AngleAxis(-rollInput, transform.forward);

            targetRot = yaw * pitch * roll * targetRot;
            smoothedRot = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotSmoothSpeed);
            cameraRot = Quaternion.RotateTowards(cameraRot, targetRot, Time.deltaTime * rotSmoothSpeed);
        }
        else
        {
            targetRot = transform.rotation;
            smoothedRot = transform.rotation;
        }
    }

    int GetInputAxis(KeyCode negativeAxis, KeyCode positiveAxis)
    {
        int axis = 0;
        if (Input.GetKey(positiveAxis))
        {
            axis++;
        }
        if (Input.GetKey(negativeAxis))
        {
            axis--;
        }
        return axis;
    }

    void FixedUpdate()
    {
        HandleMovement();
        CameraArm.transform.rotation = smoothedRot;
        // Thrusters
        Vector3 thrustDir = transform.TransformVector(thrusterInput);
        rb.AddForce(thrustDir * thrustStrength);

        if (numCollisionTouches == 0)
        {
            rb.MoveRotation(smoothedRot);
        }
    }
}

public static class DebugHelper
{

    public static void HandleEditorInput(bool lockCursor)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Break();
        }
    }
}
