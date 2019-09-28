
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
    /// FrozenBlockBase class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class FrozenBlockBase : ThingWithComps
    {

        public virtual ThingDef_FrozenBlockBase attributes
        {
            get
            {
                return def as ThingDef_FrozenBlockBase;
            }
        }


        public virtual bool isMelting
        {
            get
            {
                float temp = Position.GetTemperature(Map);
                return temp > 0;
            }
        }

        public virtual float meltRate
        {
            get
            {
                if (isMelting)
                {
                    float temp = Position.GetTemperature(Map);
                    return temp / 30;
                }
                else
                {
                    return 0f;
                }
            }
        }

        public override void TickRare()
        {
            // Melt if hot
            if (isMelting)
            {
                float meltDamage = meltRate;
                if (meltDamage < 1 & Rand.Range(0f, 1f) < meltDamage)
                {
                    TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1, 0, -1, null, null, null));
                }
                else
                {
                    TakeDamage(new DamageInfo(DamageDefOf.Deterioration, (int) Math.Floor(meltDamage), 0, -1, null, null, null));

                }
            }

            base.TickRare();
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (DebugSettings.godMode)
            {
                stringBuilder.AppendLine("Melt Rate: " + meltRate);
            }
            else
            {
                stringBuilder.AppendLine("Melting.");
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

    }       


    /// <summary>
    /// ThingDef_FrozenBlockBase class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_FrozenBlockBase : ThingDef
    {

    }
}
