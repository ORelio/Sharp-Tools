﻿using Microsoft.Win32;
using System.Management;

namespace SharpTools
{
    /// <summary>
    /// Retrieve information about the current Windows version
    /// By ORelio - (c) 2018-2024 - Available under the CDDL-1.0 license
    /// </summary>
    /// <remarks>
    /// Environment.OSVersion does not work with Windows 10, it returns 6.2 which is Windows 8
    /// Querying the registry returns Windows 10 on Windows 11, so WMI is used for FriendlyName property
    /// </remarks>
    /// <seealso>
    /// https://stackoverflow.com/a/37755503
    /// https://docs.microsoft.com/en-us/answers/questions/464971/
    /// </seealso>
    class WindowsVersion
    {
        private const string CurrentVersionRegKey = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
        private static string FriendlyNameCache = null;

        /// <summary>
        /// Try retrieving a registry key
        /// </summary>
        /// <param name="path">key path</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value (output)</param>
        /// <returns>TRUE if successfully retrieved</returns>
        private static bool TryGetRegistryKey(string path, string key, out dynamic value)
        {
            value = null;
            try
            {
                var rk = Registry.LocalMachine.OpenSubKey(path);
                if (rk == null) return false;
                value = rk.GetValue(key);
                return value != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the Windows major version number for this computer.
        /// </summary>
        public static uint WinMajorVersion
        {
            get
            {
                dynamic major;
                // The 'CurrentMajorVersionNumber' string value in the CurrentVersion key is new for Windows 10, 
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(CurrentVersionRegKey, "CurrentMajorVersionNumber", out major))
                {
                    return (uint) major;
                }

                // When the 'CurrentMajorVersionNumber' value is not present we fallback to reading the previous key used for this: 'CurrentVersion'
                dynamic version;
                if (!TryGetRegistryKey(CurrentVersionRegKey, "CurrentVersion", out version))
                    return 0;

                var versionParts = ((string) version).Split('.');
                if (versionParts.Length != 2) return 0;
                uint majorAsUInt;
                return uint.TryParse(versionParts[0], out majorAsUInt) ? majorAsUInt : 0;
            }
        }

        /// <summary>
        /// Returns the Windows minor version number for this computer.
        /// </summary>
        public static uint WinMinorVersion
        {
            get
            {
                dynamic minor;
                // The 'CurrentMinorVersionNumber' string value in the CurrentVersion key is new for Windows 10, 
                // and will most likely (hopefully) be there for some time before MS decides to change this - again...
                if (TryGetRegistryKey(CurrentVersionRegKey, "CurrentMinorVersionNumber",
                    out minor))
                {
                    return (uint) minor;
                }

                // When the 'CurrentMinorVersionNumber' value is not present we fallback to reading the previous key used for this: 'CurrentVersion'
                dynamic version;
                if (!TryGetRegistryKey(CurrentVersionRegKey, "CurrentVersion", out version))
                    return 0;

                var versionParts = ((string) version).Split('.');
                if (versionParts.Length != 2) return 0;
                uint minorAsUInt;
                return uint.TryParse(versionParts[1], out minorAsUInt) ? minorAsUInt : 0;
            }
        }

        /// <summary>
        /// Returns whether or not the current computer is a server or not.
        /// </summary>
        public static uint IsServer
        {
            get
            {
                dynamic installationType;
                if (TryGetRegistryKey(CurrentVersionRegKey, "InstallationType",
                    out installationType))
                {
                    return (uint) (installationType.Equals("Client") ? 0 : 1);
                }

                return 0;
            }
        }

        /// <summary>
        /// Returns whether the current computer is running Mono instead of .NET framework (likely Mac and Linux)
        /// </summary>
        public static bool IsMono
        {
            get
            {
                return System.Type.GetType("Mono.Runtime") != null;
            }
        }

        /// <summary>
        /// Get friendly name of the system version
        /// </summary>
        public static string FriendlyName
        {
            get
            {
                // Retrieve Friendly Name from WMI (Works on Windows 11, but has small latency, hence caching)
                if (FriendlyNameCache == null)
                {
                    try
                    {
                        ManagementClass osClass = new ManagementClass("Win32_OperatingSystem");
                        foreach (ManagementObject queryObj in osClass.GetInstances())
                        {
                            foreach (PropertyData prop in queryObj.Properties)
                            {
                                if (prop.Name == "Caption")
                                {
                                    FriendlyNameCache = prop.Value.ToString();
                                    break;
                                }
                            }
                        }
                    }
                    catch (ManagementException) { }
                }

                // Fallback method: Build Friendly Name from Registry (But Windows 11 will report itself as Windows 10)
                if (FriendlyNameCache == null)
                {
                    dynamic ProductName;
                    dynamic CSDVersion;
                    TryGetRegistryKey(CurrentVersionRegKey, "ProductName", out ProductName);
                    TryGetRegistryKey(CurrentVersionRegKey, "CSDVersion", out CSDVersion);
                    if (ProductName != null)
                    {
                        FriendlyNameCache
                            = (ProductName.StartsWith("Microsoft") ? "" : "Microsoft ")
                            + ProductName.ToString()
                            + (CSDVersion != null ? " " + CSDVersion.ToString() : "");
                    }
                }

                return FriendlyNameCache;
            }
        }

        /// <summary>
        /// Check if the current Windows version is between the specified bounds (inclusive)
        /// </summary>
        /// <param name="versionMin">Minimum version in "M.m" format (Major, minor)</param>
        /// <param name="versionMax">Maximum version in "M.m" format (Major, minor)</param>
        /// <returns>TRUE if the current version is between the specified bounds</returns>
        public static bool IsBetween(string versionMin, string versionMax)
        {
            string[] versionMinParts = versionMin.Split('.');
            string[] versionMaxParts = versionMax.Split('.');

            if (versionMinParts.Length == 2 && versionMinParts.Length == 2)
            {
                uint minMajor, minMinor, maxMajor, maxMinor;
                if (uint.TryParse(versionMinParts[0], out minMajor)
                    && uint.TryParse(versionMinParts[1], out minMinor)
                    && uint.TryParse(versionMaxParts[0], out maxMajor)
                    && uint.TryParse(versionMaxParts[1], out maxMinor))
                {
                    return IsBetween(minMajor, minMinor, maxMajor, maxMinor);
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the current Windows version is between the specified bounds (inclusive)
        /// </summary>
        /// <param name="minMajor">Major version of the minimum version</param>
        /// <param name="minMinor">Minor version of the minimum version</param>
        /// <param name="maxMajor">Major version of the maximum version</param>
        /// <param name="maxMinor">Minor version of the maximum version</param>
        /// <returns>TRUE if the version is between the specified bounds</returns>
        public static bool IsBetween(uint minMajor, uint minMinor, uint maxMajor, uint maxMinor)
        {
            uint winMajor = WinMajorVersion;
            uint winMinor = WinMinorVersion;

            if (winMajor < minMajor)
            {
                return false;
            }
            else if (winMajor == minMajor)
            {
                return winMinor >= minMinor;
            }
            else if (winMajor < maxMajor)
            {
                return true;
            }
            else if (winMajor == maxMajor)
            {
                return winMinor <= maxMinor;
            }
            else return false;
        }

        /// <summary>
        /// Check if the current Windows version is XP/2003
        /// </summary>
        public static bool IsXP
        {
            get
            {
                return WinMajorVersion == 5 && (WinMinorVersion == 1 || WinMinorVersion == 2);
            }
        }

        /// <summary>
        /// Check if the current Windows version is at least XP/2003
        /// </summary>
        public static bool IsAtLeastXP
        {
            get
            {
                return WinMajorVersion > 5 || (WinMajorVersion == 5 && WinMinorVersion >= 1);
            }
        }

        /// <summary>
        /// Check if the current Windows version is Vista/2008
        /// </summary>
        public static bool IsVista
        {
            get
            {
                return WinMajorVersion == 6 && WinMinorVersion == 0;
            }
        }

        /// <summary>
        /// Check if the current Windows version is Vista/2008
        /// </summary>
        public static bool IsAtLeastVista
        {
            get
            {
                return WinMajorVersion >= 6;
            }
        }

        /// <summary>
        /// Check if the current Windows version is 7/2008 R2
        /// </summary>
        public static bool Is7
        {
            get
            {
                return WinMajorVersion == 6 && WinMinorVersion == 1;
            }
        }

        /// <summary>
        /// Check if the current Windows version is Vista/2008
        /// </summary>
        public static bool IsAtLeast7
        {
            get
            {
                return WinMajorVersion > 6 || (WinMajorVersion == 6 && WinMinorVersion >= 1);
            }
        }

        /// <summary>
        /// Check if the current Windows version is 8, 8.1 or 2012
        /// </summary>
        public static bool Is8
        {
            get
            {
                return WinMajorVersion == 6 && (WinMinorVersion == 2 || WinMinorVersion == 3);
            }
        }

        /// <summary>
        /// Check if the current Windows version is Vista/2008
        /// </summary>
        public static bool IsAtLeast8
        {
            get
            {
                return WinMajorVersion > 6 || (WinMajorVersion == 6 && WinMinorVersion >= 2);
            }
        }

        /// <summary>
        /// Check if the current Windows version is 10
        /// </summary>
        public static bool Is10
        {
            get
            {
                return WinMajorVersion == 10 && WinMinorVersion == 0 && !Is11;
            }
        }

        /// <summary>
        /// Check if the current Windows version is Vista/2008
        /// </summary>
        public static bool IsAtLeast10
        {
            get
            {
                return WinMajorVersion >= 10;
            }
        }

        /// <summary>
        /// Check if the current Windows version is 11
        /// </summary>
        public static bool Is11
        {
            get
            {
                return FriendlyName.ToLowerInvariant().Contains("windows 11");
            }
        }
    }
}
