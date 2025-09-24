# Damage System

- Hitboxes are a trigger that deal damage when they detect a hurtbox
- Hurtboxes keep track of health

- Adding effects like screenshake, hitstop, hitstun, or knockback can be 
  accomplished by adding to the HitActions when activating a hitbox

## Setup

- Must have layers named 'Hurtbox' and 'Hitbox'
- No gaps in layers (ex. Layer 4 can't be empty if Layer 6 has a word in it)
  - This is so the Layer Enumerator script doesn't get confused and assign things the wrong layer
- Hitbox should ONLY be able to collide with Hurtbox
- Hurtbox should ONLY be able to collide with Hitbox
  - Edit this in Project Settings > Physics > Collision Matrix