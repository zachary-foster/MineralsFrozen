
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


        public virtual float currentTemp
        {
            get
            {
                return this.AmbientTemperature;
            }
        }

        public virtual bool isMelting
        {
            get
            {
                return currentTemp > 0;
            }
        }

        public virtual float meltRate
        {
            get
            {
                if (isMelting)
                {
                    return (currentTemp / stackCount) * attributes.meltRateFactor;
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
                        GenTemperature.PushHeat(this, -meltDamage * attributes.coolRateWhenMelting);
                        TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 1, 0, -1, null, null, null));
                    }
                    if (stackCount > 1 & Rand.Range(0f, 1f) < meltDamage)
                    {
                        GenTemperature.PushHeat(this, -meltDamage * attributes.coolRateWhenMelting);
                        stackCount = stackCount - 1;
                    }
                }
                else
                {
                    GenTemperature.PushHeat(this, -meltDamage * attributes.coolRateWhenMelting);
                    TakeDamage(new DamageInfo(DamageDefOf.Deterioration, (int) Math.Floor(meltDamage), 0, -1, null, null, null));
                    if (stackCount > Math.Floor(meltDamage))
                    {
                        stackCount = stackCount - (int) Math.Floor(meltDamage);
                    }
                }
            }

            //base.TickLong();
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
        public float coolRateWhenMelting = 10f;
        public float meltRateFactor = 2f;
    }
}
