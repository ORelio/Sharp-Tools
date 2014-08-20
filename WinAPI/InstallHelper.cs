using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SharpTools
{
    class InstallHelper
    {
        /// <summary>
        /// Retrieve uninstall command for a MSI package by package display name.
        /// Code from nullskull.com/q/10397219/display.aspx
        /// </summary>
    
        private static string GetUninstallCommandFor(string productDisplayName)
        {
            Microsoft.Win32.RegistryKey localMachine = Microsoft.Win32.Registry.LocalMachine;
            string productsRoot = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            Microsoft.Win32.RegistryKey products = localMachine.OpenSubKey(productsRoot);
            string[] productFolders = products.GetSubKeyNames();

            foreach (string p in productFolders)
            {
                Microsoft.Win32.RegistryKey installProperties = products.OpenSubKey(p);
                if (installProperties != null)
                {
                    string displayName = (string)installProperties.GetValue("DisplayName");
                    if ((displayName != null) && (displayName.Contains(productDisplayName)))
                    {
                        string uninstallCommand = (string)installProperties.GetValue("UninstallString");
                        return uninstallCommand;
                    }
                }
            }
            return "";
        }
    }
}
