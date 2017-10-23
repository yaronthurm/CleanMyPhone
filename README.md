# CleanMyPhone
A small desktop app to help clean my phone by copying all photos from it to the desktop and then delete the photos from the phone (while keeping me in full control as for what to delete and how much)

*This app is still in beta.*

Manual setup is currently required to use it.
1. See the "Example" directory to see how the setup looks like.
	1.1. Basically, there is an AppFolder (which is defined in the App.config file) - Change it manully to point to whetever directory on your PC.
	1.2. Inside the AppFolder, create a "Devices" folder.
	1.3. In the "Devices" folder create a seperate folder for each device you intend to connect with, which is named after the device id (you can choose whatever id you want)
	1.4. In the "per device folder" create a "Setting.txt" file which will control the setting for that device. You can use the file from the example
	and tweek around to suit you'r needs.
	1.5. In the "per device folder" create a "ExcludeFromCleanup.txt" file which will contains all file names you would like to be excluded 
	from deletion. Each line contains the name of the file includeing suffix excluding full path. e.g. if you want to exclude files 
	/DCIM/Camera/file1.jpg and /DCIM/Camera/file2.mp4 your ExcludeFromCleanup.txt should look like that:
	file1.jpg
	file2.mp4
2. In the device you want to connect to:
	2.1 Create a folder in the home directory called "Cleaner" and place a single file: guid.txt. the content of the file should be:
	id = TheNameOfFolderFromStep1, for the device in the Example it should be: id = ExampleDevice1
	2.2 Install a WebDAV Server app, for example: https://play.google.com/store/apps/details?id=com.theolivetree.webdavserver&hl=en
3. Start the WebDav server with username/password as configured in the Settings.txt file
4. Start CleanMyPhone and hope for the best.
	
