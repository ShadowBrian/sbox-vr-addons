using Sandbox;
using Sandbox.UI;
using System.ComponentModel;

namespace Facepunch.Boomer
{
    public class VRHider : EntityComponent
    {
        [Event.Client.Frame]
        public void Update()
        {
            if (Entity != null && Entity.Owner != null)
            {
                if (Entity.Owner == Game.LocalPawn)
                {
                    Entity.EnableDrawing = false;
                }
                else
                {
                    Entity.EnableDrawing = true;

                    if (Vector3.DistanceBetween(Entity.Position, Game.LocalPawn.Position) < 20f)
                    {
                        (Entity as ModelEntity).RenderColor = Color.White.WithAlpha(0.25f);
                    }
                    else
                    {
                        (Entity as ModelEntity).RenderColor = Color.White.WithAlpha(1f);
                    }
                }
            }
        }
    }
}
