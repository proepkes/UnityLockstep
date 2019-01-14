# UnityLockstep

A WIP implementation of the Serverbeat-Protocol described here: https://www.reddit.com/r/Unity3D/comments/aewepu/rts_networking_simulate_on_serverbeat/

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