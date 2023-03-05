using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public partial class VRControls
{
	public static int WeaponIndex = 0;
	public static bool ChangedWeapon;

	public static bool ReplacedUI;
	public static bool SpawnedCockpit;


	public static AnimatedEntity LeftHand, RightHand;

	public static Vector3 LastFramePosition;
	public static Vector3 PositionDelta;


	public static Vector3 originalPosition;
	public static Rotation originalRotation;

	public static Rotation offsetRotation;
	public static Vector3 offsetPosition;

	public static Rotation FromToRotation(Vector3 fromDirection, Vector3 toDirection)
	{
		Vector3 cross = Vector3.Cross(fromDirection, toDirection);
		float dot = Vector3.Dot(fromDirection, toDirection);
		float angle = MathX.RadianToDegree(MathF.Atan2(cross.Length, dot));
		Vector3 axis = cross.Normal;
		return Rotation.FromAxis(axis, angle);
	}

	[Event.Client.Frame]
	public static void Frame()
	{
		if (VR.Enabled && Game.LocalPawn != null)
		{
			//activeCockpit.Position = Game.LocalPawn.Position;
			// activeCockpit.Rotation = Rotation.LookAt(Game.LocalPawn.Rotation.Forward.WithZ(0));
			VR.Scale = 0.125f;
			Camera.ZNear = 1f;
			Transform pos = activeCockpit.Transform;
			pos.Position += activeCockpit.Rotation.Forward * 23f;



			if (Input.VR.LeftHand.ButtonA.WasPressed)
			{
				//offsetPosition = (Input.VR.Head.Position - VR.Anchor.Position).WithZ(-1f);
				//offsetPosition = VR.Anchor.NormalToWorld(offsetPosition);
				//offsetPosition = new Vector3(offsetPosition.y, offsetPosition.x, offsetPosition.z);

				// Calculate the offset between the current headset position and the playspace center
				offsetPosition = (Input.VR.Head.Position - VR.Anchor.Position).WithZ(-1f);

				// Calculate the offset rotation needed to align the headset forward direction with the playspace forward direction
				Vector3 headsetForward = (Input.VR.Head.Rotation * Vector3.Forward).WithZ(0);
				Vector3 playspaceForward = VR.Anchor.Rotation * Vector3.Forward;
				offsetRotation = FromToRotation(headsetForward, playspaceForward);

				// Rotate the offset position to match the playspace rotation
				offsetPosition = VR.Anchor.Rotation.Inverse * offsetPosition;
				offsetPosition = offsetRotation * offsetPosition;
				offsetPosition = VR.Anchor.Rotation * offsetPosition;
			}

			if (offsetPosition != Vector3.Zero)
			{
				//pos.Rotation *= offsetRotation;
				//pos.Position -= offsetPosition;
			}

			VR.Anchor = pos;

			PositionDelta = activeCockpit.Position - LastFramePosition;

			LastFramePosition = activeCockpit.Position;

			if (SpawnedCockpit)
			{
				LeftHand.Transform = RotateTransform(Input.VR.LeftHand.Transform);
				RightHand.Transform = RotateTransform(Input.VR.RightHand.Transform);
				LeftHand.Position += PositionDelta;
				RightHand.Position += PositionDelta;

				RightHand.Scale = 0.1f;
				LeftHand.Scale = 0.1f;
			}
		}
	}

	public static Transform RotateTransform(Transform trans)
	{
		Transform RotatedTransform = trans;
		Vector3 LocalizedPosition = trans.Position - VR.Anchor.Position;
		Angles AnchorAngles = new Angles(VR.Anchor.Rotation.Angles().pitch * 0.1f, 0, VR.Anchor.Rotation.Angles().roll);
		//Vector3 RotatedPosition = LocalizedPosition * AnchorAngles.ToRotation();
		//RotatedTransform.Position += VR.Anchor.Rotation.Forward * VR.Anchor.Rotation.Angles().pitch * 0.125f;
		//RotatedTransform.Position += VR.Anchor.Rotation.Up * VR.Anchor.Rotation.Angles().pitch * 0.005f;
		//RotatedTransform.Position += RotatedPosition;
		return RotatedTransform;
	}

	public static Cockpit activeCockpit;

	public static Interactable joy;
	public static Interactable throt;

	[Event.Client.BuildInput]
	public static void BuildInput()
	{
		if (!ReplacedUI)
		{
			//Game.RootPanel.Delete();
			//new HudVR();
			ReplacedUI = true;
		}

		if (!SpawnedCockpit)
		{
			activeCockpit = Cockpit.FromPrefab("prefabs/cockpit1.prefab");
			activeCockpit.SetParent(Game.LocalPawn);
			activeCockpit.LocalPosition = Vector3.Zero;
			activeCockpit.LocalRotation = Rotation.Identity;
			foreach (var item in activeCockpit.Children)
			{
				if (item is Interactable stick && stick.IsThrottle)
				{
					throt = stick;
				}
				else if (item is Interactable stick2)
				{
					joy = stick2;
				}
			}
			LeftHand = new AnimatedEntity("models/hands/hand_basic_left.vmdl");

			LeftHand.SetParent(activeCockpit);
			RightHand = new AnimatedEntity("models/hands/hand_basic.vmdl");

			RightHand.SetParent(activeCockpit);
			SpawnedCockpit = true;
		}

		if (joy.Parent == null || !activeCockpit.EnableDrawing)
		{
			Log.Info("We died!");
			activeCockpit.Delete();
			activeCockpit = null;
			joy = null;
			SpawnedCockpit = false;
			return;
		}

		if (activeCockpit == null)
		{
			SpawnedCockpit = false;
		}

		if (VR.Enabled)
		{

			if (Game.LocalPawn != null && joy != null)
			{
				LeftHand.SetAnimParameter("grip", Input.VR.LeftHand.Grip.Value);
				RightHand.SetAnimParameter("grip", Input.VR.RightHand.Grip.Value);

				Vector2 move = new Vector2(0f, throt.gripped ? -Input.VR.LeftHand.Joystick.Value.x : 0f);
				Input.AnalogMove = move;
				Input.AnalogLook = new Angles(joy.GetAnimParameterFloat("forwardback") / 2f, -joy.GetAnimParameterFloat("leftright"), 0);

				Input.SetButton(InputButton.Forward, throt.GetAnimParameterFloat("forwardback") > 0.5f);

				Input.SetButton(InputButton.Back, throt.GetAnimParameterFloat("forwardback") < -0.5f);

				Input.SetButton(InputButton.Run, throt.gripped && Input.VR.LeftHand.Trigger.Value > 0.75f);

				Input.SetButton(InputButton.PrimaryAttack, joy.gripped && Input.VR.RightHand.Trigger.Value > 0.75f);
			}
		}
	}
}

