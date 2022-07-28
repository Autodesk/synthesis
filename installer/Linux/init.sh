#!/bin/sh

# run before with path to a compiled build as well as robots and fields directory
# also allow user to specify version

show_help() {
	echo "-h	display help message"
	echo "-f	specify the input directory for fields"
	echo "-r	specify the input directory for robots"
}


OPTIND=1

while getopts "h?f:r:" opt; do
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
	esac
done

shift $((OPTIND-1))


if [ ! -n "$fields" ] || [ ! -n "$robots" ] ; then
	echo "Make sure you specify an input directory both fields and robots"
	exit 1
fi

echo $fields
echo $robots
