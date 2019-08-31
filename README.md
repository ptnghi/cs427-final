# GOLDEN BATTLE


## Release Download

You can find the release [here](https://drive.google.com/file/d/1oJbK7ymoq7YxQd0neDEgPtM7kB64qO2o/view?usp=sharing)

## Video Demo

The video demo can be found [here](https://youtu.be/R_Mieldor8I)

## How to play

The goal of the game is to kill all of your enemy unit. In a turn, a player can move and attack with all of their unit. Each unit can move one time and can attack one time. If a player used all of their units actions, their turn automatically ends. A player can end their turn by clicking end turn anytime. Once a unit's HP reaches zero, it dies. Each unit has different stats (Damage, Armor, HP, Movement range and Attack range).

When a player click on their unit, a blue highlight will mark it's movement range. If you want to attack, click on the "Attack" button after clicking on the unit. A pink highlight will show the unit's attack range.

## Description
This game is a turn-based tactic inspired by chess game. However instead of taking by moving over, the units in this game has stat and must fight in turn to kill enemy's unit.

### Technology highlight

The game's main features are implemented from scratch without templates.

- The game playing board is programatically generated at runtime using mesh creation with vertexes.
- The grid-based movement, path finding, range highlight are implemented using graph data structure and algorithm.
- The path-finding system use a Dijkstra implementation to navigate while range highlight use BFS to search for grid in range.
