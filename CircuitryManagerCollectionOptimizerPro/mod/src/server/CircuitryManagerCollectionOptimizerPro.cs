using HarmonyLib;
using JimmysUnityUtilities;
using LogicAPI.Data;
using LogicAPI.Server;
using LogicAPI.Services;
using LogicWorld.Server.Circuitry;
using Microsoft.Extensions.Hosting;
using ServerOnlyMods.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GHXX_CircuitryManagerCollectionOptimizerPro.Server
{
    public class CircuitryManagerCollectionOptimizerPro : ServerMod, IServerSideOnlyMod
    {
        protected override void Initialize()
        {
            Logger.Info("CircuitryManagerCollectionOptimizerPro mod loading...");

            var harmonyInstance = new Harmony("ghxx.circuitrymanagercollectionoptimizerpro");

            Logger.Info($"Creating delegate to access private function CircuitryManager.LookupPeg(PegAddress)...");
            IHost Host = (IHost)typeof(LogicWorld.Server.Program).GetField("Host", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            ICircuitryManager circuitryMgr = (ICircuitryManager)Host.Services.GetService(typeof(ICircuitryManager));
            var dtype = typeof(Func<PegAddress, Peg>);
            CircuitryManagerPatches.LookupPegPriv = typeof(CircuitryManager).GetMethod("LookupPeg", BindingFlags.NonPublic | BindingFlags.Instance)
                .CreateDelegate(dtype, circuitryMgr) as Func<PegAddress, Peg>;

            Logger.Info($"Brewing up the patch...");
            harmonyInstance.PatchAll();
            Logger.Info($"Imagine, CircuitryManagerCollectionOptimizerPro has just brewed up a faster CircuitryManager.GetPegsInGroup(Peg) method!");
        }

        [HarmonyPatch]
        public class CircuitryManagerPatches
        {
            // SEEMS TO NOT WORK ON LINUX, BUT ONLY ON WINDOWS?
            //[HarmonyReversePatch]
            //[HarmonyPatch(typeof(CircuitryManager), "LookupPeg")]
            //[MethodImpl(MethodImplOptions.NoInlining)]
            //public static Peg LookupPeg_DUMMY(object instance, PegAddress pAddress) { throw new NotImplementedException("this is a harmony stub"); }

            public static Func<PegAddress, Peg> LookupPegPriv = null;

            [HarmonyPatch(typeof(CircuitryManager), "GetPegsInGroup", new Type[] { typeof(Peg) })]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static bool Prefix(Peg peg, CircuitryManager __instance, IWorldData ___WorldData, ref (InputPeg[] Inputs, OutputPeg[] Outputs) __result)
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
                                    AddPegAndNeighbors(LookupPegPriv(wire.Point1));
                                    AddPegAndNeighbors(LookupPegPriv(wire.Point2));
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