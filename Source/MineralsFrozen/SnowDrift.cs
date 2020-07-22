using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 
using Verse.AI.Group;

namespace MineralsFrozen
{
    /// <summary>
    /// SaltCrystal class
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class SnowDrift : Minerals.DynamicMineral
    {

        public override float GrowthRate
        {
            get
            {
                return attributes.GrowthRateAtPos(Map, Position);
            }
        }

            
    }


    /// <summary>
    /// ThingDef_SnowDrift class.
    /// </summary>
    /// <author>zachary-foster</author>
    /// <permission>No restrictions</permission>
    public class ThingDef_Snow : Minerals.ThingDef_DynamicMineral
    {

        // How much faster it melts in still water
        public float stillWaterMeltFactor = 5f;
        // How much faster it melts in moving water
        public float movingWaterMeltFactor = 30f;
        // How much faster it melts in moving water
        public float rainMeltFactor = 10f;


        public override void InitNewMap(Map map, float scaling = 1)
        {
            float snowProb = 1f;
            const float minTemp = 3f;

            // Only spawn snow if it is cold out
            if (map.mapTemperature.SeasonalTemp < minTemp)
            {
                snowProb = snowProb * (float)Math.Sqrt(minTemp - map.mapTemperature.SeasonalTemp) / 5;
            }
            else
            {
                snowProb = 0f;
            }

            // Scale by rain amount
            snowProb = snowProb * map.TileInfo.rainfall / 1000;

//            Log.Message("Minerals: snow scaling due to temp/precip: " + snowProb);
            base.InitNewMap(map, snowProb);
        }

        public virtual float growthRateFactor(IntVec3 aPosition, Map aMap, float rate)
        {
            float factor = 1f;

           
            if (rate < 0)
            {
                // Melts faster in water
                if (aMap.terrainGrid.TerrainAt(aPosition).defName.Contains("Water"))
                {
                    if (aMap.terrainGrid.TerrainAt(aPosition).defName.Contains("Moving"))
                    {

                        factor = factor * movingWaterMeltFactor; // melt even faster in moving water
                    }
                    else
                    {
                        factor = factor * stillWaterMeltFactor;
                    }
                }

                // Melts faster in rain
                if (! aMap.roofGrid.Roofed(aPosition))
                {
                    factor = factor * (1 + aMap.weatherManager.curWeather.rainRate * rainMeltFactor);
                }
            }

            return factor;
        }

        public override float GrowthRateAtPos(Map aMap, IntVec3 aPosition)
        {
            float rate = base.GrowthRateAtPos(aMap, aPosition);
            return rate * growthRateFactor(aPosition, aMap, rate);
        }

    }

    public class ThingDef_DeepSnow : ThingDef_Snow
    {
        // How much each nearby obstruction reduces growth rate. The effect is multiplicative
        public float obstructionGrowthFactor = 0.5f;
        // The maximum radius obstructions are looked for
        public int obstructionSearchRadius = 1;


        public override bool PlaceIsBlocked(Map map, IntVec3 position)
        {
//            Log.Message("PlaceIsBlocked: deep");
            if (! position.InBounds(map))
            {
                return true;
            }

            // dont spawn on big things
            if (! position.Standable(map))
            {
                return true;
            }

            // dont spawn  next to stuff
            if (Rand.Range(0f, 1f) > obstructionGrowthRateFactor(position, map))
            {
                return true;
            }
                
            return base.PlaceIsBlocked(map, position);
        }

        public virtual float obstructionGrowthRateFactor(IntVec3 aPosition, Map aMap)
        {
            float factor = 1f;

            // Nearby Buldings slow growth
            for (int xOffset = -obstructionSearchRadius; xOffset <= obstructionSearchRadius; xOffset++)
            {
                for (int zOffset = -obstructionSearchRadius; zOffset <= obstructionSearchRadius; zOffset++)
                {
                    IntVec3 checkedPosition = aPosition + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(aMap))
                    {
                        foreach (Thing thing in aMap.thingGrid.ThingsListAt(checkedPosition))
                        {
                            if ((thing is Building && thing.def.altitudeLayer == AltitudeLayer.Building) || aMap.roofGrid.Roofed(checkedPosition) || (thing is Plant && ! checkedPosition.Standable(aMap)))
                            {
                                factor = factor * obstructionGrowthFactor;
                            }

                        }
                    }
                }
            }

            return factor;

        }

        public override float growthRateFactor(IntVec3 aPosition, Map aMap, float rate)
        {
            float factor = base.growthRateFactor(aPosition, aMap, rate);

            // Nearby Buldings slow growth  and melting
            factor = factor * obstructionGrowthRateFactor(aPosition, aMap);

            return factor;
        }


    }

    public class ThingDef_SnowDrift : ThingDef_Snow
    {
        // How much each nearby obstruction increases growth rate. The effect is additive
        public float obstructionGrowthBonus = 0.25f;
        // The maximum radius obstructions are looked for
        public int obstructionSearchRadius = 1;

        public override bool PlaceIsBlocked(Map map, IntVec3 position)
        {
//            Log.Message("PlaceIsBlocked: drift");
            if (! position.InBounds(map))
            {
                return true;
            }
//            Log.Message("PlaceIsBlocked: drift in bounds");

            // Cant spawn on water
            if (position.GetTerrain(map).defName.Contains("Water"))
            {
                return true;
            }
//            Log.Message("PlaceIsBlocked: drift not water");

            // dont spawn in the open
            if (Rand.Range(0f, 1f) > obstructionGrowthRateFactor(position, map))
            {
                return true;
            }


            //            Log.Message("PlaceIsBlocked: drift not blocked");

            return base.PlaceIsBlocked(map, position);
        }

        public virtual float obstructionGrowthRateFactor(IntVec3 aPosition, Map aMap)
        {
            float factor = 0f;

            // Nearby Buldings or other snow drifts allow growth
            for (int xOffset = -obstructionSearchRadius; xOffset <= obstructionSearchRadius; xOffset++)
            {
                for (int zOffset = -obstructionSearchRadius; zOffset <= obstructionSearchRadius; zOffset++)
                {
                    IntVec3 checkedPosition = aPosition + new IntVec3(xOffset, 0, zOffset);
                    if (checkedPosition.InBounds(aMap))
                    {
                        foreach (Thing thing in aMap.thingGrid.ThingsListAt(checkedPosition))
                        {
                            if ((thing is Building && thing.def.altitudeLayer == AltitudeLayer.Building) || thing.def is ThingDef_SnowDrift)
                            {
                                factor = factor + obstructionGrowthBonus;
                            }

                        }
                    }
                }
            }

            return factor;

        }

        public override float growthRateFactor(IntVec3 aPosition, Map aMap, float rate)
        {
            float factor = base.growthRateFactor(aPosition, aMap, rate);

            // Nearby Buldings slow growth  and melting
            factor = factor * obstructionGrowthRateFactor(aPosition, aMap);

            return factor;
        }

    }
}


