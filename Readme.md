## Dots shooter

The code is structured in a few different systems:
- GameSystem
- PlayerSystem
- EnemySystem
- BulletSystem
- CollisionSystem

I made built the game with performance in mind. There are no dynamic data structures or allocations, just plain-old-data components and systems that iterate through the data each frame. For collision detection, I implemented a spatial acceleration structure grid, which efficiently looks through collision pairs per grid cell instead of naively looping through all possible entity pairs. Using temporary allocator is efficient, because it groups allocations together instead of having many fragmented allocations which would be inefficient for the CPU cache.

