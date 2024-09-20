## Dots shooter

The code is structured in a few different systems:
- GameSystem
- PlayerSystem
- EnemySystem
- BulletSystem
- CollisionSystem

I made the game with performance in mind. There are no dynamic data structures or allocations, just plain-old-data components and systems that iterate through the data each frame. For collision detection, I implemented a spatial acceleration structure grid, which efficiently looks through collision pairs per grid cell instead of naively looping through all possible entity pairs. This collision-detection grid uses the temporary allocator which is efficient, because it groups allocations together each frame instead of having many small allocations which would fragment the heap and would make lookup more inefficient for the CPU cache.

