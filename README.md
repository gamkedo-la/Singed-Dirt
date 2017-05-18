# Singed Dirt

SD (Singed Dirt) is a 3D, turn based, artillery game with heavy Scorched Earth influence.

Using Unity 3d engine version 5.5.0f3 (ish) and Blender 2.78a (ish).

The main scenes are in the Scenes / Lobby folder.  To start the game start with the MainLobby scene and for editing main game assets use the LobbyGame scene.

# Creating Prefabs

When creating prefabs for SD there are a few key things that need to be taken into consideration:

* Once the prefab is finished it needs to go into the appropriate folder for that type in Assets > Resources
* Once it's in that location update SingedPrefabEnums.cs to include the name of that prefab in the appropriate group

These two tasks allow for the networking component to instantiate things when needed and provides a handy way to easily access enums for different types.

# Projectile Prefabs

When creating projectiles for SD the following things are required in addition to the previous two generic prefab steps:

## Projectile Components

In addition to the default components you will need to add the following to your prefab:

* A collider of appropriate shape and size.
* A Network Transform component (which will also add a Network Identity component which is needed, adding this one first will save a step since identity is required for transform but not the other way around).
* A rigidbody is needed.
* The Projectile Controller script.  This will allow you to select Explosion Kind and Deformation Kind.  If this is not going to be a cluster style projectile don't worry about the cluster or bomblet stuff, it's ok to leave those blank or default.
* If your projectile has a distinct front then you will need to add the LookWhereYoureGoing script and add the appropriate prefab and rigidbody to it.  Adjust lerp speed as needed for projectile.

# Explosion Prefabs

When creating explosions for SD the following things are required in addition to the previous two generic prefab steps:

* A Network Transform component (which will also add a Network Identity component which is needed, adding this one first will save a step since identity is required for transform but not the other way around).
* If your explision is going to last longer than 3 seconds then you will need to add an Explosion Controller script to it and update the duration value to match the explosion length (with maybe 1 second more).

# Deformation Prefabs

When creating explosion deformations for SD the following things are required in addition to the previous two generic prefab steps:

* A Terrain Deformer script needs to be added.  It's HIGHLY recommended that you duplicate the base shotCracter prefab and then modify settings as needed to fit your explosion type.

## Terrain Deformer options

Key things to know about the options (and while these are handy to know, it's a good idea to just mess with stuff to see how it works and what it looks like):

* Performance options should be checked.
* Deformation options are mostly self explanatory.  The profile is the shape of the deformation.  The current deformation is what gives the holes the lip.  Play around to see what's appropriate for your projectile!
* Bumpiness is fine to leave as what's in the default, I honestly don't remember exactly what this part does.
* Scar options: the big on to know about here is the Scar Texture Index.  On the terrain itself (under Environment in Assets > Scenes > Lobby > LobbyGame) there are textures that have been added under the button with the paintbrush.  These textures are an array on the terrain itself (which is why there is a index for the scar).  If you are creating a custom scar then you will want to add that texture to the terrain and then use the index (starts counting at 0 for the first one) to specify which scar to use.  The current scar texture is at 4.

# One-Liners/Barks

### General criteria for adding new barks and character voices:
1. New ammo/bark types can have more or fewer barks than the other ammo/bark types.
	* (Example: if you want a new bark triggered on Death, you might only have 1 one-liner.)
2. BUT any new one-liner must have 1 version/file for each character voice.
	* (Example: the Death one-liner might have 4 different files: meanie, grumpy, aggro, and cheery.) 
3. Any new character voice must match the other characters in the number of one-liners for every ammo/bark type. (Example: If the other characters have 4 barks for Acorn and 1 for Death, then the new character must have 4 barks for Acorn and 1 for Death.)
 
### To add a new type of ammo/bark one-liner: 
1. Add a new Enum below and add a reference in the NetRegistry.cs file. 
2. Group the one-liners by character voice using the same voice sequence as the other Enums. 
	* (Example: If meanie is the first group in one Enum, it should be first in all Enums.)

### To add one-liners to already-existing ammo/bark types: 
1. Add each version's filename to the appropriate Enum, grouping it with its appropriate character voice.

### To add a new character voice: 
1. Add each filename to its appropriate Enum below, grouping the references by character voice. 
2. Always add your voice group at the same position within each Enum. 
	* (Example: If the new character's lines are put 2nd in one Enum, it should be 2nd in all the other Enums.) 
3. Increase the totalVoices property by the number of new character voices added.
4. Declare a currentIndex property and totalLines property for your the bark type.
5. Add an indexQueue for your new bark type.
6. Declare a kind variable for the bark type. (Example: deathOneLinersKind deathOneLiner)
7. Assign the totalLines value in Awake.
8. Assign the reset value for currentIndex and build the indexQueue in the Reset method.
9. Add the appropriate condition and triggered code in GetTheShotOneLiner method, or write a new method specific to your bark.

If you have any questions ask in the #team-singed-dirt channel on Slack. (this obviously only works if you are part of the Gamkedo club, if you would care to join and make games with us go to http://gamkedo.club for more info)
