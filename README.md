# UnityLockstep

[![Discord](https://img.shields.io/discord/413156098993029120.svg)](https://discord.gg/F9hJhcX) 


Currently the rollback of adding and changing entities should be stable. If you spawn or navigate entities in multiple game-instances, all simulations will end up in a synced state, no matter how many packets drop or how high the lag is. Clientside prediction makes sure that local commands are executed under any condition.

[![Video](http://img.youtube.com/vi/fDrSTzMjxbQ/0.jpg)](https://youtu.be/fDrSTzMjxbQ "UnityLockstep")

### Architecture
![Overview](/Docs/Overview.svg "Overview")

### Server UI 
![ServerUI](https://i.imgur.com/b7D7J2X.png)

## Getting started

1. Open Server.LiteNetLib/Server.LiteNetLib.sln
2. Run Server-project (Console), enter "1" when asked for room-size
3. Open SampleScene in Unity
4. Hit play and wait until connection to server is established
5. Holding right mouse button will continously spawn new agents, press 'X' to navigate all agents to your current mouse position
   
### Getting started (multiplayer)
1. Build and run the Unityproject
2. Start the server. The roomsize will default to 2 after a few seconds of no input
3. Hit play in Unity so you should now have two instances of the game running
4. Wait until both players are connected to the server. The server will display a message when the simulation has started.
5. For controls, same as above

## Work in progress (ordered by priority)
1. Synchronizing framerate. If a client lags too much or lag compensation is too low, the delayed input will force every other client to constantly rollback the simulation. The project requires a solution to minimize the need of rollback for every client.
1. Performance spikes are currently by design. In case of an input from the very past, a rollback of many (speak: thousands of) ticks at once occur. Adding constraints like max. input-offset and adjusting framerates for slow clients will help improving everyones performance. The target-frame for rollback is also not optimized, this can save lots of performance too.
1. Rollback for despawned (e.g. killed) entities is not implemented yet, it will arrive after navigation, combat/projectiles and health is working.

## Dependencies

- The ECS-Project currently targets .NET Framework 4.6.1 ([#806](https://github.com/sschmid/Entitas-CSharp/issues/806#issuecomment-429578569))
- The server targets .NET Core Framework 2.2.103 [(link to setup)](https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.103-windows-x64-installer)

## References

Inspired by LockstepFramework, in memory of SnpM:
<https://github.com/SnpM/LockstepFramework>

Uses a fork of BEPUPhysics for deterministic physics:
<https://github.com/sam-vdp/bepuphysics1int>

Uses FixedMath.Net for deterministic fp-calculations:
<https://github.com/asik/FixedMath.Net>

Uses Entitas as ECS Framework:
<https://github.com/sschmid/Entitas-CSharp>

The project includes an implementation example using LiteNetLib:
<https://github.com/RevenantX/LiteNetLib>

Initial commit was targeting the following protocol: https://www.reddit.com/r/Unity3D/comments/aewepu/rts_networking_simulate_on_serverbeat/.  The project has shifted to an architecture you see above.

## Limitations

- Physics values are limited to 1000 so keep your world in these limitations or shift the world when your values become too large
