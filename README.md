# Amazfit sport data exporter

## What does this app do
* Export all workouts data from watch (amount of workouts is limited by watch itself - they automatically rewrite old workouts)
* detects already exported workouts and skip them
* Exported workouts saves 3 ways:
    1. last export - in this folder, you can find workouts from last export.
    2. ordered by date - there are all workouts in one folder ordered by date (you have to order folder by name in explorer)
    3. ordered by sport - there are workouts split into their sport name folder
* If there is unknown sport (eg. you are using amazfit 3 - there are more sports, than on Amazfit 2 stratos for which I build this app) it asks, whether to export it or not. (Note this could potentialy break program. If so, you can open issue and I will repair it.)

> note: App cannot export triathlon and multisport. I don't plan to implement this if there is no interest...

## Future plans
* I would like to make app be able to directly upload data to eg. Endomondo or another apps (create issue - new feature if you want any other apps implement. I will consider it)
* Be able to export directly from file (eg. if you have older not exported backups)
* maybe make GUI, but this is probably not neccessary

## How to use this app
1. In App folder run Amazfit_data_exporter.exe
    * It checks for new version and give info
2. click "y" for continue
3. App sends request for backup to Amazfit. Click on Backup my data
    * Wait until it finishes the backup. This can take up to minute.
    * If Amazfit disconnected during the backup or another issue happends it will give an error. Just restart the app and try again.
    * If more android device are connected to PC (with android debugging enabled) app tries to export data, but I can't controll from which device it will export (if wrong device, it throws error)
4. After export it shows all workouts.
5. (If there is unknown sport app will ask whether to export or not. Press "y" or "n".)
6. Last prompt - press "y" for creating .tcx files of workouts
7. Enter to exit

## How this app works (advanced stuff)
Amazfit watches uses android core, so there can be used ADB (android debug bridge). So using Adb I backup apk with id com.huami.watch.newsport, transfer backup file to .tar (something like .zip) and then extract that. Now there is sport_data.db - this file is database and contains all needed data. Sport_summary table's rows represents one workout each row (with track_id as key). This timestamp by the way is unix time in miliseconds. And finaly heart_rate table containing whole workout details. Each record is assigned to one workout and contain eg. time from start or BPM of heart (beats per minute). From this data app can build xml formated document. Tcx file is basically xml file.
