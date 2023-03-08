using Facepunch.Boomer.WeaponSystem;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch.Boomer.UI;
namespace Facepunch.Boomer;

public partial class VRControls
{
    public static int WeaponIndex = 0;
    public static bool Rotated;

    public static bool ReplacedUI;

    public static Model WeaponModel;

    public static VRWeapon WeaponEnt;

    public static List<IClient> ClientList = new List<IClient>();

    public static Rotation SnapRotate = Rotation.Identity;

    [Event.Client.PostCamera]
    public static void Postcam()
    {
        VR.Scale = 1f;
        Transform pos = Game.LocalPawn.Transform.WithRotation(SnapRotate);
        //pos.Position -= SnapRotate.Inverse * (VR.Anchor.Position - Input.VR.Head.Position.WithZ(0));
        VR.Anchor = pos;
    }

    [Event.Tick.Server]
    public static void Tick()
    {
        foreach (var item in Game.Clients)
        {
            if (item.IsUsingVr && !ClientList.Contains(item))
            {
                var WeaponEnt = new VRWeapon();
                WeaponEnt.Owner = item.Pawn as Entity;
                WeaponEnt.Predictable = true;

                ClientList.Add(item);
            }
        }

        foreach (var item in ClientList)
        {
            if (!Game.Clients.Contains(item))
            {
                ClientList.Remove(item);
            }
        }
    }

    [Event.Client.BuildInput]
    public static void BuildInput()
    {
        if (VR.Enabled)
        {
            if (!ReplacedUI)
            {
                new HudVR();
                ReplacedUI = true;
            }

            if (Rotated && Input.VR.RightHand.Joystick.Value.x > -0.5f && Input.VR.RightHand.Joystick.Value.x < 0.5f)
            {
                Rotated = false;
            }

            if (!Rotated)
            {
                if (Input.VR.RightHand.Joystick.Value.x > 0.5f)
                {
                    SnapRotate *= new Angles(0, -30, 0).ToRotation();
                    Rotated = true;
                }

                if (Input.VR.RightHand.Joystick.Value.x < -0.5f)
                {
                    SnapRotate *= new Angles(0, 30, 0).ToRotation();
                    Rotated = true;
                }
            }

            Vector2 move = new Vector2(Input.VR.LeftHand.Joystick.Value.y, MathF.Round(-Input.VR.LeftHand.Joystick.Value.x));
            Input.AnalogMove = Input.VR.Head.Rotation * (Game.LocalPawn.Rotation).Inverse * move;

            Input.SetButton(InputButton.Jump, Input.VR.RightHand.ButtonA.IsPressed);

            Input.SetButton(InputButton.Run, Input.VR.LeftHand.Trigger.Value > 0.75f);

            Input.SetButton(InputButton.PrimaryAttack, Input.VR.RightHand.Trigger.Value > 0.75f);

            Input.SetButton(InputButton.SecondaryAttack, Input.VR.RightHand.JoystickPress.IsPressed);

            Input.SetButton(InputButton.Duck, (Input.VR.Head.Position - Game.LocalPawn.Position).z < 50f);

        }
    }
}

