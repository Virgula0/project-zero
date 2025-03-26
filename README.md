# Project Zero

**Game developed for the Game Development course at Unifi.**

## Brief Description:
[Project Zero] is a top-down 2D shooter that focuses on parrying, shooting, and fast-paced gameplay. Similar games include *Hotline Miami 1 & 2*, *Door Kickers*, and *Nuclear Throne*.

## Setting:
The game is set in the near future, aboard a space station orbiting Jupiter. A twisted competition, similar to a Hunger Games scenario, is played and streamed to the Metanet (or Hypernet, if we want a more serious tone). You are one of the competitors, and your objective is to survive, escape, and collect as much cash as possible. To succeed, you’ll need to paint the station red with blood.

## Game Mechanics:

### Movement:
- **Movement:** Standard top-down movement, using `WASD` for movement and the mouse to aim.
- **Dash:** Press [Shift] to dash a short distance. The dash direction is determined by the mouse pointer.
- **Jump:** There is no jump mechanic.

### Weapons:
Weapons are divided into **Primary** and **Secondary** categories. Enemies can use any **Primary** weapon but are limited to a subset of **Secondary** weapons.

#### Primary Weapons:
Primary weapons are your main damage-dealers, and they are subdivided into **Ranged** and **Melee** weapons.

##### Ranged Weapons:
Ranged weapons are effective at long range but have limited ammunition.

**List of Ranged Weapons:**

- **Pistol**:  
  - Firing Rate: High  
  - Stopping Power: Low  
  - Magazine Size: 15  
  - Base Spread: Low  
  - Kill on Throw: No
  
- **SMG**:  
  - Firing Rate: Very High  
  - Stopping Power: Very Low  
  - Magazine Size: 50  
  - Base Spread: High  
  - Kill on Throw: No
  
- **Assault Rifle**:  
  - Firing Rate: High  
  - Stopping Power: Medium  
  - Magazine Size: 30  
  - Base Spread: Medium  
  - Kill on Throw: No
  
- **Shotgun**:  
  - Firing Rate: Low  
  - Stopping Power: Very High  
  - Magazine Size: 9  
  - Base Spread: High  
  - Kill on Throw: No (4 pellets per shot)
  
- **Laser Rifle**:  
  - Fire Rate: Continuous  
  - Stopping Power: None  
  - Magazine Size: 300  
  - Base Spread: None  
  - Kill on Throw: No (single beam, ammo depletes at 1 every 0.5s)

##### Melee Weapons:
Melee weapons are effective only at close range, don’t require ammunition, and can parry enemy melee attacks with [Left Click].  
**Parry mechanic:** Timing is key. If you successfully parry, the enemy becomes vulnerable for a short period, allowing you to deal increased damage.

**List of Melee Weapons:**

- **Knife**:  
  - Range: Low  
  - Swing Speed: High  
  - Kill on Throw: Yes  
  - Parry Difficulty: High
  
- **Sword**:  
  - Range: Medium  
  - Swing Speed: Medium  
  - Kill on Throw: Yes  
  - Parry Difficulty: Medium

#### Secondary Weapons:
Secondary weapons are items activated with [Q]. They are subdivided into **Throwables** and **Usables**.

##### Throwables:
Throwables can deal damage, stun, or immobilize enemies. Some can be used multiple times.

**List of Throwables:**

- **Grenade**:  
  - Effect: Deals damage in a radius  
  - Charges: 1

- **Flashbang**:  
  - Effect: Stuns enemies for 0.75s in a large radius  
  - Charges: 2

- **Throwing Knives**:  
  - Effect: Fast projectiles dealing damage to enemies. Can be used between primary weapon shots  
  - Charges: 8

- **PBH (Pocket Black Hole)**:  
  - Effect: Attracts all enemies in a large radius and stuns them  
  - Charges: 1

- **Stasis Module**:  
  - Effect: Slows enemies in a small radius for 1.5s  
  - Charges: 2

- **M.A.D.**:  
  - Effect: Enemies lose aggro on the player and gain aggro on each other in a medium radius  
  - Charges: 1

- **Mine**:  
  - Effect: Detonates if any entity steps on it (including the player)  
  - Charges: 1

- **Wall-B**:  
  - Effect: Creates a small wall when thrown  
  - Charges: 1

##### Usables:
Usables are items that apply status effects or perform actions. They can also be used multiple times.

**List of Usables:**

- **Teleport**:  
  - Effect: Teleports the player to the cursor’s position  
  - Charges: 3 (Rechargeable)

- **ReflectO**:  
  - Effect: Grants the player immunity for 2 seconds and reflects bullets back  
  - Charges: 1

- **Speed**:  
  - Effect: Increases player speed for 15 seconds  
  - Charges: 1

- **Flak Vest**:  
  - Effect: Reduces damage from a bullet  
  - Charges: 1

### Levels:
Levels are handcrafted and consist of multiple areas (scenes). Each area contains enemies that patrol predefined paths or guard specific points. Weapons and equipment spawn in special objects called **E-Boxes**, which also allow players to refill ammo (max ammo: 500). The player begins at one end of the map, and the exit is located on the opposite side. The exit unlocks after a certain number of enemies are defeated. If the player dies before reaching the exit, the level resets, and all equipment is lost.

### Enemies:
Enemies are categorized into four types:

- **Goon**:  
  Basic enemy, killed with a single shot from any damage-dealing weapon. Equipped with any primary weapon but only grenades as a secondary.

- **Fat Guy**:  
  A large enemy that can withstand low-stopping power guns for a period before dying. Like the Goon, it can be equipped with any primary weapon, but only grenades as a secondary. It can patrol or guard a point.

- **Dog**:  
  Fast, melee-based enemies that rush the player on sight. They can patrol or guard a point.

### Reward System:
- **Scoring:** Every kill earns points. If multiple kills happen consecutively, the points will multiply with a bonus. At the end of the level, the total points are combined with the time taken to complete the level, resulting in a final score. This score is saved for comparison.

### UI

#### Main Menu:
The main menu has an animated background and three buttons:
1. **Start**: Starts the game.
2. **Stats**: Shows the top 10 scores.
3. **Options**: Opens the options UI.
4. **Quit**: Exits the game and returns to the operating system.
