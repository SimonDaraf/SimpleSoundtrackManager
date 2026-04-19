# SimpleSoundtrackManager
The automated soundtrack system allows a user to seamlessly transition between audiotracks.
The primary focus of this application is to be used during roleplaying/tabletop games, or other live events where control over background audio is desired.

## Latest Version

### v1.2.0-alpha
#### Changelog
* Keep session volume between sessions.
* Display a warning a user attempts to play a track when audio is still being cached.
* Allow track specific volume to be modified during an active session.
* When an active track is clicked during a session it fades out instead of trying to play again from the beginning causing the position to jump all over between reads.
* Allow Users to start tracks av overlay tracks, the user can start any number of tracks as an overlay track except the base track currently playing. An overlay track can be started
using ctrl + left click on a track, and stopped with ctrl + left click on the same track.
* Tracks can now be assigned a category in the track editor.
* During a session a user can now filter tracks based on their category and/or if they are active.

## Features

### General

#### Saving a session
To save a session, click ``File -> Save`` or the shortcut ``left ctrl + S``. This can be done from anywhere in the application.

### Track View
The track view is where new tracks can be added and customized.

<img width="1921" height="1039" alt="track editor" src="https://github.com/user-attachments/assets/15ef2a10-1df5-4426-96d0-c9b9591e2d40" />

#### Add new track
Click the ``Add New Track`` button in the bottom left and select the desired audio file.

#### Change loop points
To change the loop points, drag the two handles to the desired start and end position.

#### Change transition length
To change the transition length, move the cursor over the start handle and hold ``left ctrl + left mouse`` and drag.

#### Change track name
Modify the left most text input with the desired name.

#### Change track color
Click on the color button and pick the desired color.

#### Change track category
Modify the category text field with the desired category. The category is **case sensitive**.

### Session View
The session view is where you control an active session.

<img width="1921" height="1040" alt="base session" src="https://github.com/user-attachments/assets/f90d4dc5-6d9f-41dd-8a9d-13319093c5da" />

#### Base track
To start a base track, simply click on the track to start. Whenever another track is clicked, the current active track will fade into the selected one.

#### Overlay Track
To start a track as an overlay track use ``left shift + left click``. This will start the track as an overlay. Overlay tracks can be active even if no base track is playing.
To stop an overlay track press ``left shift + left click`` on it again. To transition an oveerlay track to a base track. Click on an active overlay track. This will transition it from an overlay to a base track.

#### Filters
The filters at the top can be used to show all track within a category or only active tracks.

<img width="1921" height="1038" alt="filter by category" src="https://github.com/user-attachments/assets/a37c983e-f6bf-427c-a011-ae5eff5de21f" />
<img width="1921" height="1039" alt="isactiveshowcase" src="https://github.com/user-attachments/assets/2999c456-75bc-44ae-af91-50b4c64cc401" />


