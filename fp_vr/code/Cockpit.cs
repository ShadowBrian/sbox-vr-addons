using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    [Prefab]
    public partial class Cockpit : AnimatedEntity
    {
        public static Cockpit FromPrefab(string prefabName)
        {
            if (PrefabLibrary.TrySpawn<Cockpit>(prefabName, out var cockpit))
            {
                return cockpit;
            }

            return null;
        }
    }
}
