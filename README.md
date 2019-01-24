# UnityLockstep

[![Discord](https://img.shields.io/discord/413156098993029120.svg)](https://discord.gg/F9hJhcX) 

A WIP implementation of the Serverbeat-Protocol described here: https://www.reddit.com/r/Unity3D/comments/aewepu/rts_networking_simulate_on_serverbeat/

![Overview](/Docs/Overview.svg "Overview")

[![Video](http://img.youtube.com/vi/fDrSTzMjxbQ/0.jpg)](https://youtu.be/fDrSTzMjxbQ "UnityLockstep")

## Getting started

1. Open Engine/Lockstep.sln
2. Run Server-project, enter "1" when asked for room-size
3. Open SampleScene in Unity
4. Hit play and wait until connection to server is established
5. Holding right mouse button will continously spawn new agents, press 'X' to navigate all agents to your current mouse position
   
### Getting Multiplayer

1. Build and run the Unityproject
2. Start the server. The roomsize will default to 2 after a few seconds of no input
3. Hit play in Unity so you should now have two instances of the game running
4. Wait until both players are connected to the server. The server will display a message when the simulation has started.
5. For controls, same as above

## Dependencies

- The ECS-Project currently targets .NET Framework 4.6.1 ([#806](https://github.com/sschmid/Entitas-CSharp/issues/806#issuecomment-429578569))
- The server targets .NET Core Framework 2.2.103 [(link to setup)](https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.103-windows-x64-installer)

## References

Inspired by LockstepFramework:
<https://github.com/SnpM/LockstepFramework>

Uses a fork of BEPUPhysics for deterministic physics:
<https://github.com/sam-vdp/bepuphysics1int> 

Uses LiteNetLib as Network-layer:
<https://github.com/RevenantX/LiteNetLib>

Uses FixedMath.Net for deterministic fp-calculations:
<https://github.com/asik/FixedMath.Net>

Uses Entitas as ECS Framework:
<https://github.com/sschmid/Entitas-CSharp>

## Limitations

- Physics values are limited to 1000 so keep your world in these limitations or shift the world when your values become too large
