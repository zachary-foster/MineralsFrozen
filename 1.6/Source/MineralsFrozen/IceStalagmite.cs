using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 
using MineralsFramework;

namespace MineralsFrozen
{
    public class IceStalagmite : MineralsFramework.DynamicMineral
    {
     
        public override void TickLong()
        {
            this.size -= Mathf.Clamp(this.attributes.tempGrowthRateModifer.maxIdeal + this.Position.GetTemperature(this.Map), 0, 100) / 1000;

            base.TickLong();
        }

    }

    /// <summary>
    /// ThingDef_SnowDrift class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_IceStalagmite : MineralsFramework.ThingDef_DynamicMineral
    {

        public override bool isRoofConditionOk(Map map, IntVec3 position)
        {
            if (! position.InBounds(map))
            {
                return false;
            }

            // Allow to spawn near roofs
            Predicate<IntVec3> validator = c => c.InBounds(map) && c.Roofed(map);
            IntVec3 unused;

            if (CellFinder.TryFindRandomCellNear(position, map, 1, validator, out unused))
            {
                return true;
            }

            return base.isRoofConditionOk(map, position);
        }


    }
}



