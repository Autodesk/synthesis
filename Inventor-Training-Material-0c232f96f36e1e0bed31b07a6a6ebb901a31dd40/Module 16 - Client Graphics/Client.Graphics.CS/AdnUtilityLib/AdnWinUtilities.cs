////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved 
// Written by Jan Liska & Philippe Leefsma 2011 - ADN/Developer Technical Services
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
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.Configuration.Install;
using System.Runtime.InteropServices;

namespace Autodesk.ADN.Utility.WinUtils
{
    public class CommonUtils
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)]string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        //The function determins whether a method exists in the export table of a certain module. 
        static bool Win32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);

            if (moduleHandle == IntPtr.Zero)
                return false;

            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }   

        public static bool Is64BitOperatingSystem()
        {
            // 64-bit programs run only on Win64
            if (IntPtr.Size == 8)
                return true;

            // 32-bit programs run on both 32-bit and 64-bit Windows 
            bool flag;
            return ((Win32MethodExist("kernel32.dll", "IsWow64Process") && 
                IsWow64Process(GetCurrentProcess(), out flag)) && flag);
        }

        /// <summary>
        /// The function determines whether the operating system of the 
        /// current machine of any remote machine is a 64-bit operating 
        /// system through Windows Management Instrumentation (WMI).
        /// </summary>
        /// <param name="machineName">
        /// The full computer name or IP address of the target machine. "." 
        /// or null means the local machine.
        /// </param>
        /// <param name="domain">
        /// NTLM domain name. If the parameter is null, NTLM authentication 
        /// will be used and the NTLM domain of the current user will be used.
        /// </param>
        /// <param name="userName">
        /// The user name to be used for the connection operation. If the 
        /// user name is from a domain other than the current domain, the 
        /// string may contain the domain name and user name, separated by a 
        /// backslash: string 'username' = "DomainName\\UserName". If the 
        /// parameter is null, the connection will use the currently logged-
        /// on user
        /// </param>
        /// <param name="password">
        /// The password for the specified user.
        /// </param>
        /// <returns>
        /// The function returns true if the operating system is 64-bit; 
        /// otherwise, it returns false.
        /// </returns>
        /// <exception cref="System.Management.ManagementException">
        /// The ManagementException exception is generally thrown with the  
        /// error code: System.Management.ManagementStatus.InvalidParameter.
        /// You need to check whether the parameters for ConnectionOptions 
        /// (e.g. user name, password, domain) are set correctly.
        /// </exception>
        /// <exception cref="System.Runtime.InteropServices.COMException">
        /// A common error accompanied with the COMException is "The RPC 
        /// server is unavailable. (Exception from HRESULT: 0x800706BA)". 
        /// This is usually caused by the firewall on the target machine that 
        /// blocks the WMI connection or some network problem.
        /// </exception>
        //public static bool Is64BitOperatingSystem(
        //    string machineName,
        //    string domain, 
        //    string userName, 
        //    string password)
        //{
        //    ConnectionOptions options = null;
        //    if (!string.IsNullOrEmpty(userName))
        //    {
        //        // Build a ConnectionOptions object for the remote connection 
        //        // if you plan to connect to the remote with a different user 
        //        // name and password than the one you are currently using.
        //        options = new ConnectionOptions();
        //        options.Username = userName;
        //        options.Password = password;
        //        options.Authority = "NTLMDOMAIN:" + domain;
        //    }
        //    // Else the connection will use the currently logged-on user

        //    // Make a connection to the target computer.
        //    ManagementScope scope = new ManagementScope("\\\\" +
        //        (string.IsNullOrEmpty(machineName) ? "." : machineName) +
        //        "\\root\\cimv2", options);
        //    scope.Connect();

        //    // Query Win32_Processor.AddressWidth which dicates the current 
        //    // operating mode of the processor (on a 32-bit OS, it would be 
        //    // "32"; on a 64-bit OS, it would be "64").
        //    // Note: Win32_Processor.DataWidth indicates the capability of 
        //    // the processor. On a 64-bit processor, it is "64".
        //    // Note: Win32_OperatingSystem.OSArchitecture tells the bitness
        //    // of OS too. On a 32-bit OS, it would be "32-bit". However, it 
        //    // is only available on Windows Vista and newer OS.
        //    ObjectQuery query = new ObjectQuery(
        //        "SELECT AddressWidth FROM Win32_Processor");

        //    // Perform the query and get the result.
        //    ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
        //    ManagementObjectCollection queryCollection = searcher.Get();
        //    foreach (ManagementObject queryObj in queryCollection)
        //    {
        //        if (queryObj["AddressWidth"].ToString() == "64")
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

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
        public enum InstallMode
        {
            kRegistryFree = 0,
            kRegistry = 1,
            kBoth = 2
        };

        public enum RegFreeMode
        {
            kVersionDep,
            kVersionIndep,
            kUserOverride
        };

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Use: Generates path for .addin file
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string GenerateAddinFileLocation(RegFreeMode mode, string version)
        {
            string user = System.Environment.UserName;

            string os = Autodesk.ADN.Utility.WinUtils.CommonUtils.GetOsVersion(false);

            switch (mode)
            {
                case RegFreeMode.kVersionIndep:

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

                case RegFreeMode.kVersionDep:

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

                case RegFreeMode.kUserOverride:
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
            RegFreeMode mode,
            string version)
        {
            return InstallRegistryFree(stateSaver, Assembly.GetExecutingAssembly(), mode, version);
        }

        public static string InstallRegistryFree(
            IDictionary stateSaver,  
            Assembly Asm,
            RegFreeMode mode,
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

                if(!Directory.Exists(addinFilenameDest))
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Use: Registry Install / Uninstall
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void InstallRegistry(IDictionary stateSaver, Assembly asm)
        {
            if (Autodesk.ADN.Utility.WinUtils.CommonUtils.Is64BitOperatingSystem())
            {
                RegAsm64("/codebase", asm);
            }
            else
            {
                RegistrationServices regsrv = new RegistrationServices();

                regsrv.RegisterAssembly(asm, AssemblyRegistrationFlags.SetCodeBase);
            }
        }

        public static void UninstallRegistry(IDictionary savedState, Assembly asm)
        {
            if (Autodesk.ADN.Utility.WinUtils.CommonUtils.Is64BitOperatingSystem())
            {
                RegAsm64("/u", asm);
            }
            else
            {
                RegistrationServices regsrv = new RegistrationServices();
                regsrv.UnregisterAssembly(asm);
            }
        }

        //from http://resnikb.wordpress.com/2009/05/21/installing-the-same-managed-addin-in-32bit-and-64bit-autodesk-inventor/.
        public static void RegAsm64(string parameters)
        {
            RegAsm64(parameters, Assembly.GetExecutingAssembly());
        }

        public static void RegAsm64(string parameters,  Assembly asm)
        {
            //.Net Framework Path
            string fmwk_path =
                Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..\\.."));

            //RegAsm Path
            string regasm_path = string.Concat(fmwk_path,
                "\\Framework64\\",
                RuntimeEnvironment.GetSystemVersion(),
                "\\regasm.exe");

            if (!File.Exists(regasm_path))
            {
                MessageBox.Show("Failed to find RegAsm",
                    "Installer Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string addin_path = asm.Location;

            System.Diagnostics.Process process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    UseShellExecute = false,
                    FileName = regasm_path,
                    Arguments = string.Format("\"{0}\" {1}",
                        addin_path,
                        parameters)
                }
            };

            using (process)
            {
                process.Start();
                process.WaitForExit();
            }
        }
    }

    /////////////////////////////////////////////////////////////
    // Use: a small utility class to create child dialogs
    //
    /////////////////////////////////////////////////////////////
    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        private IntPtr mHwnd;

        public WindowWrapper(IntPtr handle)
        {
            mHwnd = handle;
        }

        public IntPtr Handle
        {
            get
            {
                return mHwnd;
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    // Description:
    //
    //////////////////////////////////////////////////////////////////////////////////////////////
    public sealed class PictureDispConverter
    {
        [DllImport("OleAut32.dll",
            EntryPoint = "OleCreatePictureIndirect",
            ExactSpelling = true,
            PreserveSig = false)]
        private static extern stdole.IPictureDisp
            OleCreatePictureIndirect(
                [MarshalAs(UnmanagedType.AsAny)] object picdesc,
                ref Guid iid,
                [MarshalAs(UnmanagedType.Bool)] bool fOwn);

        static Guid iPictureDispGuid = typeof(stdole.IPictureDisp).GUID;

        private static class PICTDESC
        {
            //Picture Types
            public const short PICTYPE_UNINITIALIZED = -1;
            public const short PICTYPE_NONE = 0;
            public const short PICTYPE_BITMAP = 1;
            public const short PICTYPE_METAFILE = 2;
            public const short PICTYPE_ICON = 3;
            public const short PICTYPE_ENHMETAFILE = 4;

            [StructLayout(LayoutKind.Sequential)]
            public class Icon
            {
                internal int cbSizeOfStruct = Marshal.SizeOf(typeof(PICTDESC.Icon));
                internal int picType = PICTDESC.PICTYPE_ICON;
                internal IntPtr hicon = IntPtr.Zero;
                internal int unused1;
                internal int unused2;

                internal Icon(System.Drawing.Icon icon)
                {
                    this.hicon = icon.ToBitmap().GetHicon();
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public class Bitmap
            {
                internal int cbSizeOfStruct = Marshal.SizeOf(typeof(PICTDESC.Bitmap));
                internal int picType = PICTDESC.PICTYPE_BITMAP;
                internal IntPtr hbitmap = IntPtr.Zero;
                internal IntPtr hpal = IntPtr.Zero;
                internal int unused;

                internal Bitmap(System.Drawing.Bitmap bitmap)
                {
                    this.hbitmap = bitmap.GetHbitmap();
                }
            }
        }

        public static stdole.IPictureDisp ToIPictureDisp(System.Drawing.Icon icon)
        {
            PICTDESC.Icon pictIcon = new PICTDESC.Icon(icon);

            return OleCreatePictureIndirect(pictIcon, ref iPictureDispGuid, true);
        }

        public static stdole.IPictureDisp ToIPictureDisp(System.Drawing.Bitmap bmp)
        {
            PICTDESC.Bitmap pictBmp = new PICTDESC.Bitmap(bmp);

            return OleCreatePictureIndirect(pictBmp, ref iPictureDispGuid, true);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Use: 
    //
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class AdnTimer
    {
        DateTime _previous;

        public AdnTimer()
        {
            _previous = DateTime.Now;
        }

        public double ElapsedSeconds
        {
            get
            {
                DateTime Now = DateTime.Now;
                TimeSpan Elaspsed = Now.Subtract(_previous);

                _previous = Now;

                return Elaspsed.TotalSeconds;
            }
        }
    }
}


