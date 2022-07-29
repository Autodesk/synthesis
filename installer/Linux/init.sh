#!/bin/sh

# run before with path to a compiled build as well as robots and fields directory
# also allow user to specify version

show_help() {
	echo "-h	display help message"
	echo "-f	specify the input directory for fields"
	echo "-r	specify the input directory for robots"
	echo "-b	specify the build directory of synthesis"
}

OPTIND=1

APP_NAME="Synthesis"
INIT_DIR="$(dirname "$(readlink -f "${0}")")"
APP_DIR="$INIT_DIR/$APP_NAME.AppDir"

mkdir -p "$APP_DIR/usr/bin/"
mkdir -p "$APP_DIR/fields"
mkdir -p "$APP_DIR/robots"

while getopts "h?f:r:b:" opt; do
	case "$opt" in
		h|\?)
			show_help
			exit 0
			;;
		f)
			fields="$OPTARG"
			;;
		r)
			robots="$OPTARG"
			;;
		b)
			build="$OPTARG"
			;;
	esac
done

shift $((OPTIND-1))

if [ ! -n "$build" ] ; then
	echo "Specify synthesis build directory using \"-b\""
	exit 1
fi

if [ -n "$fields" ] ; then
	cp "$fields/"*.mira "$APP_DIR/fields/" 
fi

if [ -n "$robots" ] ; then
	cp "$robots/"*.mira "$APP_DIR/robots/"
fi

cp -R "$build/"* "$APP_DIR/usr/bin" 
chmod +x "$APP_DIR/AppRun"
chmod +x "$APP_DIR/synthesis.desktop"
chmod +x "$APP_DIR/usr/bin/Synthesis.x86_64"
