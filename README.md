# Unity Simple Pooling System
An easy to implement pooling system. Meant for those *"in hindsight I should have used that"* moments. It uses dictionaries to keep track of the pools and the objects.

This was made as a "one size fits all approach" needless to say, it is faster than Instantiate-then-Destroy, but not as fast as a custom system.

> The main purpose here is a system that works with minimal changes to
> the rest of the code, `Instantiate()` calls can be swapped easily by
> `PoolManager.Allocate()`, and the same goes for `Destroy()` and
> `PoolManager.Deallocate()`

## Usage
- **Allocate** from the pool using `PoolManager.Allocate()`, in place of instantiate, no need for those pesky pool managing things or convoluted calls:
- ![Allocation Example](https://github.com/Woreira/Unity-Pooling-System/blob/main/Snippets/Captura%20de%20tela%202022-02-09%20121514.png)
- **Deallocate** from the pool using `PoolManager.Deallocate()`:
- ![Deallocation Example](https://github.com/Woreira/Unity-Pooling-System/blob/main/Snippets/Captura%20de%20tela%202022-02-09%20121614.png)
- Use `PoolManager.ClearAllPools()` to clear all pools *(duh)*, it calls `PoolManager.Deallocate(`) on every pooled obj. This prevents bugs especially on scene changes and level wipes (like having to wipe all enemies from the scene on gameover, for example);

Liked the repo? Give it a star then!
