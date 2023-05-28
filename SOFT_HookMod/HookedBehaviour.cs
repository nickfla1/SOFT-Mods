using TheForest.Utils;
using UnityEngine;

namespace SOFT_HookMod
{
    internal class HookedBehaviour : MonoBehaviour
    {
        private const float MIN_DISTANCE = 2f;
        private const float MAX_DISTANCE = 30f;
        private const float STOP_DISTANCE = 1f;
        private const float MIN_DISTANCE_SQR = MIN_DISTANCE * MIN_DISTANCE;
        private const float MAX_DISTANCE_SQR = MAX_DISTANCE * MAX_DISTANCE;
        private const float STOP_DISTANCE_SQR = STOP_DISTANCE * STOP_DISTANCE;

        // TODO: make it an easing function
        private const float ON_HOOK_VELOCITY = 60f;

        private const float HOOK_SIZE_SMALL = 4f;
        private const float HOOK_SIZE_LARGE = 8f;

        private enum HookPlayerState
        {
            Moving,
            Looking,
        }

        private enum HookState
        {
            Idle,
            TooClose,
            TooFar,
            Ungrappable,
            NoHit,
            CanGrapple,
        }

        private Vector3 _hookHit = Vector3.zero;
        private HookState _hookState = HookState.Idle;
        private HookPlayerState _hookPlayerState = HookPlayerState.Looking;

        private LayerMask _hookLM = LayerMask.GetMask(new string[] { "TreeWS", "Terrain", "Water", "Blocker", "BasicCollider" });

        private void GrapplingHook()
        {
            switch (_hookPlayerState)
            {
                case HookPlayerState.Looking:
                    UpdateHookLook();
                    break;
                case HookPlayerState.Moving:
                    MoveToHook();
                    break;
            }
        }

        private void UpdateHookLook()
        {
            var cam = LocalPlayer.MainCam;

            RaycastHit hit;
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);

            if (Physics.Raycast(ray, out hit, 200f, _hookLM))
            {
                if (hit.collider.tag == "Water")
                {
                    _hookState = HookState.Ungrappable;
                    return;
                }

                var toHit = hit.point - LocalPlayer.GameObject.transform.position;
                var dist = toHit.sqrMagnitude;

                if (dist < MIN_DISTANCE_SQR)
                {
                    _hookState = HookState.TooClose;
                }
                else if (dist > MAX_DISTANCE_SQR)
                {
                    _hookState = HookState.TooFar;
                }
                else
                {
                    _hookState = HookState.CanGrapple;
                }

                _hookHit = hit.point;
            }
            else
            {
                _hookState = HookState.NoHit;
            }

            if (_hookState == HookState.CanGrapple && Input.GetMouseButtonDown(2))
            {
                _hookPlayerState = HookPlayerState.Moving;
            }
        }

        private void MoveToHook()
        {
            var toHit = _hookHit - LocalPlayer.GameObject.transform.position;
            
            if (toHit.sqrMagnitude <= STOP_DISTANCE_SQR)
            {
                LocalPlayer.FpCharacter.SetCanJump(true);
                LocalPlayer.FpCharacter.EnableCrouch();
                _hookPlayerState = HookPlayerState.Looking;
                _hookState = HookState.Idle;

                return;
            }

            LocalPlayer.FpCharacter.SetOverrideMovement(Vector2.zero);
            LocalPlayer.FpCharacter.SetCanJump(false);
            LocalPlayer.FpCharacter.DisableCrouch();

            var dir = toHit.normalized;
            var velocity = dir * Time.deltaTime * ON_HOOK_VELOCITY;

            LocalPlayer.GameObject.transform.position += velocity;
        }

        // Invoked by Unity at runtime
        private void OnGUI()
        {
            if (!LocalPlayer.IsInWorld || Sons.Gui.PauseMenu.IsActive)
            {
                return;
            }

            GUI.depth = 0;

            float size = HOOK_SIZE_SMALL;

            switch (_hookState)
            {
                case HookState.Idle:
                case HookState.NoHit:
                    GUI.color = Color.gray;
                    break;
                case HookState.CanGrapple:
                    GUI.color = Color.white;
                    size = HOOK_SIZE_LARGE;
                    break;
                case HookState.TooClose:
                case HookState.TooFar:
                case HookState.Ungrappable:
                    GUI.color = Color.red;
                    break;
            }

            var x = (Screen.width - size) / 2.0f;
            var y = (Screen.height - size) / 2.0f;

            GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture, ScaleMode.StretchToFill);
        }

        // Invoked by Unity at runtime
        private void Update()
        {
            if (!LocalPlayer.IsInWorld || Sons.Gui.PauseMenu.IsActive) {
                return;
            }

            GrapplingHook();
        }
    }
}
