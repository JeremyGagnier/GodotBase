# GodotBase
A Godot setup for starting development on a new game.

This setup is intended to be highly opinionated and is based on data oriented development. The setup relies on the core model of the game being serializable, and for the game update cycle to depend purely on commands that come from the control system. These requirements will allow the game to be saved & loaded, server-side verified, and debugged more easily. It also makes adding certain features such as multiplayer more easily, and is designed to increase development velocity.

## Goals
- Set up the UI system to enable the following:
  - Creating interfaces purely from scripts
    - This simplifies various interface building aspects for the developer since all of the linkages between the UI and other aspects of the game can be done explicitly in code. This simplifies localization, for example.
  - Serialize & deserialize the interface, enabling keyframes and debugging contexts
  - Headless mode, enabling replayability for debugging or other uses such as unit testing
- Set up a control system to enable the following:
  - Setting up a command log to track the sequence of player-driven events in a game
    - This enables multiplayer: players can distribute their command logs
  - Setting up and configuring player controls
  - Managing multiple control contexts
