using MeekStudio.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace MeekStudio
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if(Properties.Settings.Default.NeedUpdate)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.NeedUpdate = false;
            }

            SetLanguage();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }

        /// <summary>
        /// Locale currently in use
        /// </summary>
        public static CultureInfo Locale { get; private set; }
        /// <summary>
        /// Sets the application language to the language code
        /// </summary>
        /// <param name="lang">Language code such as <c>en</c>, <c>ru</c>, <c>ja</c>, <c>nl</c></param>
        static public void SetLanguage(string lang)
        {
            CultureInfo ci = new CultureInfo(lang);
            ci.NumberFormat = CultureInfo.InvariantCulture.NumberFormat;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = ci;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = ci;
            Settings.Default.Lang = lang;
            Settings.Default.Save();
        }

        /// <summary>
        /// Sets the application language to the user-selected language
        /// </summary>
        static public void SetLanguage()
        {
            if (Settings.Default.Lang != null)
            {
                SetLanguage(Settings.Default.Lang);
            } 
            else
            {
                SetLanguage("en");
            }
        }
    }
}
