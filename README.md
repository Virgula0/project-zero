
# project-zero

Game developed for the course Game Development at Unifi.

  

# [PROJECT ZERO]

  

## Brief Description:

[Project Zero] is a top down 2D shooter with a focus on [] and fast gameplay. Some examples of similar games that are already on the market are Hotline Miami 1 & 2, Door Kickers and Nuclear Throne.

  

## Setting:

The game is set in a non-too-far future, in a space station orbiting Jupiter where a sort of Hunger Game is played [how much is played and when is not important] and streamed to the Metanet [Hypernet if we don't want to be funny]. You are one of the competitors and, of course, your objective is to survive and get out with a lot of cash. Of course in order to succede you'll have to paint the station red.

  

## Game Mechanics:

  

### Movement:

The movement is going to be standard top down movemnt, WASD for moving across the screen while aiming with the mouse. There's no jump. The player will be able to dash a small distance by pressing [Shift], [the dash direction is dictated by the movent].

  

### Weapons:

Weapons are divided in two macro categories:

  

 **Primary:**

Primary weapons are the exlusive damage dealer and subdivide in:

  

- Ranged: Ranged weapons can hit from long range, but have very limited ammunitions

  

**List of Ranged Weapons:**

  

Pistol -> firing rate: high, stopping power: low, mag size: 15, base spread: low, kill on throw: no

  

SMG -> firing rate: very high, stopping power: very low, mag size: 50, base spread: high, kill on throw: no

  

Assault Rifle -> firing rate: high, stopping power: medium, mag size: 30, base spread: medium, kill on throw: no

  

Shotgun -> firing rate: low, stopping power: very high, mag size: 9, base spread: high, kill on throw: no (4 pellets per shot)

  

- Melee: Melee weapons can only hit at close range, they do not require ammo and can parry the enemy's melee attacks by pressing [Left Click]

  

**List of Melee Weapons:**

Knife -> range: low, swing speed: high, kill on throw: yes, parry difficulty: high

  

Baseball Bat -> range: medium, swing speed: medium, kill on throw: no, parry difficulty: high

  

Sword -> range: medium, swing speed: medium, kill on throw: yes, parry difficulty: medium

  

Tec-Sword -> range medium, swing speed: medium, kill on throw: yes, parry difficulty: medium

  

**Secondary:**

Secondary are basically items that can be used pressing [Q], they subdivide in:

  

- Throwables: Throwables objects that can deal damage, stun, or immobilize. They can have more than one use.

  

**List of Throwables:**

  

Granade -> deals damage in a radius. charges: 1

  

Flashbang -> stuns the enemeies for 0.75s in a big radius. charges: 2

  

Throwing Knives -> fast projectile that deal damage to the enemy, can be shot between the firing animation of the Primary. charges: 8

  

PBH (Pocket Black Hole) -> once thrown attract to his position all the enemies in a big radius, the enemies are stunned while attracted. charges: 1

  

Stasis Module -> for 1.5s in a small radius all enemies inside it or that enter it will be slowed. chrages: 2

  

M.A.D -> enemies cuaght in the medium radius, loose aggro on the player and gain it on each other. charges: 1 (we will have to add a friendly fire enabler)

  

Mine -> can be placed, and if anyone (player included) steps on it it will detonate. charges: 1

  

- Usables: Usables are objects that can apply a status or do an action. They can have more than one use.

  

**List of Usables:**

  

Teleport -> when used it teleports the player to the cursor position. charges: 3 [, rechargable: yes]

  

ReflectO -> when used it make the player immune for 2 seconds [and reflect the bullets back]

  

Speed -> when used it enhance the player speed for 15 seconds

  

[4D Printer -> when equipped regenerates your primary ammo 1 every 5 seconds]
