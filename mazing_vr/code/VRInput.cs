using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Facepunch.Mazing;

public static class VRInput
{
    static float Scale = 1.5f;

    public static Rotation SnapRotate = Rotation.Identity;

    public static bool Rotated;


    [Event.Client.BuildInput]
    public static void BuildInput()
    {

        Input.AnalogMove = new Vector3(-Input.VR.LeftHand.Joystick.Value.x, -Input.VR.LeftHand.Joystick.Value.y, 0) * VR.Anchor.Rotation;

        Input.SetButton(InputButton.Jump, Input.VR.RightHand.ButtonA.IsPressed);

        var pos = Game.LocalPawn.Transform.WithRotation(SnapRotate);

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

        VR.Anchor = pos;

        VR.Scale = Scale;

        Scale += Input.VR.RightHand.Joystick.Value.y * Time.Delta * 2f;

        Scale = MathX.Clamp(Scale, 1f, 10f);
    }
}
