
# project-zero
Game developed for the course Game Development at Unifi.
  
## Brief Description:
[Project Zero] is a top down 2D shooter with a focus on [] and fast gameplay. Some examples of similar games that are already on the market are Hotline Miami 1 & 2, Door Kickers and Nuclear Throne.
  
## Setting:

The game is set in a non-too-far future, in a space station orbiting Jupiter where a sort of Hunger Game is played [how much is played and when is not important] and streamed to the Metanet [Hypernet if we don't want to be funny]. You are one of the competitors and, of course, your objective is to survive and get out with a lot of cash. Of course in order to succede you'll have to paint the station red.
  
## Game Mechanics:
  
### Movement:
The movement is going to be standard top down movemnt, `WASD` for moving across the screen while aiming with the mouse. There's no jump. The player will be able to dash a small distance by pressing [Shift], [the dash direction is dictated by the mouse direction.]
  
### Weapons:
Weapons are divided in two macro categories `Primary` and `Secondary`, enemies can be equipped with whatever Primary weapon, but with only a subset of Secondaries. The description of the two categories are:

 **Primary:**
Primary weapons are the exlusive damage dealer and subdivide in:
- Ranged: Ranged weapons can hit from long range, but have very limited ammunitions  

**List of Ranged Weapons:**

`Pistol` -> firing rate: high, stopping power: low, mag size: 15, base spread: low, kill on throw: no

`SMG` -> firing rate: very high, stopping power: very low, mag size: 50, base spread: high, kill on throw: no

`Assault Rifle `-> firing rate: high, stopping power: medium, mag size: 30, base spread: medium, kill on throw: no

`Shotgun` -> firing rate: low, stopping power: very high, mag size: 9, base spread: high, kill on throw: no (4 pellets per shot)

`Laser Rifle` -> fire rate: continous, stopping power: none, mag size: 300, base spread: none, kill on throw: no (single beam that damage the enemies, ammo depletes at a 1 every 0.5s)

- Melee: Melee weapons can only hit at close range, they do not require ammo and can parry the enemy's melee attacks by pressing [Left Click]

Parry will be timed. The user need to be skilled enough and get the correct timing for parrying the enemy. Once an enemy as parried, it is vulnerable for X seconds and attacking the enemy within this time range will inflict a lot of damage rather than normal.

**List of Melee Weapons:**

`Knife` -> range: low, swing speed: high, kill on throw: yes, parry difficulty: high

`Sword` -> range: medium, swing speed: medium, kill on throw: yes, parry difficulty: medium
  
**Secondary:**
Secondary are basically items that can be used pressing [Q], they subdivide in:
- Throwables: Throwables objects that can deal damage, stun, or immobilize. They can have more than one use.

**List of Throwables:**

`Granade` -> deals damage in a radius. charges: 1
Flashbang -> stuns the enemeies for 0.75s in a big radius. charges: 2

`Throwing Knives` -> fast projectile that deal damage to the enemy, can be shot between the firing animation of the Primary. charges: 8

`PBH (Pocket Black Hole)` -> once thrown attract to his position all the enemies in a big radius, the enemies are stunned while attracted. charges: 1

[`Stasis Module` -> for 1.5s in a small radius all enemies inside it or that enter it will be slowed. chrages: 2]

[`M.A.D` -> enemies cuaght in the medium radius, loose aggro on the player and gain it on each other. charges: 1 (we will have to add a friendly fire enabler)]

[`Mine` -> can be placed, and if anyone (player included) steps on it it will detonate. charges: 1]

[`Wall-B` -> when thrown creates a small wall. charges: 1]

- Usables: Usables are objects that can apply a status or do an action. They can have more than one use.

**List of Consumable:**

`Teleport` -> when used it teleports the player to the cursor position. charges: 3 [, rechargable: yes]

`ReflectO` -> when used it make the player immune for 2 seconds [and reflect the bullets back]. charges: 1

`Speed` -> when used it enhance the player speed for 15 seconds. charges: 1

`Flak Vest` -> when equipped protect the player from a bullet, charges: 1

### Levels

Levels are handcrafted and can be divided into multiple areas. An area is basically a new scene. In the level are gonna be present enemies that patrols predefined paths or that guard a predefined point. Guns and equipment is spawned in special object called "E-Box" that may also allow the player to refill a weapon's ammo [indicatively the ammo that can be gathered is equal to 500]. The player will be spawned in the map and the exit is going to be on the other side of the map. The exit of the level or of the area is activated once a certain number of enemies has been killed. If you die before reaching the next level the all level will restarting loosing the equipment.

### Enemies

Enemies are divided in 4 types

**Goon:** The goon is the base enemy it can be killed by one shot of every DD (Damage Dealing) weapon. It can be equipped with any primary weapon but with only granades for secondaries.

**Fat Guy:** The Fat Guy is a big enemy that can withstand the damage of low stopping power guns for a variable period of time before dying. Like the goon it can be equipped with any primary weapon, but can only have granades as a secondary. Still like the goon it can patrol a certain path or guard a certain point.

**Dog:** Dogs are rush enemies, their only weapon is a bite melee weapon, and they will rush the player anytime they see it. They're fast. Like the other moving enemies they can patrol a path or guard a point.

### Reward System:

Every kill will grant a set amount of points, if multiple kills happen one after the other the points gathered will be multiplied by a increasing value (that will increase every bunch of kills). At the end of the level the points gathered will be combined with the time of completion of the level and will be returned the final score for the level, this score will be saved.

### UI
#### Main Menu
The main menu has a animated background, three buttons: Start, Options and Quit.
Start -> starts the game.
Stats -> It shows the best 10 scores.
Options -> opens the option UI.
Quit -> return to the OS.