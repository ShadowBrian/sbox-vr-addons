using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;
namespace Grubs;

public partial class VRControls
{
    public static int WeaponIndex = 0;
    public static bool ChangedWeapon;

    public static bool ReplacedUI;

    public static VROverlayPanel VRMenu;

    [Event.Client.BuildInput]
    public static void BuildInput()
    {
        if (VR.Enabled)
        {
            VR.Scale = 5f;
            VR.Anchor = new Transform(Camera.Position - Vector3.Up * 64f * VR.Scale, Rotation.LookAt(Vector3.Left));


            if (!ReplacedUI)
            {
                //Game.RootPanel.Delete();
                //new GrubsHudVR();
                VRMenu = new VROverlayPanel(Game.RootPanel)
                {
                    Transform = new Transform(Vector3.Forward * 40.0f + Vector3.Up * 60.0f),
                    Width = 40.0f,
                    Curvature = 0.2f,
                };
                ReplacedUI = true;
            }

            /*var trans = new Transform(Vector3.Zero);
            trans.Position += Vector3.Forward * 40.0f + Vector3.Up * 60.0f;

            VRMenu.Transform = trans;*/

            if (Game.Clients.Count >= GrubsConfig.MinimumPlayers && (Game.IsServerHost && Game.IsClient) && GamemodeSystem.Instance is FreeForAll ffa && ffa.TerrainReady && !ffa.Started)
            {
                GameStatePanel.StartGame();
            }

            if (Game.LocalPawn != null)
            {
                Player pl = Game.LocalPawn as Player;

                if (pl.IsTurn && pl.Inventory.ActiveWeapon == null)
                {
                    pl.ActiveWeaponInput = pl.Inventory.Weapons.ElementAt(WeaponIndex);
                }

                if (pl.IsTurn && !ChangedWeapon)
                {
                    if (Input.VR.RightHand.Joystick.Value.y > 0.5f)
                    {
                        WeaponIndex++;
                        if (WeaponIndex > pl.Inventory.Weapons.Count() - 1)
                        {
                            WeaponIndex = pl.Inventory.Weapons.Count() - 1;
                        }
                        ChangedWeapon = true;
                        pl.ActiveWeaponInput = pl.Inventory.Weapons.ElementAt(WeaponIndex);
                    }

                    if (Input.VR.RightHand.Joystick.Value.y < -0.5f)
                    {
                        WeaponIndex--;
                        if (WeaponIndex < 0)
                        {
                            WeaponIndex = 0;
                        }
                        ChangedWeapon = true;
                        pl.ActiveWeaponInput = pl.Inventory.Weapons.ElementAt(WeaponIndex);
                    }
                }

                if (ChangedWeapon && Input.VR.RightHand.Joystick.Value.y > -0.5f && Input.VR.RightHand.Joystick.Value.y < 0.5f)
                {
                    ChangedWeapon = false;
                }

                Vector2 move = new Vector2(Input.VR.LeftHand.Joystick.Value.y, MathF.Round(-Input.VR.LeftHand.Joystick.Value.x));
                Input.AnalogMove = move;

                Input.SetButton(InputButton.Jump, Input.VR.RightHand.ButtonA.IsPressed);

                Input.SetButton(InputButton.Run, Input.VR.RightHand.ButtonB.IsPressed);

                Input.SetButton(InputButton.PrimaryAttack, Input.VR.RightHand.Trigger.Value > 0.75f);

                var mouseDir = Input.VR.LeftHand.Transform.Rotation.Forward + Input.VR.LeftHand.Transform.Rotation.Down;
                var tr = Trace.Ray(Input.VR.LeftHand.Transform.Position, Input.VR.LeftHand.Transform.Position + (mouseDir * 2048f)).Run();

                pl.MousePosition = tr.EndPosition.WithY(0f);//Doesn't work, gets overriden by player's buildinput

                DebugOverlay.Sphere(pl.MousePosition, 10f, Color.Green);
            }
        }
    }
}
