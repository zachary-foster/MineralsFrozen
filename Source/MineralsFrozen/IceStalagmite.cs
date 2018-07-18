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
    /// SaltCrystal class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class IceStalagmite : Minerals.DynamicMineral
    {
        public new static bool isRoofConditionOk(Minerals.ThingDef_StaticMineral myDef, Map map, IntVec3 position)
        {
            // Allow to spawn near roofs
            Predicate<IntVec3> validator = (IntVec3 c) => c.Roofed(map);
            IntVec3 unused;

            if (CellFinder.TryFindRandomCellNear(position, map, 1, validator, out unused))
            {
                return true;
            }

            return Minerals.DynamicMineral.isRoofConditionOk(myDef, map, position);
        }

    }       

}



