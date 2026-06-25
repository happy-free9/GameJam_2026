# Core Prefab Setup Notes

Create these prefabs in the Unity Editor when the team is ready:

- `Player_Core.prefab`
- `DoorTransition.prefab`
- `SpawnPoint.prefab`
- `InteractionTarget.prefab`
- `CoreUI.prefab`
- `ChaseAgent.prefab`

Recommended contents:

## Player_Core.prefab

- `SpriteRenderer`
- `Rigidbody2D`
- `Collider2D`
- `TopDownPlayerController`
- `PlayerInteractor`

## DoorTransition.prefab

- `Collider2D` with `Is Trigger` enabled
- `InteractionTarget`
- `SceneTransitionTrigger`

## SpawnPoint.prefab

- `SpawnPoint`

## InteractionTarget.prefab

- `Collider2D` with `Is Trigger` enabled
- `InteractionTarget`

## CoreUI.prefab

- `CoreUIRoot`

## ChaseAgent.prefab

- `SpriteRenderer`
- `Collider2D`
- `SimpleChaseAgent2D`
