
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

        public virtual ThingDef_FrozenBlockBase Attributes
        {
            get
            {
                return def as ThingDef_FrozenBlockBase;
            }
        }


        public virtual float CurrentTemp
        {
            get
            {
                return this.AmbientTemperature;
            }
        }

        public virtual bool IsMelting
        {
            get
            {
                return CurrentTemp > Attributes.meltTemp;
            }
        }

        public virtual bool IsHealing
        {
            get
            {
                return CurrentTemp < Attributes.healTemp && CanHeal;
            }
        }

        public virtual bool CanHeal
        {
            get
            {
                return (HitPoints < MaxHitPoints) && (HitPoints >= Math.Ceiling(MaxHitPoints * Attributes.maxHealHP));
            }
        }

        public virtual float StackSizeMeltFactor
        {
            get
            {
                // Models the proportion of surface exposed if stack was a cube
                return (float) Math.Pow(stackCount, - 1f / 3f);
            }
        }

        public virtual float CurrentMeltRate
        {
            get
            {
                // Calculate base melt rate
                float temp = CurrentTemp;
                float rate = 0f;
                if (temp > Attributes.meltTemp)
                {
                    rate = ((temp - Attributes.meltTemp) / 20) * Attributes.meltRate;
                }
                if (temp < Attributes.healTemp && CanHeal)
                {
                    rate = -((Attributes.healTemp - temp) / 20) * Attributes.healRate;
                }

                // Adjust for stack amount
                rate *= StackSizeMeltFactor;

                // Dont change too much per tick
                if (Math.Abs(rate) > Attributes.maxChangeRate)
                {
                    rate = Attributes.maxChangeRate * Math.Sign(rate);
                }

                return rate;
            }
        }

        public virtual float ExposedHitPoints()
        {
            return ((1 / StackSizeMeltFactor) * ((float)HitPoints / (float)MaxHitPoints - 1)) + 1;
        }

        public override void TickLong()
        {

            float rate = CurrentMeltRate;
            float hitPointProp = (float) HitPoints / (float) MaxHitPoints;
            float rateInHitPoints = 0f;
            float rateInCount = 0f;
            float tempChange = 0f;

            // Find melt/freeze rate in hit points and stack count
            if (rate > 0f) // is melting
            {
                // calculate stack count and hit points lost
                rateInCount = (rate * (float) stackCount) / hitPointProp;
                float decimalPart = rateInCount - (float)Math.Floor(rateInCount);
                rateInCount = (float)Math.Floor(rateInCount);
                rateInHitPoints = decimalPart * MaxHitPoints / stackCount;
                if (Math.Abs(rateInHitPoints) < 1)
                {
                    if (Rand.Range(0f, 1f) < Math.Abs(rateInHitPoints))
                    {
                        rateInHitPoints = Math.Sign(rateInHitPoints);
                    }
                    else
                    {
                        rateInHitPoints = 0f;
                    }
                }

                // Calculate temperature change
                if (rateInCount > 0f || rateInHitPoints > 0f)
                {
                    float tempChangeFromHitPoints = (rateInHitPoints / MaxHitPoints) * Attributes.maxStoredHeat * stackCount;
                    float tempChangeFromStackCount = rateInCount * Attributes.maxStoredHeat * hitPointProp;
                    tempChange = tempChangeFromHitPoints + tempChangeFromStackCount;
                    GenTemperature.PushHeat(this, -tempChange);
                }
            }
            else if (rate < 0f) // if freezing
            {
                // calculate hit points gained
                rateInCount = 0f; // cant gain items back 
                rateInHitPoints = rate * MaxHitPoints;
                if (Math.Abs(rateInHitPoints) < 1)
                {
                    if (Rand.Range(0f, 1f) < Math.Abs(rateInHitPoints))
                    {
                        rateInHitPoints = Math.Sign(rateInHitPoints);
                    }
                    else
                    {
                        rateInHitPoints = 0f;
                    }
                }
                else
                {
                    rateInHitPoints = (float)Math.Floor(rateInHitPoints);
                }

                // Dont heal past max health
                float newHitpoints = HitPoints - rateInHitPoints;
                if (newHitpoints > MaxHitPoints)
                {
                    rateInHitPoints = MaxHitPoints - HitPoints;
                }

                // Calculate temperature change
                if (Math.Abs(rateInHitPoints) > 0)
                {
                    tempChange = (rateInHitPoints / MaxHitPoints) * Attributes.maxStoredHeat * stackCount;
                }
            }

            // Apply temperature change
            if (Math.Abs(tempChange) > 0)
            {
                GenTemperature.PushHeat(this, -tempChange);
            }

            // Apply hit point change
            if (Math.Abs(rateInHitPoints) > 0)
            {
                TakeDamage(new DamageInfo(DamageDefOf.Deterioration, rateInHitPoints, 0, -1, null, null, null));
            }

            // Adjust stack count for simulated hit points of exposed items
            if (ExposedHitPoints() <= 0)
            {
                int exposedStackCount = (int)Math.Floor(stackCount * StackSizeMeltFactor);
                rateInCount += exposedStackCount;
                HitPoints = MaxHitPoints;
             }

            // Apply stack count change
            if (rateInCount > 0)
            {
                stackCount -= (int)rateInCount;
            }

            // Dont go below 1 stack count
            if (stackCount < 1)
            {
                stackCount = 1;
            }

            //base.TickLong();
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Melt Rate: " + CurrentMeltRate.ToStringPercent());
            stringBuilder.AppendLine("Exposed portion health: " + ExposedHitPoints().ToStringPercent());
            if (IsMelting)
            {
                stringBuilder.AppendLine("Melting.");
            }
            else if (IsHealing)
            {
                stringBuilder.AppendLine("Freezing.");
            }
            else
            {
                stringBuilder.AppendLine("Frozen.");
            }
            if (DebugSettings.godMode)
            {
                stringBuilder.AppendLine("Stack size factor: " + StackSizeMeltFactor);
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
        // Maximum stable temperature
        public float meltTemp = 1f;
        // Minimum temperature the wall will heal
        public float healTemp = -1f;
        // Minimum proportion of hit point needed to heal
        public float maxHealHP = 0.95f;
        // The proportion of health restored each tick at 20C below heal temperature
        public float healRate = 0.1f;
        // The proportion of health lossed each tick at 20C above melt temperature
        public float meltRate = 0.1f;
        // The  maximum amount of change per tick (to stop large temperature changes in small rooms)
        public float maxChangeRate = 0.2f;
        // The difference in stored energy between the solid and liquid
        public float maxStoredHeat = 1000f;
    }
}
