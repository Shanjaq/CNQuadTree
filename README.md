# CNQuadTree - Cardinal Neighbor Quad tree
1. contains a public rect that sets its boundaries
2. contains a public list of Probes
3. contains a private tree of Quadrants
4. is generic for objects contained by quadrants

# Quadrant
1. contains a rect with its lower left corner at location that shows its Size and Position
2. contains an object that describes its location
3. is generic for contained object
4. is enumerable

# Probe
1. contains a rect centered on its location that shows its Size and Position
2. contains a rect that includes all intersecting quadrants that shows its Domain
3. contains a ref to the object in the quadrant with its center nearest the probe Position that describes its location
4. provides iterator for neighbor rects of the same size with objects in the W, N, E, S directions
5. provides a generic method to update objects describing quadrant locations
6. provides events for entering and leaving quadrants
7. internally handles quadrant creation using supplied update method
8. can be translated to any position within the boundaries of the quadtree
9. is generic for object manipulation

# Iterator
1. contains a public rect that describes its size and location
2. contains a public object ref that describes its location
3. contains public refs to neighbor iterators moving outward from W, N, E, S
4. contains public ref to contained iterators moving inward from W, N, E, S
5. is generic for object selection
