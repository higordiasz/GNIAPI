using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GNIAPI.Controllers.Users;

namespace GNIAPI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Token { get; set; }
        public static string GrupoName { get; set; }
        public static bool Humanizacao { get; set; }
        public static string[] arguments { get; set; }
        public static string TypeUserAgent { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            arguments = e.Args;
            if (e.Args.Length > 3)
            {
                Token = e.Args[0];
                GrupoName = e.Args[1];
                Humanizacao = e.Args[2] == "true";
                TypeUserAgent = e.Args[3];
                var check = UserController.CheckToken(Token);
                bool checkToken = check.Status == 1 ? true : false; //Checar o token
                if (!checkToken)
                {
                    MessageBox.Show(check.Erro);
                    System.Windows.Application.Current.Shutdown();
                }
            } else
            {
                MessageBox.Show($"Erro argumentos: {e.Args}");
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}