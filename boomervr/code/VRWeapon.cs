using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.Boomer
{
    public partial class VRWeapon : AnimatedEntity
    {
        [Net]
        public Vector3 LastFramePosition { get; set; }

        [Net]
        public Vector3 PositionDelta { get; set; }

        public VRWeaponWheel WeaponWheel { get; set; }

        [Net, Predicted]
        public VRPuppet Puppet { get; set; }

        Entity VRAnchorFollow;

        public override void Spawn()
        {
            if (Owner != null && Owner is Player pl)
            {
                SetModel(pl.ActiveWeapon.Model.ResourcePath);
            }

            base.Spawn();
        }

        [Event.Client.BuildInput]
        public void BuildInputEvent()
        {
            if (WeaponWheel.EnableDrawing)
            {
                switch (WeaponWheel.activeItemIndex)
                {
                    case 0:
                        Input.SetButton(InputButton.Slot1);
                        break;
                    case 1:
                        Input.SetButton(InputButton.Slot2);
                        break;
                    case 2:
                        Input.SetButton(InputButton.Slot3);
                        break;
                    case 3:
                        Input.SetButton(InputButton.Slot4);
                        break;
                    case 4:
                        Input.SetButton(InputButton.Slot5);
                        break;
                    case 5:
                        Input.SetButton(InputButton.Slot6);
                        break;
                    default:
                        break;
                }
            }
        }

        [Event.Client.Frame]
        public void Frame()
        {
            if (VRAnchorFollow == null)
            {
                VRAnchorFollow = new Entity();
            }

            VRAnchorFollow.Transform = VR.Anchor;

            if (WeaponWheel == null && Owner != null && Owner is Player pl)
            {
                WeaponWheel = new VRWeaponWheel();
                WeaponWheel.InitWeapons(pl);
                WeaponWheel.SetParent(VRAnchorFollow);
            }

            if (Input.VR.RightHand.ButtonB.IsPressed && !WeaponWheel.EnableDrawing)
            {
                WeaponWheel.Rotation = Input.VR.RightHand.Transform.Rotation * new Angles(35f, 0f, 0f).ToRotation();
                WeaponWheel.Position = Input.VR.RightHand.Transform.Position + WeaponWheel.Rotation.Forward * 4f;
            }

            WeaponWheel.EnableDrawing = Input.VR.RightHand.ButtonB.IsPressed;

            PositionDelta = Owner.Position - LastFramePosition;

            LastFramePosition = Owner.Position;

            SetHandIK();

            Owner.Rotation = Input.VR.Head.Angles().WithPitch(0).WithRoll(0).ToRotation();

            Transform = Input.VR.RightHand.Transform;
            Position += PositionDelta;
            Rotation *= new Angles(35, 0, 0).ToRotation();
            SetBoneTransform(0, Transform);
        }

        public void SetHandIK()
        {
            Transform LHand = Input.VR.LeftHand.Transform;//AssociatedPlayer.EmptyHand.GetBoneTransform("hand_L");
            Transform RHand = Input.VR.RightHand.Transform;

            LHand.Position = Owner.Transform.PointToLocal(LHand.Position);

            LHand.Position += PositionDelta;

            LHand.Rotation *= new Angles(45f, 0, 90f).ToRotation();

            LHand.Rotation = Owner.Transform.RotationToLocal(LHand.Rotation);


            RHand.Position = Owner.Transform.PointToLocal(RHand.Position);

            RHand.Position += PositionDelta;

            RHand.Rotation *= new Angles(45f, 0, 90f).ToRotation();

            RHand.Rotation = Owner.Transform.RotationToLocal(RHand.Rotation);


            RHand.Position -= RHand.Rotation.Forward * 6f;
            LHand.Position -= LHand.Rotation.Forward * 6f;

            RHand.Position -= RHand.Rotation.Left * 3f;
            LHand.Position -= LHand.Rotation.Left * 3f;


            (Owner as AnimatedEntity).SetAnimParameter("left_hand_ik", LHand);
            (Owner as AnimatedEntity).SetAnimParameter("right_hand_ik", RHand);
        }

        [Event.Tick]
        public void Simulate()
        {
            if (Owner != null && Owner is Player pl)
            {

                Owner.Rotation = Input.VR.Head.Angles().WithPitch(0).WithRoll(0).ToRotation();

                if (pl.ActiveWeapon.Model != Model)
                {
                    SetModel(pl.ActiveWeapon.Model.ResourcePath);
                }

                pl.SetAnimParameter("b_vr", true);

                SetHandIK();

                pl.EnableHideInFirstPerson = false;

                foreach (var item in pl.Children)
                {

                    if ((item as ModelEntity).GetModelName().Contains("helmet"))
                    {

                    }
                    else
                    {
                        item.EnableHideInFirstPerson = false;
                    }
                }

                pl.SetBodyGroup("Head", 1);

                pl.ActiveWeapon.EnableDrawing = false;

                /*if (Puppet == null)
                {
                    Puppet = new VRPuppet();
                    Puppet.Owner = Owner;
                    Puppet.AssociatedPlayer = pl;
                    Puppet.Predictable = true;
                }*/

                if (Owner.Client.IsUsingVr)
                {
                    //PositionDelta = Owner.Position - LastFramePosition;

                    //LastFramePosition = Owner.Position;

                    Transform = Input.VR.RightHand.Transform;
                    Position += PositionDelta;
                    Rotation *= new Angles(35, 0, 0).ToRotation();
                    SetBoneTransform(0, Transform);

                    pl.EyePosition = GetAttachment("muzzle").Value.Position;

                    pl.EyeRotation = GetAttachment("muzzle").Value.Rotation;

                    pl.SetViewAngles(GetAttachment("muzzle").Value.Rotation.Angles());
                }
            }
        }
    }
}
