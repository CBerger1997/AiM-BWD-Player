# AiM-BWD-Player

Brief READ ME for now

To change the file path for the videos and output file you need to:
  - go into the SettingsManager.cs
  - within FILENAME SETTINGS region near the bottom locate the SetFilePathDestinations() function
  - change the value of the path string variable to the location of the videos
  - the @"\" are there as with "/" within a file path don't seem to work, you'll need to change the path to contain @"\" wherever there is a "/", the current one is an example of this
  - ensure the folder containing the videos also contains a folder (can be new and empty) called "Data Output"
  - Run unity to check that the filepaths are correct in their setting
  - Now if you build unity and run the .exe everything should work...
  - Will only work properly through build!!
  
Things that will be fixed
  - I will do a string.replace to fix the @"\" problem, meaning in future you'll be able to just copy and paste a file path and it will work without the annoying changing
