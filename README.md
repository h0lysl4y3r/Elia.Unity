# Elia.Unity

This is a high-level framework for Unity used in several of our games.

It contains several main modules that are accessible as singletons:
## Boostrap
A module used at the start of application, it provides management of splash screens and scene management.
## App 
A main entry point, that manages several other modules and components. An app can be either in **Menu** or in **Game**.
## Game
Main game module that handles level flow. Each game has a level, level state and can have several missions or tutorial.
## GUI
Main UI module that manages visual sets such as screens, overlays and modals.
Screen is a distinct set of controls, only one can be active at a time. Overlay is a control set that is displayed over some screen, and modal can be on top of everything. In order to manage these 3 types separately modules **Screens**, **Overlays** and **Modals** can be used.
## PrefsStorage
An extension to PlayerPrefs providing obfuscation.
## Audio
An extension to easier manage 'fire-forget' sounds, loops and music.

There are many other specific modules and components to explore.
This work is sparsely maintained.