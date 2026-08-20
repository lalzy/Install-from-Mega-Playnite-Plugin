A playnite plugin to download / install games with an mega link.

# Setup
1. Either clone and compile, or download the release binary.
2. Place into {PlayniteDir}/extensions/InstallFromMega/
3. Download Megatools, and extract into: {Playnite}/[installtools]/MegaTools/

# Setup entries to be installable:
1. Create a game entry how you normally would.
2. Set install path to where it should be (I recommend relative to {PlayniteDir})
3. Create a link entry called MEGA that point to the FOLDER of the game.
4. Store the playnite Folder somewhere (mega, cdRom, anywhere, second device).
5. Download/retrieve ther playnite folder on secondary device(s) you want to play games on.

# Workflow on second device:
1. Download/place your playnite app (that contain your DB) to your secondary device(s).
2. Browse to the game you want to play on it.
3. Open the entry
4. Press 'install'
5. Wait until the script works (and retrieves the game from Mega)
6. When done, press play.
