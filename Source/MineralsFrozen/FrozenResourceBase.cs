
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
                    return (temp / stackCount);
                }
                else
                {
                    return 0f;
                }
            }
        }

        public override void TickLong()
        {
            // Melt if hot
            if (isMelting)
            {
                float meltDamage = meltRate;
                if (meltDamage < 1)
                {
                    if (Rand.Range(0f, 1f) < meltDamage)
                    {
                        TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1, 0, -1, null, null, null));
                        GenTemperature.PushHeat(this, - meltDamage * attributes.coolRateWhenMelting);
                    }
                    if (stackCount > 1 & Rand.Range(0f, 1f) < meltDamage)
                    {
                        stackCount = stackCount - 1;
                        GenTemperature.PushHeat(this, - meltDamage * attributes.coolRateWhenMelting);
                    }
                }
                else
                {
                    TakeDamage(new DamageInfo(DamageDefOf.Deterioration, (int) Math.Floor(meltDamage), 0, -1, null, null, null));
                    if (stackCount > Math.Floor(meltDamage))
                    {
                        stackCount = stackCount - (int) Math.Floor(meltDamage);
                    }
                    GenTemperature.PushHeat(this, - meltDamage * attributes.coolRateWhenMelting);
                }
            }

            base.TickLong();
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
                if (isMelting)
                {
                    stringBuilder.AppendLine("Melting.");
                }
                else
                {
                    stringBuilder.AppendLine("Frozen.");
                }
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
        public float coolRateWhenMelting = 50f;
    }
}
