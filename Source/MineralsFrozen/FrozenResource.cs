
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 

namespace MineralsFrozen
{
    /// <summary>
    /// FrozenResource class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class FrozenResource : ThingWithComps
    {

        public virtual ThingDef_FrozenResource attributes
        {
            get
            {
                return def as ThingDef_FrozenResource;
            }
        }


        public override void TickRare()
        {
            // Melt if hot
            float temp = Position.GetTemperature(Map);
            if (temp > 0)
            {
                float meltDamage = temp / 20;
                if (meltDamage < 1 & Rand.Range(0f, 1f) < meltDamage)
                {
                    TakeDamage(new DamageInfo(DamageDefOf.Rotting, 1, -1, null, null, null));
                }
                else
                {
                    TakeDamage(new DamageInfo(DamageDefOf.Rotting, (int) Math.Floor(meltDamage), -1, null, null, null));

                }
            }

            base.TickRare();
        }

    }       


    /// <summary>
    /// ThingDef_FrozenResource class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_FrozenResource : ThingDef
    {

    }
}
