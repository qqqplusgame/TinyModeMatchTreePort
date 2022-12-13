//using Unity.Tiny.Input;

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace ProjectM
{
    public static class InputUtil
    {
        public static bool GetInputDown() //InputSystem input)
        {
            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.wasPressedThisFrame;
            }

            if (Touchscreen.current != null)
            {
                return Touch.activeTouches.Count > 0 && Touch.activeTouches[0].phase == TouchPhase.Began;
            }

            //     if (input.IsMousePresent())
            //     return input.GetMouseButtonDown(0);
            //
            // return input.TouchCount() > 0 &&
            //     input.GetTouch(0).phase == TouchState.Began;
            return false;
        }

        /// <summary>
        /// check if mouse or touch is pressed
        /// </summary>
        /// <returns></returns>
        public static bool GetInputPressed()
        {
            //ut.Runtime.Input.getMouseButton(0) || (ut.Runtime.Input.touchCount() == 1 &&
            //     (ut.Runtime.Input.getTouch(0).phase == ut.Core2D.TouchState.Stationary || ut.Runtime.Input.getTouch(0).phase == ut.Core2D.TouchState.Moved));
            //
            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.isPressed;
            }

            if (Touchscreen.current != null)
            {
                return (Touch.activeTouches.Count == 1 && Touch.activeTouches[0].phase == TouchPhase.Stationary)
                       || (Touch.activeTouches.Count == 1 && Touch.activeTouches[0].phase == TouchPhase.Moved);
            }

            return false;
        }

        public static bool GetInputUp() //InputSystem input)
        {
            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.wasReleasedThisFrame;
            }
            // if (input.IsMousePresent())
            //     return input.GetMouseButtonUp(0);
            //
            // return input.TouchCount() > 0 &&
            //     input.GetTouch(0).phase == TouchState.Ended;

            if (Touchscreen.current != null)
            {
                return Touch.activeTouches.Count > 0 && Touch.activeTouches[0].phase == TouchPhase.Ended;
            }

            return false;
        }

        public static float2 GetInputPosition() //InputSystem input)
        {
            if (Mouse.current != null)
            {
                var pos = Mouse.current.position.ReadValue();
                //return camera.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 0));
                return CameraUtil.ScreenPointToWorldPoint(World.DefaultGameObjectInjectionWorld,new float2(pos.x, pos.y));
            }

            if (Touchscreen.current != null && Touch.activeTouches.Count > 0)
            {
                var pos = Touch.activeTouches[0].screenPosition;
                return CameraUtil.ScreenPointToWorldPoint(World.DefaultGameObjectInjectionWorld,new float2(pos.x, pos.y));
            }

            return float2.zero;
            // if (input.IsMousePresent())
            //     return input.GetInputPosition();
            //
            // return input.TouchCount() > 0 ? new float2(input.GetTouch(0).x, input.GetTouch(0).y) : float2.zero;
        }
    }
}