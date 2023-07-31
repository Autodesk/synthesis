# Synthesis OSX Installer (DMG Version)

## Setup
1. Install `create-dmg`:
```
$ git clone git@github.com:create-dmg/create-dmg.git
```

2. Copy the signed Synthesis app into source_folder:
```
$ cp -r [Location of app] ./source_folder/Synthesis.app
```

## Create Disk Image
Run the `make-installer.sh` shell script:
```
$ ./make-installer.sh
```

Disk Image will be created at `/installer/OSX-DMG/Synthesis-Installer.dmg`

## Notes
Update `source_folder/license.html` as needed as well as settings for the `create-dmg` command inside of `make-installer.sh`. See [create-dmg repository](https://github.com/create-dmg/create-dmg) for configuration information.