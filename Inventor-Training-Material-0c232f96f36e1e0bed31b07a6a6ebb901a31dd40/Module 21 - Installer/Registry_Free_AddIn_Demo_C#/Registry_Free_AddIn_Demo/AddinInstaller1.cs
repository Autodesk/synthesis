////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved 
// Written by Philippe Leefsma 2011 - ADN/Developer Technical Services
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Runtime.InteropServices;

using Autodesk.ADN.Utility.WinUtils;

namespace InvAddIn
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // AddinInstaller1: Registry Free Inventor Add-in installer
    //  
    // Author: liangx
    // Creation date: 1/24/2013 3:12:30 PM
    // 
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [RunInstaller(true)]
    public class AddinInstaller1 : Installer
    {
        #region Base Implementation

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion

        public AddinInstaller1()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            try
            {
                base.Install(stateSaver);

                InstallUtils.InstallModeEnum installMode = InstallUtils.InstallModeEnum.kRegistryFree;

                Assembly Asm = Assembly.GetExecutingAssembly();
                FileInfo asmFile = new FileInfo(Asm.Location);

                stateSaver.Add("InstallMode", (int)installMode);

                stateSaver.Add("PathToAddinFile",
                    InstallUtils.InstallRegistryFree(
                        stateSaver,
                        Asm,
                        InstallUtils.RegFreeModeEnum.kVersionIndep,
                        string.Empty));

                // Example for version dependent

                //stateSaver.Add("PathToAddinFile",
                //    InstallUtils.InstallRegistryFree(
                //        stateSaver,
                //        Asm,
                //        InstallUtils.RegFreeModeEnum.kVersionDep,
                //        "Inventor 2012"));

            }
            catch (InstallException ex)
            {
                throw new InstallException(ex.Message);
            }
            catch
            {
                throw new InstallException("Error installing addin!");
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            try
            {
                InstallUtils.InstallModeEnum installMode =
                    (InstallUtils.InstallModeEnum)savedState["InstallMode"];

                string pathToAddinFile = (string)savedState["PathToAddinFile"];
                InstallUtils.UninstallRegistryFree(savedState, pathToAddinFile);
            }
            catch
            {

            }

            base.Uninstall(savedState);
        }
    }
}


namespace Autodesk.ADN.Utility.WinUtils
{
    public class CommonUtils
    {
        public static string GetOsVersion(bool checkSP)
        {
            OperatingSystem os = Environment.OSVersion;

            string operatingSystem = "Unknown";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows 
                switch (os.Version.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (os.Version.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (os.Version.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (os.Version.Minor == 0)
                            operatingSystem = "2000";
                        else
                            operatingSystem = "XP";
                        break;
                    case 6:
                        if (os.Version.Minor == 0)
                            operatingSystem = "Vista";
                        else
                            operatingSystem = "7";
                        break;
                    default:
                        break;
                }
            }

            //Make sure we actually got something in our OS check 
            //We don't want to just return " Service Pack 2" or " 32-bit" 
            if (operatingSystem != "")
            {
                operatingSystem = "Windows " + operatingSystem;

                if (os.ServicePack != "" && checkSP)
                    operatingSystem += " " + os.ServicePack;
            }

            return operatingSystem;
        }
    }

    public class InstallUtils
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Use: Enums for install options
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum InstallModeEnum
        {
            kRegistryFree = 0,
            kRegistry = 1,
            kBoth = 2
        };

        public enum RegFreeModeEnum
        {
            kVersionDep,
            kVersionIndep,
            kUserOverride
        };

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Use: Generates path for .addin file
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string GenerateAddinFileLocation(RegFreeModeEnum mode, string version)
        {
            string user = System.Environment.UserName;

            string os = Autodesk.ADN.Utility.WinUtils.CommonUtils.GetOsVersion(false);

            switch (mode)
            {
                case RegFreeModeEnum.kVersionIndep:

                    switch (os)
                    {
                        case "Windows XP":
                            return @"C:\Documents and Settings\All Users\Application Data\Autodesk\Inventor Addins\";

                        case "Windows Vista":
                        case "Windows 7":
                            return @"C:\ProgramData\Autodesk\Inventor Addins\";

                        default:
                            break;
                    }
                    break;

                case RegFreeModeEnum.kVersionDep:

                    switch (os)
                    {
                        case "Windows XP":
                            return @"C:\Documents and Settings\All Users\Application Data\Autodesk\" + version + "\\Addins\\";

                        case "Windows Vista":
                        case "Windows 7":
                            return @"C:\ProgramData\Autodesk\" + version + "\\Addins\\";

                        default:
                            break;
                    }
                    break;

                case RegFreeModeEnum.kUserOverride:
                    switch (os)
                    {
                        case "Windows XP":
                            return @"C:\Documents and Settings\" + user + @"\Application Data\Autodesk\" + version + "\\Addins\\";

                        case "Windows Vista":
                        case "Windows 7":
                            return @"C:\Users\" + user + @"\AppData\Roaming\Autodesk\" + version + "\\Addins\\";

                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }

            throw new InstallException();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Use: Registry-Free Install / Uninstall
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string InstallRegistryFree(
            IDictionary stateSaver,
            RegFreeModeEnum mode,
            string version)
        {
            return InstallRegistryFree(stateSaver, Assembly.GetExecutingAssembly(), mode, version);
        }

        public static string InstallRegistryFree(
            IDictionary stateSaver,
            Assembly Asm,
            RegFreeModeEnum mode,
            string version)
        {
            try
            {
                FileInfo asmFile = new FileInfo(Asm.Location);

                FileInfo addinFile = null;

                foreach (FileInfo fileInfo in asmFile.Directory.GetFiles())
                {
                    if (fileInfo.Extension.ToLower() == ".addin")
                    {
                        addinFile = fileInfo;
                        break;
                    }
                }

                if (addinFile == null)
                {
                    throw new InstallException();
                }

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(addinFile.FullName);

                XmlNode node = xmldoc.GetElementsByTagName("Assembly")[0];

                if (node == null)
                {
                    throw new InstallException();
                }

                node.InnerText = asmFile.FullName;

                string addinFilenameDest = GenerateAddinFileLocation(mode, version);

                if (!Directory.Exists(addinFilenameDest))
                    Directory.CreateDirectory(addinFilenameDest);

                addinFilenameDest += addinFile.Name;

                if (File.Exists(addinFilenameDest))
                    File.Delete(addinFilenameDest);

                // copy the addin to target folder according to OS type and all users/separate users
                xmldoc.Save(addinFilenameDest);

                addinFile.Delete();

                return addinFilenameDest;
            }
            catch
            {
                throw new InstallException("Error installing .addin file!");
            }
        }

        public static void UninstallRegistryFree(IDictionary savedState, string pathToAddinFile)
        {
            if (File.Exists(pathToAddinFile))
                File.Delete(pathToAddinFile);
        }
    }
}

