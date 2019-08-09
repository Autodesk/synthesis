#!/bin/bash
function printSuccess(){
    string=$1;
    printf -- "\033[32m - $string \033[0m\n";
}

function printWarning(){
    string=$1;
    printf -- "\033[33m - $string \033[0m\n";

}

function printError(){
    string=$1;
    printf -- "\033[31m ERROR: $string \033[0m\n";
    exit 1;
}

function install(){
    # Sanity check for brew
    if [ type brew &> /dev/null ]
    then
        printWarning ' Could not find Brew installed on local machine ';
    else
        printSuccess ' Found Brew '
    fi;

    # Sanity check for unzip
    if [ type unzip &> /dev/null ]
    then
        printWarning ' Could not find unzip installed on local machine ';
    else
        printSuccess ' Found unzip '
    fi;

    # Sanity check for Synthesis
    if [ -e $HOME/.config/Autodesk/Synthesis ]
    then
        printSuccess " Found Synthesis ";
    else
        printWarning " Synthesis is not installed ";
        mkdir -p $HOME/.config/Autodesk/Synthesis
    fi

    # Change to Synthesis Directory
    cd $HOME/.config/Autodesk/synthesis

    printWarning " Downloading Robots "
    wget https://github.com/Autodesk/synthesis/releases/download/v4.2.3/SynthesisSampleAssets.zip -q --show-progress

    printWarning " Unzipping Assets "
    unzip -a -o SynthesisSampleAssets.zip | awk 'BEGIN {ORS=" "} {if(NR%10==0)print "."}'

    printWarning " Copying Fields and Robots "
    cp -a SynthesisSampleAssets/Basic\ Fields\ and\ Robots/* ./

    printWarning " Removing Samples Folder "
    rm -rf SynthesisSampleAssets
    rm SynthesisSampleAssets.zip

    path="~/.config/Autodesk/Synthesis"
    printSuccess " Sucessfully Downloaded Robots and Fields to $path "
    
}

function uninstall(){
    rm -rf ~/.config/Autodesk/Synthesis

    printSuccess " Uninstalled Successfully "
}

function help(){
    echo " synthesis -- "
    echo " -install installs the field and robot files in the default dmg path "
    echo " -uninstall uninstalls the entire application "
    echo " - eg. synthesis -install "
}


if [ ${#@} -ne 0 ] && [ "${@#"--help"}" = "" ]; then
    help;
    exit 0;
elif [ ${#@} -ne 0 ] && [ "${@#"-install"}" = "" ]; then
    echo " Installing Synthesis "
    install ;
elif [ ${#@} -ne 0 ] && [ "${@#"-uninstall"}" = "" ]; then
    echo " Uninstalling Synthesis "
    uninstall;
else
    help;
    exit 0;
fi;
