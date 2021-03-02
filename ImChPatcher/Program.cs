using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using System.Threading.Tasks;
using Mutagen.Bethesda.FormKeys.SkyrimSE;

namespace ImChPatcher
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .Run(args, new RunPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "ImChPatch.esp",
                        TargetRelease = GameRelease.SkyrimSE,
                    }
                });
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state){
            //Your code here!

            var keyword_IsAdultRace = FormKey.Factory("045AF7:ImCh.esm");
            var keyword_IsChildRace = FormKey.Factory("005901:ImCh.esm");

            foreach (var furnitureGetter in state.LoadOrder.PriorityOrder.Furniture().WinningOverrides()) {

                if (furnitureGetter.InteractionKeyword == Skyrim.Keyword.ActorTypeNPC && !furnitureGetter.MajorFlags.HasFlag(Furniture.MajorFlag.ChildCanUse)) {
                    var furniture = state.PatchMod.Furniture.GetOrAddAsOverride(furnitureGetter);

                    furniture.InteractionKeyword = keyword_IsAdultRace;
                }
            }

            foreach (var raceGetter in state.LoadOrder.PriorityOrder.Race().WinningOverrides()) {

                if (raceGetter.Keywords != null && raceGetter.Keywords.Contains(Skyrim.Keyword.ActorTypeNPC)) {
                    var race = state.PatchMod.Races.GetOrAddAsOverride(raceGetter);

                    if (race.Flags.HasFlag(Race.Flag.Child)) {
                        if (race.Keywords != null) {
                            race.Flags -= Race.Flag.Child;
                            race.Flags |= Race.Flag.AllowPickpocket; 
                            race.Keywords.Add(keyword_IsChildRace);
                       }
                    } else {
                        if (race.Keywords != null) race.Keywords.Add(keyword_IsAdultRace);
                    }
                }
            }
        }
    }
}
