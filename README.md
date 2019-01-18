# UnityLockstep

[![Discord](https://img.shields.io/discord/413156098993029120.svg)](https://discord.gg/F9hJhcX) 

A WIP implementation of the Serverbeat-Protocol described here: https://www.reddit.com/r/Unity3D/comments/aewepu/rts_networking_simulate_on_serverbeat/

[![Video](http://img.youtube.com/vi/bNwlnO4BzFw/0.jpg)](https://www.youtube.com/watch?v=bNwlnO4BzFw "UnityLockstep")

## Getting started

1. Open Engine/Lockstep.sln
2. Run Server-project
3. Open SampleScene in Unity
4. Hit play and click on connect button
   
### Getting Multiplayer

1. Build and run the Unityproject
2. Set the servers' roomsize to 2 (currently done by code in Program.cs)
3. Hit play in Unity so you should now have two instances of the game running
4. Connect both instances to the server
5. Right click on the terrain moves all spawned cubes on both players

## Dependencies

- The server targets .NET Core Framework 2.2

## References

Uses parts of LockstepFramework:
<https://github.com/SnpM/LockstepFramework>

Uses a fork of BEPUPhysics for deterministic physics:
<https://github.com/sam-vdp/bepuphysics1int> 

Uses LiteNetLib as Network-layer:
<https://github.com/RevenantX/LiteNetLib>

Uses FixedMath.Net for deterministic fp-calculations:
<https://github.com/asik/FixedMath.Net>

## Limitations

- Physics values are limited to 1000 so keep your world small or shift the world when your values become too large