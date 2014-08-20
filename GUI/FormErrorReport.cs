using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace SharpTools
{
    /// <summary>
    /// A form for showing error reports to the user, with a copy button to facilitate bug reporting.
    /// By ORelio - (c) 2014 - Available under the CDDL-1.0 license
    /// </summary>

    public partial class FormErrorReport : Form
    {
        private string error_report;

        /// <summary>
        /// Create a new error report form with a custom title, message and content
        /// </summary>
        /// <param name="title">Title of the window</param>
        /// <param name="message">Message to show to the user</param>
        /// <param name="error_report">The error report</param>

        public FormErrorReport(string title, string message, string error_report)
        {
            InitializeComponent();
            this.error_report = error_report.Replace("\r", "").Replace("\n", "\r\n");
            Box_ErrorReport.Text = error_report;
            label_Message.Text = message;
            this.Text = title;
        }

        /// <summary>
        /// Close the window by clicking OK
        /// </summary>

        private void button_OK_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Copy error report to the clipboard
        /// </summary>

        private void button_Copy_Click(object sender, EventArgs e)
        {
            WriteToClipboard(error_report);
        }

        /// <summary>
        /// Write a string to the clipboard
        /// </summary>

        private static string WriteToClipboard(string text)
        {
            string clipdata = "";
            Thread staThread = new Thread(new ThreadStart(
                delegate
                {
                    try
                    {
                        Clipboard.SetText(text);
                    }
                    catch { }
                }
            ));
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return clipdata;
        }
    }
}
