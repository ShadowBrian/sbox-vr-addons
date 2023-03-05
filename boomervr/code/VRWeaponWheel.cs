using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Facepunch.Boomer.WeaponSystem;
using Sandbox;

namespace Facepunch.Boomer
{
    public partial class VRWeaponWheel : AnimatedEntity
    {
        List<ModelEntity> WeaponOptions = new List<ModelEntity>();

        Vector3[] itemPositions;

        public Vector3 centerPosition;
        public float radius = 10f;
        public float angularWidth = MathF.PI / 3f;
        public float hoverScaleFactor = 1.25f;

        public int activeItemIndex = -1;

        public Player AssociatedPlayer;

        public void InitWeapons(Player pl)
        {
            AssociatedPlayer = pl;
            for (int i = 0; i < 6; i++)
            {
                var Weapon = new ModelEntity();
                Weapon.SetModel(pl.Inventory.GetSlot(i).Model.ResourcePath);
                //Weapon.SetParent(this);
                WeaponOptions.Add(Weapon);
            }

            CalculateItemPositions();
        }

        private void CalculateItemPositions()
        {
            itemPositions = new Vector3[WeaponOptions.Count];

            for (int i = 0; i < WeaponOptions.Count; i++)
            {
                float angle = 2 * MathF.PI / (WeaponOptions.Count) * i;
                Vector3 itemPosition = new Vector3(0f, MathF.Cos(angle), MathF.Sin(angle)) * radius;
                itemPositions[i] = itemPosition;
                WeaponOptions[i].Position = itemPosition;
            }
        }

        [Event.Client.Frame]
        public void Frame()
        {
            if (EnableDrawing)
            {
                CalculateItemPositions();
                for (int i = 0; i < WeaponOptions.Count; i++)
                {
                    if (AssociatedPlayer.Inventory.GetSlot(i)?.GetComponent<Ammo>(true).AmmoCount < 1)
                    {

                        Vector3 targetPosition = itemPositions[i];
                        WeaponOptions[i].Position = Transform.PointToWorld(targetPosition);// targetPosition;// Vector3.Lerp(WeaponOptions[i].LocalPosition, targetPosition, Time.Delta * 10f);
                        WeaponOptions[i].Rotation = Rotation;

                        if (i == activeItemIndex)
                        {
                            //DebugOverlay.Sphere(Transform.PointToWorld(targetPosition), 2f, Color.Green);
                            WeaponOptions[i].Scale = MathX.Lerp(WeaponOptions[i].Scale, 0.1f * hoverScaleFactor, Time.Delta * 10f);
                        }
                        else
                        {
                            //DebugOverlay.Sphere(Transform.PointToWorld(targetPosition), 1f, Color.Green);
                            WeaponOptions[i].Scale = MathX.Lerp(WeaponOptions[i].Scale, 0.1f, Time.Delta * 10f);
                        }
                    }
                    else
                    {

                        Vector3 targetPosition = itemPositions[i];
                        WeaponOptions[i].Position = Transform.PointToWorld(targetPosition);// targetPosition;// Vector3.Lerp(WeaponOptions[i].LocalPosition, targetPosition, Time.Delta * 10f);
                        WeaponOptions[i].Rotation = Rotation;

                        if (i == activeItemIndex)
                        {
                            //DebugOverlay.Sphere(Transform.PointToWorld(targetPosition), 2f, Color.Green);
                            WeaponOptions[i].Scale = MathX.Lerp(WeaponOptions[i].Scale, 0.25f * hoverScaleFactor, Time.Delta * 10f);
                        }
                        else
                        {
                            //DebugOverlay.Sphere(Transform.PointToWorld(targetPosition), 1f, Color.Green);
                            WeaponOptions[i].Scale = MathX.Lerp(WeaponOptions[i].Scale, 0.25f, Time.Delta * 10f);
                        }
                    }

                }

                Vector3 handPosition = Input.VR.RightHand.Transform.Position;
                if (Vector3.DistanceBetween(Position, handPosition) > radius / 2f)
                {
                    int closestIndex = 0;
                    float closestDistance = Vector3.DistanceBetween(handPosition, WeaponOptions[0].Transform.Position);

                    for (int i = 1; i < itemPositions.Length; i++)
                    {
                        float distance = Vector3.DistanceBetween(handPosition, WeaponOptions[i].Transform.Position);

                        if (distance < closestDistance)
                        {
                            closestIndex = i;
                            closestDistance = distance;
                        }
                    }

                    SetActiveItem(closestIndex);
                }
                else
                {
                    activeItemIndex = -1;
                }
            }
            else
            {
                for (int i = 0; i < WeaponOptions.Count; i++)
                {
                    WeaponOptions[i].LocalPosition = Vector3.Lerp(WeaponOptions[i].LocalPosition, Vector3.Zero, Time.Delta * 10f);
                    WeaponOptions[i].Scale = MathX.Lerp(WeaponOptions[i].Scale, 0.25f, Time.Delta * 10f);
                }
            }
        }

        public void SetActiveItem(int index)
        {
            if (index < 0 || index >= WeaponOptions.Count)
            {
                return;
            }

            activeItemIndex = index;
        }

        [Event.Tick]
        public void Simulate()
        {

        }
    }
}
