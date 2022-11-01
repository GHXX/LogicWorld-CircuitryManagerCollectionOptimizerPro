using HarmonyLib;
using JimmysUnityUtilities;
using LogicAPI.Data;
using LogicAPI.Server;
using LogicAPI.Services;
using LogicWorld.Server.Circuitry;
using ServerOnlyMods.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GHXX_CircuitryManagerCollectionOptimizerPro.Server
{
    public class CircuitryManagerCollectionOptimizerPro : ServerMod, IServerSideOnlyMod
    {
        protected override void Initialize()
        {
            Logger.Info("CircuitryManagerCollectionOptimizerPro mod loading...");

            var harmonyInstance = new Harmony("ghxx.circuitrymanagercollectionoptimizerpro");
            Logger.Info($"Brewing up the patch...");
            harmonyInstance.PatchAll();
            Logger.Info($"Congratulations: CircuitryManagerCollectionOptimizerPro has just optimized the CircuitryManager.GetPegsInGroup(Peg) method!");
        }

        [HarmonyPatch]
        public class CircuitryManagerPatches
        {
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(CircuitryManager), "LookupPeg")]
            [MethodImpl(MethodImplOptions.NoInlining)]
            private static Peg LookupPeg_DUMMY(CircuitryManager instance, PegAddress pAddress) { return null; }

            [HarmonyPatch(typeof(CircuitryManager), "GetPegsInGroup", new Type[] { typeof(Peg) })]
            private static bool Prefix(Peg peg, CircuitryManager __instance, IWorldData ___WorldData, ref (InputPeg[] Inputs, OutputPeg[] Outputs) __result)
            {
                var inputs = new HashSet<InputPeg>(Config.InitialHashsetSizePrime);
                var outputs = new HashSet<OutputPeg>(Config.InitialHashsetSizePrime);
                void AddPegAndNeighbors(Peg boi)
                {
                    if (boi != null)
                    {
                        if (boi is InputPeg inputPeg)
                        {
                            if (inputs.Add(inputPeg))
                            {
                                foreach (WireAddress item2 in ___WorldData.LookupPegWires(inputPeg.Address).OrEmptyIfNull())
                                {
                                    Wire wire = ___WorldData.Lookup(item2);
                                    AddPegAndNeighbors(LookupPeg_DUMMY(__instance, wire.Point1));
                                    AddPegAndNeighbors(LookupPeg_DUMMY(__instance, wire.Point2));
                                }

                                foreach (InputPeg item3 in inputPeg.SecretLinks.OrEmptyIfNull())
                                {
                                    AddPegAndNeighbors(item3);
                                }
                            }
                            return;
                        }
                        if (!(boi is OutputPeg item))
                        {
                            throw new Exception("[CircuitryManagerCollectionOptimizerPro] A peg seems to be neither an output nor an input. That is very odd...");
                        }
                        outputs.Add(item);
                    }
                }

                AddPegAndNeighbors(peg);

                __result = (inputs.ToArray(), outputs.ToArray());

                return false; // we need to skip the original function; THIS IS A FULL REPLACEMENT!
            }
        }
    }
}