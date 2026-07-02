using UnityEngine;
using UnityEngine.InputSystem;

public class WebGLFocusFix : MonoBehaviour
{
    private void Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = true;
        #endif
    }

    // Клик по экрану для захвата фокуса
    private void Update()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            WebGLInput.captureAllKeyboardInput = true;
        }
        #endif
    }
}