using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox;

public static class VRControls
{
    public static bool ReplacedUI;

    public static VROverlayPanel VRMenu;

    [Event.Client.Frame]
    public static void Frame()
    {
        VR.Anchor = Game.LocalPawn.Transform;
    }

    [Event.Client.BuildInput]
    public static void BuildInput()
    {
        if (VR.Enabled)
        {

            if (!ReplacedUI && Entity.All.OfType<RootPanel>().Any())
            {

                VRMenu = new VROverlayPanel(Entity.All.OfType<RootPanel>().First())
                {
                    Transform = new Transform(Vector3.Forward * 40.0f + Vector3.Up * 60.0f),
                    Width = 40.0f,
                    Curvature = 0.2f,
                };


                ReplacedUI = true;
            }

            Vector2 move = new Vector2(Input.VR.LeftHand.Trigger.Value, -Input.VR.LeftHand.Joystick.Value.x);
            Input.AnalogMove = move;//Input.VR.Head.Rotation * (Game.LocalPawn.Rotation).Inverse * move;

            Input.SetButton(InputButton.Forward, Input.VR.LeftHand.Trigger.Value > 0.75f);

            Input.SetButton(InputButton.Back, Input.VR.RightHand.Trigger.Value > 0.75f);

            Input.SetButton(InputButton.Right, Input.VR.LeftHand.Joystick.Value.x > 0.75f);

            Input.SetButton(InputButton.Left, Input.VR.LeftHand.Joystick.Value.x < -0.75f);

        }
    }
}
