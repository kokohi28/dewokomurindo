using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MataDewa4
{
    static class Program
    {
        //...Instantiate first
        public static Main m_MainInstance;
        // static Avionic m_AvinonicInstance;

        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // instance Main
            m_MainInstance = new Main();
            // m_AvinonicInstance = new Avionic();

            Application.Run(m_MainInstance); //Main Form
            // Application.Run(m_AvinonicInstance); //Instrument Form
        }
    }
}