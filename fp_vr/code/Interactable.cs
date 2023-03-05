using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    [Prefab]
    public partial class Interactable : AnimatedEntity
    {
        [Prefab]
        public bool IsThrottle { get; set; } = false;

        public override void ClientSpawn()
        {
            base.ClientSpawn();
            SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
        }

        public Transform handController;

        public bool gripped;
        bool toggledgrip;

        public float angleThreshold = 30f;
        public float maxOffset = 0.5f;

        public Vector2 actualdirection;

        public Vector3 offset;

        Input.VrHand ClosestHand;

        [Event.Client.Frame]
        public void FrameSim()
        {
            if (IsThrottle)
            {
                ClosestHand = Input.VR.LeftHand;
                handController = VRControls.LeftHand.Transform;
            }
            else
            {
                ClosestHand = Input.VR.RightHand;
                handController = VRControls.RightHand.Transform;
            }


            //handController.Position += VRControls.PositionDelta * Time.Delta;

            // get the hand controller's position and rotation
            Vector3 handPosition = handController.Position;

            //DebugOverlay.Sphere(GetBoneTransform(1).Position + Vector3.Up * 0.5f, 1f, Color.Green);

            if (Vector3.DistanceBetween(handPosition, GetBoneTransform(1).Position + Vector3.Up * 0.5f) < 1f)
            {
                if (Input.VR.IsKnuckles)
                {
                    gripped = ClosestHand.Grip.Value > 0.5f;
                }
                else
                {
                    if (!toggledgrip && ClosestHand.Grip.Value > 0.5f)
                    {
                        toggledgrip = true;
                        gripped = !gripped;
                    }

                    if (toggledgrip && ClosestHand.Grip.Value < 0.5f)
                    {
                        toggledgrip = false;
                    }
                }
            }
            else
            {
                gripped = false;
            }

            if (gripped)
            {
                // calculate the offset vector between the joystick's center position and the hand controller's position
                offset = handPosition - Position;
                offset.z = 0f;
                offset *= Rotation.Inverse;
            }

            // calculate the direction based on the offset vector
            Vector2 direction = new Vector2(-offset.y, offset.x).Normal;

            if (!gripped && !IsThrottle)
            {
                direction = Vector2.Zero;
            }

            // calculate the amount of offset from the neutral pose
            float offsetMagnitude = new Vector2(-offset.y, offset.x).Length;
            float normalizedOffsetMagnitude = offsetMagnitude / maxOffset;
            float clampedOffsetMagnitude = MathX.Clamp(normalizedOffsetMagnitude, 0f, 1f);

            // interpolate between the neutral pose and the direction based on the offset
            Vector2 neutralPose = new Vector2(0f, 0f);
            actualdirection = Vector2.Lerp(neutralPose, direction, clampedOffsetMagnitude);

            SetAnimParameter("leftright", actualdirection.x);
            SetAnimParameter("forwardback", actualdirection.y);

            //DebugOverlay.Line(Position, Position + Rotation.Forward * direction.y * 2f);

            // DebugOverlay.Line(Position, Position + Rotation.Right * direction.x * 2f);
        }


    }
}
