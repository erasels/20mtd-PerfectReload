# PerfectReload
This is a BepInEx plugin for the game 20 Minutes Till Dawn.

You have a (20% of your current reload speed) time window towards the later half of your reload to press the reload key/button which then results in a perfect reload, skipping the rest of it.
For each successive perfect reload you increase the sweet spot range (blue) to a maximum of the normal range which, when triggered gives you a 50% damage boost for 1.5 (+ 0.25 seconds for each chained succes to a maximum of 5) seconds which can stack.
The chain gets reset by failing to perfect reload or letting the gun reload normally.
Failing does nothing other than stopping you from trying again during the current reload.

By default this mod adds f as an additional reload key but this can be changed in the PerfectReload.cfg in ".\Steam\steamapps\common\20MinuteTillDawn\BepInEx\config".

A small gif to show it in action:
https://cdn.discordapp.com/attachments/987507054082162758/997510077433008258/test43.gif