# LogicWorld-CircuitryManagerCollectionOptimizerPro (for servers)
Improves a recursive local method located in `LogicWorld.Server.Circuitry.CircuitryManager.GetPegsInGroup(Peg)` by swapping the two List<T>'s, which are located in the parent method, out for two HashSet<T>'s and reordering some checks.
This yields a huge circuit-paste speedup in v0.90.3, and further optimizes world loading times, ontop of Ecconia's [server load accelerator](https://github.com/Ecconia/Ecconia-LogicWorld-Mods/tree/master/ServerLoadAccelerator), which is highly recommended.

This mod REPLACES the mentioned method (`LogicWorld.Server.Circuitry.CircuitryManager.GetPegsInGroup(Peg)`). Any mods that edit the same method may be incompatible.

# Install
Put the contents of the "mod" directory into a new folder inside your server's GameData folder.

You will also need to install the following dependencies: 
* "ServerOnlyMods" mod made by Ecconia: https://github.com/Ecconia/Ecconia-LogicWorld-Mods/tree/master/ServerOnlyMods
* "HarmonyForServers" mod made by Ecconia: https://github.com/Ecconia/Ecconia-LogicWorld-Mods/tree/master/HarmonyForServers
