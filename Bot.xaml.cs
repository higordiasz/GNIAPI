using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GNIAPI.Controllers;
using GNIAPI.Models;
using GNIAPI.Models.Instagrams;
using GNIAPI.Controllers.Actions;
using GNIAPI.Controllers.HelpInstaAPI;
using GNIAPI.Models.Blocks;
using GNIAPI.Models.ActionModels;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using Block = GNIAPI.Models.Blocks.Block;
using System.Windows.Media;
using GNIAPI.WebInstagram;

namespace GNIAPI
{
    public static class Variaveis
    {
        private static string _token = "";
        private static string _grupo = "";
        private static string _contaatual = "";
        private static string _typeua = "seed";
        private static double _total = 0;
        private static double _saldogerado = 0.0;
        private static double _totalseguir = 0;
        private static double _totalcurtir = 0;
        private static double _meta = 0;
        private static double _totalmeta = 0;
        private static double _metasalcansadas = 0;
        private static double _totalconta = 0;
        private static bool _humanizacao = false;
        public static string Token { get { return _token; } set { _token = value; } }
        public static string Grupo { get { return _grupo; } set { _grupo = value; } }
        public static string Conta_Atual { get { return _contaatual; } set { _contaatual = value; } }
        public static string TypeUserAgent { get { return _typeua; } set { _typeua = value; } }
        public static double Total { get { return _total; } set { _total = value; } }
        public static double Saldo { get { return _saldogerado; } set { _saldogerado = value; } }
        public static double TotalSeguir { get { return _totalseguir; } set { _totalseguir = value; } }
        public static double TotalCurtir { get { return _totalcurtir; } set { _totalcurtir = value; } }
        public static double Meta { get { return _meta; } set { _meta = value; } }
        public static double TotalMeta { get { return _totalmeta; } set { _totalmeta = value; } }
        public static double MetaAlcancada { get { return _metasalcansadas; } set { _metasalcansadas = value; } }
        public static double TotalContaAtual { get { return _totalconta; } set { _totalconta = value; } }
        public static bool Humanizacao { get { return _humanizacao; } set { _humanizacao = value; } }
        public static string Verde { get { return "#0CE855"; } }
        public static string Amarelo { get { return "#FFE033"; } }
        public static string Vermelho { get { return "#F15060"; } }
        public static string Ciano { get { return "#70DFDF"; } }
        public static string Branco { get { return "#FFFFFF"; } }
        public static string Magenta { get { return "#221154"; } }
        public static string Azul { get { return "#45CEFF"; } }
        public static string AzulEscuro { get { return "#005DDE"; } }
        public static string CianoEscuro { get { return "#50BFC3"; } }
        public static string CinzaEscuro { get { return "#D7E1F3"; } }
        public static string VerdeEscuro { get { return "#4CCB70"; } }
        public static string VermelhoEscuro { get { return "#F51A24"; } }
        public static string AmareloEscuro { get { return "#FEDE4B"; } }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Bot : Window
    {
        static GNI.GNI Plat { get; set; }
        static private Models.Grupos.Grupo Grupo { get; set; }
        static private Models.Globals.Global Global { get; set; }
        static private List<Instagram> Contas { get; set; }
        static private Bloqueios Bloqueios { get; set; }
        static private List<string> Challenges { get; set; }
        static private List<string> Incorrects { get; set; }
        static private List<InstaController> InstaController { get; set; }
        static private string[] argumentos { get { return App.arguments; } set { App.arguments = value; } }

        static private int Index = 0;

        private DateTime inicio = DateTime.Now;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        private string NormalCollor = "#07bb9f";
        private string BlockCollor = "#f5c300";
        private string ChallengeCollor = "#f03c22";
        private string IncorrectCollor = "#fe8495";

        public Bot()
        {
            InitializeComponent();
            Variaveis.Grupo = App.GrupoName;
            argumentos = App.arguments;
            Variaveis.Token = App.Token;
            Variaveis.Humanizacao = App.Humanizacao;
            Variaveis.TypeUserAgent = App.TypeUserAgent;
            dispatcherTimer.Tick += new EventHandler(TimerTick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            tx_curtir.Text = "0";
            tx_meta.Text = "0/0";
            tx_saldo.Text = "$0,0";
            tx_seguir.Text = "0";
            tx_tconta.Text = "0";
            tx_total.Text = "0";
            tx_username.Text = "@";
            _ = Iniciar();
        }

        // 1 = Roxo #221154 : 2 = Verde #07BB9F : 3 = Amarelo #F5C300 : 4 = Vermelho #F03C22 : 5 Rosa = #FE8495 : 6 = Azul #0052cc : 7 = Laranja #FD4731
        private async Task ConsoleMessage(string message, int c)
        {
            var color = c == 1 ? "#221154" : c == 2 ? "#07BB9F" : c == 3 ? "#F5C300" : c == 4 ? "#F03C22" : c == 5 ? "#FE8495" : c == 6 ? "#0052cc" : "#FD4731";
            var bc = new BrushConverter();
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(message);
            paragraph.Foreground = (Brush)bc.ConvertFrom(color);
            BotConsole.Document.Blocks.Add(paragraph);
            BotConsole.Focus();
            BotConsole.ScrollToEnd();
            await Task.Delay(300);
        }

        private async Task ConsoleMessage2 (string message, string cor = "#221154")
        {
            var bc = new BrushConverter();
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(message);
            paragraph.Foreground = (Brush)bc.ConvertFrom(cor);
            BotConsole.Document.Blocks.Add(paragraph);
            BotConsole.Focus();
            BotConsole.ScrollToEnd();
            await Task.Delay(300);
        }

        private async Task SetBorder (int c)
        {
            var color = c == 1 ? "#221154" : c == 2 ? "#07BB9F" : c == 3 ? "#F5C300" : c == 4 ? "#F03C22" : c == 5 ? "#FE8495" : "#0052cc";
            var bc = new BrushConverter();
            BorderImage.BorderBrush = (Brush)bc.ConvertFrom(color);
            await Task.Delay(300);
        }

        private async Task SetBorderColor (string color)
        {
            var bc = new BrushConverter();
            BorderImage.BorderBrush = (Brush)bc.ConvertFrom(color);
            await Task.Delay(300);
        }

        private void MenuStrip_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuStrip.Visibility = Visibility.Hidden;
        }

        private void MenuInicio_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TelaDados.Visibility = Visibility.Visible;
            TelaConsole.Visibility = Visibility.Hidden;
            TelaLista.Visibility = Visibility.Hidden;
            MenuStrip.Visibility = Visibility.Hidden;
        }

        private void MenuConsole_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TelaDados.Visibility = Visibility.Hidden;
            TelaConsole.Visibility = Visibility.Visible;
            TelaLista.Visibility = Visibility.Hidden;
            MenuStrip.Visibility = Visibility.Hidden;
        }

        private void MenuContas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            List<InstaList> lsit = new List<InstaList>();
            foreach (var i in Contas)
            {
                var aux = new InstaList();
                aux.Username = i.Username;
                aux.Color = i.Incorrect ? IncorrectCollor : i.Challeng ? ChallengeCollor : i.Block ? BlockCollor  :  NormalCollor;
                if (InstaController.Exists( c => c.Insta.Username == i.Username))
                {
                    var j = InstaController.FindIndex(c => c.Insta.Username == i.Username);
                    aux.Seguir = InstaController[j].Seguir.ToString();
                    aux.Curtir = InstaController[j].Curtir.ToString();
                    aux.Url = InstaController[j].PictureURL;
                } else
                {
                    aux.Seguir = "0";
                    aux.Curtir = "0";
                    aux.Url = "https://imgur.com/b24Rzo7.jpg";
                }
                lsit.Add(aux);
            }
            ListaContasList.ItemsSource = null;
            ListaContasList.ItemsSource = lsit;
            TelaDados.Visibility = Visibility.Hidden;
            TelaConsole.Visibility = Visibility.Hidden;
            TelaLista.Visibility = Visibility.Visible;
            MenuStrip.Visibility = Visibility.Hidden;
        }

        private void bt_menu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MenuStrip.Visibility = Visibility.Visible;
        }

        private async Task AddSaldo(double Valor)
        {
            Variaveis.Saldo += Valor;
            tx_saldo.Text = $"${Variaveis.Saldo.ToString("N3")}";
            await Task.Delay(102);
        }

        private void Show (int i) {
            Variaveis.TotalSeguir = InstaController[i].Seguir;
            Variaveis.TotalCurtir = InstaController[i].Curtir;
            Variaveis.TotalContaAtual = InstaController[i].Total;
            Variaveis.Conta_Atual = InstaController[i].Insta.Username;
            tx_seguir.Text = Variaveis.TotalSeguir.ToString();
            tx_curtir.Text = Variaveis.TotalCurtir.ToString();
            tx_tconta.Text = Variaveis.TotalContaAtual.ToString();
            ProfilePic.ImageSource = new BitmapImage(new Uri(InstaController[i].PictureURL));
        }

        private void TimerTick(object sender, EventArgs e)
        {
            DateTime agora = DateTime.Now;
            TimeSpan span = agora.Subtract(inicio);
            string time = $"{span.ToString(@"hh\:mm\:ss")}";
            tx_timer.Text = time;
        }

        private void BotBorderTop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void TextBlock_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //Novas funções

        async Task Iniciar()
        {
            BotGrupoName.Text = App.GrupoName;
            await ConsoleMessage2("Carregando dados do servidor...");
            if (String.IsNullOrEmpty(Variaveis.Grupo))
            {
                //Close
                LogMessage("Não foi possivel carregar o nome do grupo");
                await ConsoleMessage2("Erro ao carregar o grupo", Variaveis.Vermelho);
                return;
            }
            Models.Grupos.Grupo ret;
            if (argumentos.Length > 4)
            {
                ret = new Models.Grupos.Grupo
                {
                    Contas = new List<string>(),
                    Global = Variaveis.Grupo,
                    Nome = Variaveis.Grupo
                };
                for (var i = 4; i < argumentos.Length; i++)
                {
                    ret.Contas.Add(argumentos[i]);
                }
            }
            else
            {
                ret = Models.Grupos.ExtendGrupos.GetGroupByname(Variaveis.Grupo);
            }
            if (ret == null)
            {
                //Close
                LogMessage($"Não foi possivel localizar o grupo com nome '{Variaveis.Grupo}'");
                await ConsoleMessage2("Erro ao carregar o grupo com o nome " + Variaveis.Grupo, Variaveis.Vermelho);
                return;
            }
            Grupo = ret;
            if (Grupo.Contas.Count <= 0)
            {
                LogMessage($"O grupo '{Variaveis.Grupo}' não possui contas para rodar o bot");
                await ConsoleMessage2($"O grupo {Variaveis.Grupo} não possui contas para rodar o bot", Variaveis.Vermelho);
                return;
            }
            var resGlobal = Models.Globals.ExtendsGlobal.GetGlobalByname(Grupo.Global);
            if (resGlobal == null)
            {
                LogMessage($"Não foi possivel localizar o global com nome '{Grupo.Global}'");
                await ConsoleMessage2($"Não foi possivel localizar o global com o nome {Grupo.Global}", Variaveis.Vermelho);
                return;
            }
            Global = resGlobal;
            Contas = new List<Models.Instagrams.Instagram>();
            foreach (string username in Grupo.Contas)
            {
                var res = Models.Instagrams.ExtendInstagram.GetInstaByUsername(username);
                if (res != null)
                {
                    Contas.Add(res);
                }
            }
            if (Contas.Count > 0)
            {
                await IniciarSistema();
            }
            else
            {
                LogMessage($"Não foi possivel carregar suas contas do instagram.");
                await ConsoleMessage2("Não foi possivel carregar suas contas do instagram", Variaveis.Vermelho);
                return;
            }
        }

        private async Task IniciarSistema()
        {
            await ConsoleMessage2("Todos os dados foram carregados do servidor, iniciando o sistema.");
            await Task.Delay(1000);
            await ConsoleMessage2("Realizando login na plataforma.");
            var dir = Directory.GetCurrentDirectory();
            if (File.Exists($@"{dir}\Config\gni.txt"))
            {
                string[] linhas = File.ReadAllLines($@"{dir}\Config\gni.txt");
                if (linhas.Length == 1)
                {
                    string GT = linhas[0];
                    Plat = new GNI.GNI(GT);
                    if (Plat.LoadComplet)
                    {
                        await RodarCiclo();
                    }
                    else
                    {
                        await ConsoleMessage2("Erro ao conectar com o GNI", Variaveis.Vermelho);
                        return;
                    }
                }
                else
                {
                    LogMessage($"Não foi possivel carregar o login da plataforma");
                    await ConsoleMessage2("Erro ao carregar o token da plataforma", Variaveis.Vermelho);
                    return;
                }
            }
            else
            {
                LogMessage($"Não foi possivel carregar o login da plataforma");
                await ConsoleMessage2("Erro ao carregar token da plataforma", Variaveis.Vermelho);
                return;
            }
        }

        private async Task RodarCiclo()
        {
            Bloqueios = new Models.Blocks.Bloqueios();
            Incorrects = new List<string>();
            Challenges = new List<string>();
            InstaController = new List<Models.Instagrams.InstaController>();
            Variaveis.Meta = Global.Meta;
            await ConsoleMessage2("Login efetuado na plataforma", Variaveis.Verde);
            await Task.Delay(500);
            await ConsoleMessage2("Iniciando o bot");
            try
            {
                while (true)
                {
                    for (int i = 0; i < Contas.Count; i++)
                    {
                        if (Contas[i].Block)
                        {
                            Bloqueios.LimparLista();
                            if (Bloqueios.Blocks.Exists(c => c.Username == Contas[i].Username))
                            {
                                await ConsoleMessage2($"Conta '{Contas[i].Username}' está bloqueada temporariamente.", Variaveis.Amarelo);
                            }
                            else
                            {
                                //Rodar conta
                                await ConsoleMessage2($"Conta: '{Contas[i].Username}'");
                                Index = i;
                                await RodarConta();
                            }
                        }
                        else
                        {
                            if (!Challenges.Exists(c => c == Contas[i].Username) && !Incorrects.Exists(c => c == Contas[i].Username))
                            {
                                //Rodar conta
                                await ConsoleMessage2($"Conta: '{Contas[i].Username}'");
                                Index = i;
                                await RodarConta();
                            }
                        }
                        if (Contas.Count < 2)
                        {
                            if (Challenges.Count == Contas.Count)
                            {
                                await ConsoleMessage2("Não possui contas para continuar", Variaveis.Magenta);
                                await Task.Delay(TimeSpan.FromHours(10));
                                return;
                            }
                            else
                            {
                                if ((Bloqueios.Blocks.Count + Challenges.Count) == Contas.Count)
                                {
                                    await ConsoleMessage2("Não possui contas para continuar", Variaveis.Magenta);
                                    await ConsoleMessage2("Aguardando 10 minutos", Variaveis.Ciano);
                                    await Task.Delay(TimeSpan.FromMinutes(10));
                                }
                            }
                        }
                        else
                        {
                            int t = 0;
                            foreach (var ig in Contas)
                            {
                                if (!ig.Challeng && !ig.Incorrect)
                                {
                                    t++;
                                }
                            }
                            if (t == 0)
                            {
                                await ConsoleMessage2("Não possui contas para continuar", Variaveis.Magenta);
                                await Task.Delay(TimeSpan.FromHours(10));
                                return;
                            }
                            else
                            {
                                if ((Bloqueios.Blocks.Count + Challenges.Count) == Contas.Count)
                                {
                                    await ConsoleMessage2("Não possui contas para continuar", Variaveis.Magenta);
                                    await ConsoleMessage2("Aguardando 10 minutos", Variaveis.Ciano);
                                    await Task.Delay(TimeSpan.FromMinutes(10));
                                }
                            }
                        }
                        await ConsoleMessage2("Aguardando tempo entre contas", Variaveis.Ciano);
                        await Task.Delay(TimeSpan.FromSeconds(Global.Timer_contas));
                    }
                }
            }
            catch (Exception err)
            {
                LogMessage($"Erro ao rodar 'Ciclo' : {err.Message}");
                await ConsoleMessage2("Um erro inesperado aconteceu, contact o suporte", Variaveis.VerdeEscuro);
                return;
            }
        }

        private async Task RodarConta()
        {
            try
            {
                await SetBorderColor(Variaveis.Magenta);
                Variaveis.Conta_Atual = "@" + Contas[Index].Username;
                tx_username.Text = Variaveis.Conta_Atual;
                int i = -1;
                if (InstaController.Exists(ig => ig.Insta.Username == Contas[Index].Username))
                {
                    i = InstaController.FindIndex(inst => inst.Insta.Username == Contas[Index].Username);
                }
                if (i < 0)
                {
                    var res = Plat.CheckInsta(Contas[Index].Username);
                    if (res.Status == 1)
                    {
                        Models.Instagrams.InstaController ig = new Models.Instagrams.InstaController
                        {
                            Insta = Contas[Index],
                            isLogged = false,
                            PictureURL = "https://imgur.com/b24Rzo7.jpg",
                            ContaID = res.Json.ContaID,
                            Web = null,
                            Seguir = 0,
                            Curtir = 0,
                            Total = 0
                        };
                        InstaController.Add(ig);
                        i = InstaController.Count - 1;
                    }
                    else
                    {
                        await ConsoleMessage2($"Náo foi possivel localizar a conta '{Contas[Index].Username}' na plataforma", Variaveis.CianoEscuro);
                        return;
                    }
                }
                if (!InstaController[i].isLogged)
                {
                    await ConsoleMessage2($"Realizando login na conta '{Contas[Index].Username}'");
                    UserDate sessionData = new UserDate()
                    {
                        Username = Contas[Index].Username.ToLower(),
                        Password = Contas[Index].Password
                    };
                    InstagramWeb Web = null;
                    if (Variaveis.TypeUserAgent == "txt")
                    {
                        var dir = Directory.GetCurrentDirectory();
                        string[] linhas = File.ReadAllLines($@"{dir}\Config\useragent.txt");
                        if (linhas.Length > 0)
                        {
                            var rand = new Random();
                            var li = rand.Next(0, linhas.Length);
                            Web = new InstagramWeb(sessionData, true, Variaveis.TypeUserAgent, linhas[li]);
                        }
                        else
                        {
                            Web = new InstagramWeb(sessionData, true, "seed");
                        }
                    }
                    else
                    {
                        Web = new InstagramWeb(sessionData, true, Variaveis.TypeUserAgent);
                    }
                    InstaController[i].Web = Web;
                    var login = await Login(Web);
                    if (login.Status != 1)
                    {
                        switch (login.Status)
                        {
                            case 2:
                                await ConsoleMessage2(login.Response, Variaveis.Amarelo);
                                await SetBorderColor(Variaveis.Amarelo);
                                Contas[Index].AdicionarBlock();
                                Bloqueios.AdicionarBlock(Contas[Index].Username, Global.Timer_Block);
                                await Task.Delay(1548);
                                break;
                            case 5:
                                await ConsoleMessage2(login.Response, Variaveis.VermelhoEscuro);
                                await SetBorderColor(Variaveis.VermelhoEscuro);
                                Contas[Index].AdicionarChallenge();
                                Challenges.Add(Contas[Index].Username);
                                await Task.Delay(1548);
                                break;
                            case 6:
                                await ConsoleMessage2(login.Response, Variaveis.AzulEscuro);
                                await SetBorderColor(Variaveis.AzulEscuro);
                                Contas[Index].AdicionarChallenge();
                                Challenges.Add(Contas[Index].Username);
                                await Task.Delay(1548);
                                break;
                            case 7:
                                await ConsoleMessage2(login.Response, Variaveis.Vermelho);
                                await SetBorderColor(Variaveis.Vermelho);
                                Contas[Index].AdicionarChallenge();
                                Challenges.Add(Contas[Index].Username);
                                await Task.Delay(1548);
                                break;
                            case 8:
                                await ConsoleMessage2(login.Response, Variaveis.Ciano);
                                await SetBorderColor(Variaveis.Ciano);
                                Contas[Index].AdicionarChallenge();
                                Challenges.Add(Contas[Index].Username);
                                await Task.Delay(1548);
                                break;
                            case 9:
                                await ConsoleMessage2(login.Response, Variaveis.CinzaEscuro);
                                await SetBorderColor(Variaveis.CinzaEscuro);
                                Contas[Index].AdicionarChallenge();
                                Challenges.Add(Contas[Index].Username);
                                await Task.Delay(1548);
                                break;
                            case 10:
                                await ConsoleMessage2(login.Response, Variaveis.CinzaEscuro);
                                await SetBorderColor(Variaveis.CinzaEscuro);
                                Contas[Index].AdicionarChallenge();
                                Challenges.Add(Contas[Index].Username);
                                await Task.Delay(1548);
                                break;
                            case 11:
                                await ConsoleMessage2(login.Response, Variaveis.CianoEscuro);
                                await SetBorderColor(Variaveis.CianoEscuro);
                                Contas[Index].AdicionarChallenge();
                                Challenges.Add(Contas[Index].Username);
                                await Task.Delay(1548);
                                break;
                            case 12:
                                await ConsoleMessage2(login.Response, Variaveis.Magenta);
                                Contas[Index].AdicionarIncorrect();
                                Incorrects.Add(Contas[Index].Username);
                                await Task.Delay(1548);
                                break;
                            default:
                                await ConsoleMessage2(login.Response, Variaveis.CinzaEscuro);
                                await SetBorderColor(Variaveis.CinzaEscuro);
                                Contas[Index].AdicionarChallenge();
                                Challenges.Add(Contas[Index].Username);
                                await Task.Delay(1548);
                                break;
                        }
                        await ConsoleMessage2("Indo para a próxima conta");
                        return;
                    }
                    else
                    {
                        var prof = await Web.GetUserProfileByUsernameAsync(Contas[Index].Username.ToLower());
                        if (prof.Status == 1)
                        {
                            InstaController[i].PictureURL = prof.Json.profile_pic_url_hd.ToString();
                        }
                        InstaController[i].isLogged = true;
                        Contas[Index].RemoverChallenge();
                        Contas[Index].RemoverIncorrect();
                        await ConsoleMessage2("Login realizado com sucesso", Variaveis.Verde);
                    }
                }
                else
                {
                    var check = await IsLogged(InstaController[i].Web);
                    if (!check)
                    {
                        await ConsoleMessage2($"Realizando login na conta '{Contas[Index].Username}'");
                        UserDate sessionData = new UserDate()
                        {
                            Username = Contas[Index].Username.ToLower(),
                            Password = Contas[Index].Password
                        };
                        InstagramWeb Web = null;
                        if (Variaveis.TypeUserAgent == "txt")
                        {
                            var dir = Directory.GetCurrentDirectory();
                            string[] linhas = File.ReadAllLines($@"{dir}\Config\useragent.txt");
                            if (linhas.Length > 0)
                            {
                                var rand = new Random();
                                var li = rand.Next(0, linhas.Length);
                                Web = new InstagramWeb(sessionData, true, Variaveis.TypeUserAgent, linhas[li]);
                            }
                            else
                            {
                                Web = new InstagramWeb(sessionData, true, "seed");
                            }
                        }
                        else
                        {
                            Web = new InstagramWeb(sessionData, true, Variaveis.TypeUserAgent);
                        }
                        InstaController[i].Web = Web;
                        var login = await Login(Web);
                        if (login.Status != 1)
                        {
                            switch (login.Status)
                            {
                                case 2:
                                    await ConsoleMessage2(login.Response, Variaveis.Amarelo);
                                    await SetBorderColor(Variaveis.Amarelo);
                                    Contas[Index].AdicionarBlock();
                                    Bloqueios.AdicionarBlock(Contas[Index].Username, Global.Timer_Block);
                                    await Task.Delay(1548);
                                    break;
                                case 5:
                                    await ConsoleMessage2(login.Response, Variaveis.VermelhoEscuro);
                                    await SetBorderColor(Variaveis.VermelhoEscuro);
                                    Contas[Index].AdicionarChallenge();
                                    Challenges.Add(Contas[Index].Username);
                                    await Task.Delay(1548);
                                    break;
                                case 6:
                                    await ConsoleMessage2(login.Response, Variaveis.AzulEscuro);
                                    await SetBorderColor(Variaveis.AzulEscuro);
                                    Contas[Index].AdicionarChallenge();
                                    Challenges.Add(Contas[Index].Username);
                                    await Task.Delay(1548);
                                    break;
                                case 7:
                                    await ConsoleMessage2(login.Response, Variaveis.Vermelho);
                                    await SetBorderColor(Variaveis.Vermelho);
                                    Contas[Index].AdicionarChallenge();
                                    Challenges.Add(Contas[Index].Username);
                                    await Task.Delay(1548);
                                    break;
                                case 8:
                                    await ConsoleMessage2(login.Response, Variaveis.Ciano);
                                    await SetBorderColor(Variaveis.Ciano);
                                    Contas[Index].AdicionarChallenge();
                                    Challenges.Add(Contas[Index].Username);
                                    await Task.Delay(1548);
                                    break;
                                case 9:
                                    await ConsoleMessage2(login.Response, Variaveis.CinzaEscuro);
                                    await SetBorderColor(Variaveis.CinzaEscuro);
                                    Contas[Index].AdicionarChallenge();
                                    Challenges.Add(Contas[Index].Username);
                                    await Task.Delay(1548);
                                    break;
                                case 10:
                                    await ConsoleMessage2(login.Response, Variaveis.CinzaEscuro);
                                    await SetBorderColor(Variaveis.CinzaEscuro);
                                    Contas[Index].AdicionarChallenge();
                                    Challenges.Add(Contas[Index].Username);
                                    await Task.Delay(1548);
                                    break;
                                case 11:
                                    await ConsoleMessage2(login.Response, Variaveis.CianoEscuro);
                                    await SetBorderColor(Variaveis.CianoEscuro);
                                    Contas[Index].AdicionarChallenge();
                                    Challenges.Add(Contas[Index].Username);
                                    await Task.Delay(1548);
                                    break;
                                case 12:
                                    await ConsoleMessage2(login.Response, Variaveis.Magenta);
                                    Contas[Index].AdicionarIncorrect();
                                    Incorrects.Add(Contas[Index].Username);
                                    await Task.Delay(1548);
                                    break;
                                default:
                                    await ConsoleMessage2(login.Response, Variaveis.CinzaEscuro);
                                    await SetBorderColor(Variaveis.CinzaEscuro);
                                    Contas[Index].AdicionarChallenge();
                                    Challenges.Add(Contas[Index].Username);
                                    await Task.Delay(1548);
                                    break;
                            }
                            await ConsoleMessage2("Indo para a próxima conta");
                            return;
                        }
                        else
                        {
                            InstaController[i].isLogged = true;
                            Contas[Index].RemoverChallenge();
                            Contas[Index].RemoverIncorrect();
                            await ConsoleMessage2("Login realizado com sucesso", Variaveis.Verde);
                        }
                    }
                    else
                    {
                        await ConsoleMessage2("Realizando login na conta do Instagram");
                        await Task.Delay(1478);
                        await ConsoleMessage2("Conta já estava conectada");
                    }
                }
                await Task.Delay(978);
                Show(i);
                int k = 0;
                bool sair = false;
                var r = new Random();
                while (k < Global.Quantidade && sair == false)
                {
                    await SetBorderColor(Variaveis.Magenta);
                    await ConsoleMessage2($"Buscando tarefa para realizar {k + 1}/{Global.Quantidade}");
                    var task = await Plat.GetTask(InstaController[i].ContaID);
                    if (task.Status != 1 && task.Status != 2)
                    {
                        int max = Global.Trocar ? 3 : 10;
                        int x = 0;
                        while (x < max && task.Status != 1 && task.Status != 2)
                        {
                            await Task.Delay(4589);
                            task = await Plat.GetTask(InstaController[i].ContaID);
                            x++;
                        }
                    }
                    if (task.Status == 1 || task.Status == 2)
                    {
                        await ConsoleMessage2("Tarefa encontrada | " + task.Response);
                        if (task.Status == 1)
                        {
                            string alvo = "";
                            if (task.Json.URL.ToString().IndexOf("instagram.com") > -1)
                            {
                                var array = task.Json.URL.ToString().Split("/");
                                if (array[array.Length - 1] == "")
                                    alvo = array[array.Length - 2];
                                else
                                    alvo = array[array.Length - 1];
                            }
                            else
                            {
                                alvo = task.Json.URL;
                            }
                            await ConsoleMessage2($"Seguindo o perfil '{alvo}'");
                            var seguir = await Seguir(alvo, InstaController[i].Web);
                            if (seguir.Status == 1 || seguir.Status == 0)
                            {
                                await ConsoleMessage2("Sucesso ao seguir o perfil");
                                if (k == 0)
                                {
                                    InstaController[i].Insta.RemoverBlock();
                                }
                                var confirm = await Plat.ConfirmTask(InstaController[i].ContaID, task.Json.PedidoID.ToString());
                                if (confirm.Status == 1)
                                {
                                    await SetBorderColor(Variaveis.Verde);
                                    await ConsoleMessage2("Tarefa realizada com sucesso", Variaveis.Verde);
                                    await AddTarefa(0, i);
                                    k++;
                                }
                                else
                                {
                                    await ConsoleMessage2("Erro ao confirmar a tarefa na Dizu", Variaveis.Vermelho);
                                }
                            }
                            else
                            {
                                if (seguir.Status != 0 && seguir.Status != 3 && seguir.Status != 4)
                                {
                                    var check = await IsBlockOrChallenge(InstaController[i].Web);
                                    switch (check.Status)
                                    {
                                        case 2:
                                            await ConsoleMessage2(check.Response, Variaveis.Amarelo);
                                            await SetBorderColor(Variaveis.Amarelo);
                                            Contas[Index].AdicionarBlock();
                                            Bloqueios.AdicionarBlock(Contas[Index].Username, Global.Timer_Block);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 5:
                                            await ConsoleMessage2(check.Response, Variaveis.VermelhoEscuro);
                                            await SetBorderColor(Variaveis.VermelhoEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 6:
                                            await ConsoleMessage2(check.Response, Variaveis.AzulEscuro);
                                            await SetBorderColor(Variaveis.AzulEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 7:
                                            await ConsoleMessage2(check.Response, Variaveis.Vermelho);
                                            await SetBorderColor(Variaveis.Vermelho);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 8:
                                            await ConsoleMessage2(check.Response, Variaveis.Ciano);
                                            await SetBorderColor(Variaveis.Ciano);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 9:
                                            await ConsoleMessage2(check.Response, Variaveis.CinzaEscuro);
                                            await SetBorderColor(Variaveis.CinzaEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 10:
                                            await ConsoleMessage2(check.Response, Variaveis.CinzaEscuro);
                                            await SetBorderColor(Variaveis.CinzaEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 11:
                                            await ConsoleMessage2(check.Response, Variaveis.CianoEscuro);
                                            await SetBorderColor(Variaveis.CianoEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            break;
                                        default:
                                            await ConsoleMessage2(check.Response, Variaveis.CinzaEscuro);
                                            await SetBorderColor(Variaveis.CinzaEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                    }
                                }
                                else
                                {
                                    await ConsoleMessage2(seguir.Response, Variaveis.Azul);
                                    await SetBorderColor(Variaveis.Azul);
                                    var pular = await Plat.JunpTask(InstaController[i].ContaID, task.Json.PedidoID.ToString());
                                    if (pular.Status == 1)
                                        await ConsoleMessage2("Tarefa pulada");
                                    else
                                        await ConsoleMessage2("Erro ao pular a tarefa");
                                }
                            }
                        }
                        else
                        {
                            string link = "";
                            if (task.Json.URL.ToString().IndexOf("instagram.com") > -1)
                            {
                                var array = task.Json.URL.ToString().Split("/");
                                if (array[array.Length - 1] == "")
                                    link = array[array.Length - 2];
                                else
                                    link = array[array.Length - 1];
                            }
                            else
                            {
                                link = task.Json.URL;
                            }
                            await ConsoleMessage2($"Curtindo a publicação '{link}'");
                            var seguir = await Curtir(link, InstaController[i].Web);
                            if (seguir.Status == 1)
                            {
                                if (k == 0)
                                {
                                    InstaController[i].Insta.RemoverBlock();
                                }
                                await ConsoleMessage2("Sucesso ao curtir a publicação");
                                var confirm = await Plat.ConfirmTask(InstaController[i].ContaID, task.Json.PedidoID.ToString());
                                if (confirm.Status == 1)
                                {
                                    await SetBorderColor(Variaveis.Verde);
                                    await ConsoleMessage2("Tarefa realizada com sucesso", Variaveis.Verde);
                                    await AddTarefa(1, i);
                                    k++;
                                }
                                else
                                {
                                    await ConsoleMessage2("Erro ao confirmar a tarefa na Dizu", Variaveis.Vermelho);
                                }
                            }
                            else
                            {
                                if (seguir.Status != 0 && seguir.Status != 3 && seguir.Status != 4)
                                {
                                    var check = await IsBlockOrChallenge(InstaController[i].Web);
                                    switch (check.Status)
                                    {
                                        case 2:
                                            await ConsoleMessage2(check.Response, Variaveis.Amarelo);
                                            await SetBorderColor(Variaveis.Amarelo);
                                            Contas[Index].AdicionarBlock();
                                            Bloqueios.AdicionarBlock(Contas[Index].Username, Global.Timer_Block);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 5:
                                            await ConsoleMessage2(check.Response, Variaveis.VermelhoEscuro);
                                            await SetBorderColor(Variaveis.VermelhoEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 6:
                                            await ConsoleMessage2(check.Response, Variaveis.AzulEscuro);
                                            await SetBorderColor(Variaveis.AzulEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 7:
                                            await ConsoleMessage2(check.Response, Variaveis.Vermelho);
                                            await SetBorderColor(Variaveis.Vermelho);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 8:
                                            await ConsoleMessage2(check.Response, Variaveis.Ciano);
                                            await SetBorderColor(Variaveis.Ciano);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 9:
                                            await ConsoleMessage2(check.Response, Variaveis.CinzaEscuro);
                                            await SetBorderColor(Variaveis.CinzaEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 10:
                                            await ConsoleMessage2(check.Response, Variaveis.CinzaEscuro);
                                            await SetBorderColor(Variaveis.CinzaEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                        case 11:
                                            await ConsoleMessage2(check.Response, Variaveis.CianoEscuro);
                                            await SetBorderColor(Variaveis.CianoEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            break;
                                        default:
                                            await ConsoleMessage2(check.Response, Variaveis.CinzaEscuro);
                                            await SetBorderColor(Variaveis.CinzaEscuro);
                                            Contas[Index].AdicionarChallenge();
                                            Challenges.Add(Contas[Index].Username);
                                            await Task.Delay(1548);
                                            sair = true;
                                            break;
                                    }
                                }
                                else
                                {
                                    await ConsoleMessage2(seguir.Response, Variaveis.Azul);
                                    var pular = await Plat.JunpTask(InstaController[i].ContaID, task.Json.PedidoID.ToString());
                                    if (pular.Status == 1)
                                        await ConsoleMessage2("Tarefa pulada");
                                    else
                                        await ConsoleMessage2("Erro ao pular a tarefa");
                                }
                            }
                        }
                    }
                    else
                    {
                        await ConsoleMessage2("Não foi possivel localizar tarefa no momento");
                        if (Global.Trocar && Contas.Count > 1)
                        {
                            sair = true;
                        }
                    }
                    if (sair == false)
                    {
                        if (task.Status == 1 || task.Status == 2)
                        {
                            var delay = r.Next(Convert.ToInt32(Global.Delay1), Convert.ToInt32(Global.Delay2));
                            await ConsoleMessage2($"Aguardando {delay} segundos para continuar", Variaveis.Ciano);
                            await Task.Delay(TimeSpan.FromSeconds(delay));
                        }
                        else
                        {
                            await ConsoleMessage2($"Aguardando {3} segundos para continuar", Variaveis.Ciano);
                            await Task.Delay(TimeSpan.FromSeconds(3));
                        }
                    }
                }
                await ConsoleMessage2($"Indo para a proxima conta");
                return;
            }
            catch (Exception err)
            {
                LogMessage($"Erro RodarConta: {err.Message}");
                await ConsoleMessage2("Erro ao rodar o bot, entre em contato com o suporte", Variaveis.Vermelho);
                return;
            }
        }

        #region Instagram
        /// <summary>
        /// Quando a conta levar feedback_required valida se ela só tomou Feedback ou Challenge
        /// </summary>
        /// <param name="Insta">Conta para verificar</param>
        /// <returns>2 = Block temporario / 5 = Block de SMS / 6 = Block Troca de senha / 7 = Email/SMS / 8 = Bloqueio de foto / 9 = Nao configurado / 10 = Erro ao puxar o challenge / 11 = Challenge Selfie</returns>
        async Task<Models.ActionModels.Result> IsBlockOrChallenge(InstagramWeb Insta)
        {
            Models.ActionModels.Result Ret = new Models.ActionModels.Result
            {
                Response = "",
                Status = 2
            };
            try
            {
                var profile = await Insta.GetMyProfileAsync();
                if (profile.Status == 1)
                {
                    Ret.Status = 2;
                    Ret.Response = "Bloqueio: Temporario";
                }
                else
                {
                    if (Ret.Status == -2)
                    {
                        var challenge = await Insta.GetChallengeRequestByChallengeUrlAsync();
                        if (challenge.Status == 1)
                        {
                            switch (challenge.Response)
                            {
                                case "SelectContactPointRecoveryForm":
                                    Ret.Status = 7;
                                    Ret.Response = "Bloqueio: Email/SMS";
                                    return Ret;
                                case "ReviewLoginForm":
                                    Ret.Status = 7;
                                    Ret.Response = "Bloqueio: Email/SMS";
                                    return Ret;
                                case "SelfieCaptchaChallengeForm":
                                    Ret.Status = 11;
                                    Ret.Response = "Bloqueio: Selfie";
                                    return Ret;
                                case "RecaptchaChallengeForm":
                                    Ret.Status = 5;
                                    Ret.Response = "Bloqueio: Recaptcha e SMS";
                                    return Ret;
                                case "IeForceSetNewPasswordForm":
                                    Ret.Status = 6;
                                    Ret.Response = "Bloqueio: Troca de senha";
                                    return Ret;
                                case "SubmitPhoneNumberForm":
                                    Ret.Status = 5;
                                    Ret.Response = "Bloqueio: SMS";
                                    return Ret;
                                case "EscalationChallengeInformationalForm":
                                    var res = await Insta.ReplyChallengeByChoiceAsync("0");
                                    if (res.Satus == 1)
                                    {
                                        Ret.Status = 0;
                                        Ret.Response = "Bloqueio: Foto | Ja resolvido";
                                    }
                                    else
                                    {
                                        LogMessage(res.Response);
                                        Ret.Status = 8;
                                        Ret.Response = "Bloqueio: Foto";
                                    }
                                    return Ret;
                                case "SelectVerificationMethodForm":
                                    Ret.Status = 7;
                                    Ret.Response = "Bloqueio: Email/SMS";
                                    return Ret;
                                default:
                                    LogMessage($"Tipo de bloqueio não detectado: {challenge.Response}");
                                    Ret.Status = 9;
                                    Ret.Response = "Bloqueio: " + challenge.Response;
                                    return Ret;
                            }
                        }
                        else
                        {
                            LogMessage(challenge.Response);
                            Ret.Status = 10;
                            Ret.Response = "Bloqueio: Erro ao detectar";
                            return Ret;
                        }
                    }
                    else
                    {
                        Ret.Status = -1;
                        Ret.Response = profile.Response;
                    }
                }
            }
            catch (Exception err)
            {
                Ret.Status = -1;
                Ret.Response = err.Message;
            }
            return Ret;
        }

        /// <summary>
        /// Realizar tarefa de seguir um perfil
        /// </summary>
        /// <param name="target">Username do Alvo</param>
        /// <param name="Insta">InstagramWeb logado na conta</param>
        /// <returns>-1 = Erro / 0 = Ja seguiu o perfil / 1 = Sucesso / 2 = Bloqueio temporario / 3 = Perfil não encontrado / 4 = Perfil Privado / 5 = Block de SMS / 6 = Block Troca de senha / 7 = Email/SMS / 8 = Bloqueio de foto / 9 = Nao configurado / 10 = Erro ao puxar o challenge / 11 = Challenge Selfie</returns>
        async Task<Models.ActionModels.Result> Seguir(string target, InstagramWeb Insta, bool barra = false)
        {
            Models.ActionModels.Result Ret = new Models.ActionModels.Result
            {
                Response = "Erro ao realizar a tarefa",
                Status = -1
            };
            try
            {
                InstaResponse id = null;
                if (!barra)
                    id = await Insta.GetUserBySearchBarAsync(target.ToLower());
                else
                    id = await Insta.GetUserIdByUsernameAsync(target.ToLower());
                if (id.Satus == 1)
                {
                    var relation = await Insta.GetFriendshipRelationByUsernameAsync(target);
                    if (relation.Status == 1)
                    {
                        if (relation.Is_Complet)
                        {
                            if (!relation.Is_Private)
                            {
                                if (!relation.Is_Following)
                                {
                                    var seguir = await Insta.FollowUserByIdAsync(id.Response);
                                    if (seguir.Satus == 1)
                                    {
                                        Ret.Status = 1;
                                        Ret.Response = "Sucesso ao seguir o perdil";
                                        return Ret;
                                    }
                                    else
                                    {
                                        if (seguir.Satus != -2)
                                        {
                                            if (seguir.Satus == -3)
                                            {
                                                var check = await IsBlockOrChallenge(Insta);
                                                if (check.Status == 0)
                                                {
                                                    return await Seguir(target, Insta, barra);
                                                }
                                                else
                                                {
                                                    return check;
                                                }
                                            }
                                            else
                                            {
                                                Ret.Status = 3;
                                                Ret.Response = "Perfil não encontrado";
                                                return Ret;
                                            }
                                        }
                                        else
                                        {
                                            var challenge = await Insta.GetChallengeRequestAsync();
                                            if (challenge.Status == 1)
                                            {
                                                switch (challenge.Response)
                                                {
                                                    case "SelectContactPointRecoveryForm":
                                                        Ret.Status = 7;
                                                        Ret.Response = "Bloqueio: Email/SMS";
                                                        return Ret;
                                                    case "ReviewLoginForm":
                                                        Ret.Status = 7;
                                                        Ret.Response = "Bloqueio: Email/SMS";
                                                        return Ret;
                                                    case "SelfieCaptchaChallengeForm":
                                                        Ret.Status = 11;
                                                        Ret.Response = "Bloqueio: Selfie";
                                                        return Ret;
                                                    case "RecaptchaChallengeForm":
                                                        Ret.Status = 5;
                                                        Ret.Response = "Bloqueio: Recaptcha e SMS";
                                                        return Ret;
                                                    case "IeForceSetNewPasswordForm":
                                                        Ret.Status = 6;
                                                        Ret.Response = "Bloqueio: Troca de senha";
                                                        return Ret;
                                                    case "SubmitPhoneNumberForm":
                                                        Ret.Status = 5;
                                                        Ret.Response = "Bloqueio: SMS";
                                                        return Ret;
                                                    case "EscalationChallengeInformationalForm":
                                                        var res = await Insta.ReplyChallengeByChoiceAsync("0");
                                                        if (res.Satus == 1)
                                                            return await Seguir(target, Insta, barra);
                                                        else
                                                        {
                                                            LogMessage(res.Response);
                                                            Ret.Status = 8;
                                                            Ret.Response = "Bloqueio: Foto";
                                                        }
                                                        return Ret;
                                                    case "SelectVerificationMethodForm":
                                                        Ret.Status = 7;
                                                        Ret.Response = "Bloqueio: Email/SMS";
                                                        return Ret;
                                                    default:
                                                        LogMessage($"Tipo de bloqueio não detectado: {challenge.Response}");
                                                        Ret.Status = 9;
                                                        Ret.Response = "Bloqueio: " + challenge.Response;
                                                        return Ret;
                                                }
                                            }
                                            else
                                            {
                                                LogMessage(challenge.Response);
                                                Ret.Status = 10;
                                                Ret.Response = "Bloqueio: Erro ao detectar";
                                                return Ret;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Ret.Status = 0;
                                    Ret.Response = "Ja segue o perfil";
                                    return Ret;
                                }
                            }
                            else
                            {
                                Ret.Status = 4;
                                Ret.Response = "Perfil privado";
                                return Ret;
                            }
                        }
                        else
                        {
                            Ret.Status = 3;
                            Ret.Response = "Perfil não encontrado";
                            return Ret;
                        }
                    }
                    else
                    {
                        if (relation.Status != -2)
                        {
                            if (relation.Status == -3)
                            {
                                var check = await IsBlockOrChallenge(Insta);
                                if (check.Status == 0)
                                {
                                    return await Seguir(target, Insta, barra);
                                }
                                else
                                {
                                    return check;
                                }
                            }
                            else
                            {
                                Ret.Status = 3;
                                Ret.Response = "Perfil não encontrado";
                                return Ret;
                            }
                        }
                        else
                        {
                            var challenge = await Insta.GetChallengeRequestAsync();
                            if (challenge.Status == 1)
                            {
                                switch (challenge.Response)
                                {
                                    case "SelectContactPointRecoveryForm":
                                        Ret.Status = 7;
                                        Ret.Response = "Bloqueio: Email/SMS";
                                        return Ret;
                                    case "ReviewLoginForm":
                                        Ret.Status = 7;
                                        Ret.Response = "Bloqueio: Email/SMS";
                                        return Ret;
                                    case "SelfieCaptchaChallengeForm":
                                        Ret.Status = 11;
                                        Ret.Response = "Bloqueio: Selfie";
                                        return Ret;
                                    case "RecaptchaChallengeForm":
                                        Ret.Status = 5;
                                        Ret.Response = "Bloqueio: Recaptcha e SMS";
                                        return Ret;
                                    case "IeForceSetNewPasswordForm":
                                        Ret.Status = 6;
                                        Ret.Response = "Bloqueio: Troca de senha";
                                        return Ret;
                                    case "SubmitPhoneNumberForm":
                                        Ret.Status = 5;
                                        Ret.Response = "Bloqueio: SMS";
                                        return Ret;
                                    case "EscalationChallengeInformationalForm":
                                        var res = await Insta.ReplyChallengeByChoiceAsync("0");
                                        if (res.Satus == 1)
                                            return await Seguir(target, Insta, barra);
                                        else
                                        {
                                            LogMessage(res.Response);
                                            Ret.Status = 8;
                                            Ret.Response = "Bloqueio: Foto";
                                        }
                                        return Ret;
                                    case "SelectVerificationMethodForm":
                                        Ret.Status = 7;
                                        Ret.Response = "Bloqueio: Email/SMS";
                                        return Ret;
                                    default:
                                        LogMessage($"Tipo de bloqueio não detectado: {challenge.Response}");
                                        Ret.Status = 9;
                                        Ret.Response = "Bloqueio: " + challenge.Response;
                                        return Ret;
                                }
                            }
                            else
                            {
                                LogMessage(challenge.Response);
                                Ret.Status = 10;
                                Ret.Response = "Bloqueio: Erro ao detectar";
                                return Ret;
                            }
                        }
                    }
                }
                else
                {
                    if (id.Satus != -2)
                    {
                        if (id.Satus == -3)
                        {
                            var check = await IsBlockOrChallenge(Insta);
                            if (check.Status == 0)
                            {
                                return await Seguir(target, Insta, barra);
                            }
                            else
                            {
                                return check;
                            }
                        }
                        else
                        {
                            Ret.Status = 3;
                            Ret.Response = "Perfil não encontrado";
                            return Ret;
                        }
                    }
                    else
                    {
                        var challenge = await Insta.GetChallengeRequestAsync();
                        if (challenge.Status == 1)
                        {
                            switch (challenge.Response)
                            {
                                case "SelectContactPointRecoveryForm":
                                    Ret.Status = 7;
                                    Ret.Response = "Bloqueio: Email/SMS";
                                    return Ret;
                                case "RecaptchaChallengeForm":
                                    Ret.Status = 5;
                                    Ret.Response = "Bloqueio: Recaptcha e SMS";
                                    return Ret;
                                case "IeForceSetNewPasswordForm":
                                    Ret.Status = 6;
                                    Ret.Response = "Bloqueio: Troca de senha";
                                    return Ret;
                                case "SubmitPhoneNumberForm":
                                    Ret.Status = 5;
                                    Ret.Response = "Bloqueio: SMS";
                                    return Ret;
                                case "EscalationChallengeInformationalForm":
                                    var res = await Insta.ReplyChallengeByChoiceAsync("0");
                                    if (res.Satus == 1)
                                        return await Seguir(target, Insta, barra);
                                    else
                                    {
                                        LogMessage(res.Response);
                                        Ret.Status = 8;
                                        Ret.Response = "Bloqueio: Foto";
                                    }
                                    return Ret;
                                case "SelectVerificationMethodForm":
                                    Ret.Status = 7;
                                    Ret.Response = "Bloqueio: Email/SMS";
                                    return Ret;
                                default:
                                    LogMessage($"Tipo de bloqueio não detectado: {challenge.Response}");
                                    Ret.Status = 9;
                                    Ret.Response = "Bloqueio: " + challenge.Response;
                                    return Ret;
                            }
                        }
                        else
                        {
                            LogMessage(challenge.Response);
                            Ret.Status = 10;
                            Ret.Response = "Bloqueio: Erro ao detectar";
                            return Ret;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Ret.Response = err.Message;
                Ret.Status = -1;
            }
            return Ret;
        }

        /// <summary>
        /// Curtir uma publicação pelo Shortcode
        /// </summary>
        /// <param name="target">Shortcode da publicação</param>
        /// <param name="Insta">Instagram</param>
        /// <returns>-1 = Erro / 0 = Ja curtiu a publicação / 1 = Sucesso / 2 = Bloqueio temporario / 3 = Publicação não encontrada / 4 = Publicação Privada / 5 = Block de SMS / 6 = Block Troca de senha / 7 = Email/SMS / 8 = Bloqueio de foto / 9 = Nao configurado / 10 = Erro ao puxar o challenge / 11 = Challenge Selfie</returns>
        async Task<Models.ActionModels.Result> Curtir(string target, InstagramWeb Insta)
        {
            Models.ActionModels.Result Ret = new Models.ActionModels.Result
            {
                Response = "Erro ao curtir a publicação",
                Status = -1
            };
            try
            {
                var relation = await Insta.GetMediaRelationByShortcodeAsync(target);
                if (relation.Status == 1)
                {
                    if (relation.Is_Complet)
                    {
                        if (!relation.Is_Liked)
                        {
                            var curtir = await Insta.LikeMediaByIdAsync(relation.MediaID);
                            if (curtir.Satus == 1)
                            {
                                Ret.Status = 1;
                                Ret.Response = "Publicação curtida com sucesso";
                                return Ret;
                            }
                            else
                            {
                                if (curtir.Satus != -2)
                                {
                                    if (curtir.Satus == -3)
                                    {
                                        var check = await IsBlockOrChallenge(Insta);
                                        if (check.Status == 0)
                                        {
                                            return await Curtir(target, Insta);
                                        }
                                        else
                                        {
                                            return check;
                                        }
                                    }
                                    else
                                    {
                                        Ret.Status = 3;
                                        Ret.Response = "Publicação não encontrada";
                                        return Ret;
                                    }
                                }
                                else
                                {
                                    var challenge = await Insta.GetChallengeRequestAsync();
                                    if (challenge.Status == 1)
                                    {
                                        switch (challenge.Response)
                                        {
                                            case "ReviewLoginForm":
                                                Ret.Status = 7;
                                                Ret.Response = "Bloqueio: Email/SMS";
                                                return Ret;
                                            case "SelfieCaptchaChallengeForm":
                                                Ret.Status = 11;
                                                Ret.Response = "Bloqueio: Selfie";
                                                return Ret;
                                            case "SelectContactPointRecoveryForm":
                                                Ret.Status = 7;
                                                Ret.Response = "Bloqueio: Email/SMS";
                                                return Ret;
                                            case "RecaptchaChallengeForm":
                                                Ret.Status = 5;
                                                Ret.Response = "Bloqueio: Recaptcha e SMS";
                                                return Ret;
                                            case "IeForceSetNewPasswordForm":
                                                Ret.Status = 6;
                                                Ret.Response = "Bloqueio: Troca de senha";
                                                return Ret;
                                            case "SubmitPhoneNumberForm":
                                                Ret.Status = 5;
                                                Ret.Response = "Bloqueio: SMS";
                                                return Ret;
                                            case "EscalationChallengeInformationalForm":
                                                var res = await Insta.ReplyChallengeByChoiceAsync("0");
                                                if (res.Satus == 1)
                                                    return await Curtir(target, Insta);
                                                else
                                                {
                                                    LogMessage(res.Response);
                                                    Ret.Status = 8;
                                                    Ret.Response = "Bloqueio: Foto";
                                                }
                                                return Ret;
                                            case "SelectVerificationMethodForm":
                                                Ret.Status = 7;
                                                Ret.Response = "Bloqueio: Email/SMS";
                                                return Ret;
                                            default:
                                                LogMessage($"Tipo de bloqueio não detectado: {challenge.Response}");
                                                Ret.Status = 9;
                                                Ret.Response = "Bloqueio: " + challenge.Response;
                                                return Ret;
                                        }
                                    }
                                    else
                                    {
                                        LogMessage(challenge.Response);
                                        Ret.Status = 10;
                                        Ret.Response = "Bloqueio: Erro ao detectar";
                                        return Ret;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Ret.Status = 0;
                            Ret.Response = "Ja curtiu a publicação";
                            return Ret;
                        }
                    }
                    else
                    {
                        Ret.Status = 3;
                        Ret.Response = "Publicação não encontrada";
                        return Ret;
                    }
                }
                else
                {
                    if (relation.Status != -2)
                    {
                        if (relation.Status == -3)
                        {
                            var check = await IsBlockOrChallenge(Insta);
                            if (check.Status == 0)
                            {
                                return await Curtir(target, Insta);
                            }
                            else
                            {
                                return check;
                            }
                        }
                        else
                        {
                            Ret.Status = 3;
                            Ret.Response = "Publicação não encontrada";
                            return Ret;
                        }
                    }
                    else
                    {
                        var challenge = await Insta.GetChallengeRequestAsync();
                        if (challenge.Status == 1)
                        {
                            switch (challenge.Response)
                            {
                                case "ReviewLoginForm":
                                    Ret.Status = 7;
                                    Ret.Response = "Bloqueio: Email/SMS";
                                    return Ret;
                                case "SelfieCaptchaChallengeForm":
                                    Ret.Status = 11;
                                    Ret.Response = "Bloqueio: Selfie";
                                    return Ret;
                                case "SelectContactPointRecoveryForm":
                                    Ret.Status = 7;
                                    Ret.Response = "Bloqueio: Email/SMS";
                                    return Ret;
                                case "RecaptchaChallengeForm":
                                    Ret.Status = 5;
                                    Ret.Response = "Bloqueio: Recaptcha e SMS";
                                    return Ret;
                                case "IeForceSetNewPasswordForm":
                                    Ret.Status = 6;
                                    Ret.Response = "Bloqueio: Troca de senha";
                                    return Ret;
                                case "SubmitPhoneNumberForm":
                                    Ret.Status = 5;
                                    Ret.Response = "Bloqueio: SMS";
                                    return Ret;
                                case "EscalationChallengeInformationalForm":
                                    var res = await Insta.ReplyChallengeByChoiceAsync("0");
                                    if (res.Satus == 1)
                                        return await Curtir(target, Insta);
                                    else
                                    {
                                        LogMessage(res.Response);
                                        Ret.Status = 8;
                                        Ret.Response = "Bloqueio: Foto";
                                    }
                                    return Ret;
                                case "SelectVerificationMethodForm":
                                    Ret.Status = 7;
                                    Ret.Response = "Bloqueio: Email/SMS";
                                    return Ret;
                                default:
                                    LogMessage($"Tipo de bloqueio não detectado: {challenge.Response}");
                                    Ret.Status = 9;
                                    Ret.Response = "Bloqueio: " + challenge.Response;
                                    return Ret;
                            }
                        }
                        else
                        {
                            LogMessage(challenge.Response);
                            Ret.Status = 10;
                            Ret.Response = "Bloqueio: Erro ao detectar";
                            return Ret;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Ret.Status = -1;
                Ret.Response = err.Message;
            }
            return Ret;
        }

        /// <summary>
        /// Realizar login na conta do instagram
        /// </summary>
        /// <param name="Insta">InstagramWeb</param>
        /// <returns>-1 = Erro / 1 = Sucesso / 2 = Bloqueio temporario / 5 = Block de SMS / 6 = Block Troca de senha / 7 = Email/SMS / 8 = Bloqueio de foto / 9 = Nao configurado / 10 = Erro ao puxar o challenge / 11 = Challenge Selfie / 12 = Erro ao logar: Usuario ou senha inválido</returns>
        async Task<Models.ActionModels.Result> Login(InstagramWeb Insta)
        {
            Models.ActionModels.Result Ret = new Models.ActionModels.Result
            {
                Response = "Erro ao realizar o login",
                Status = -1
            };
            try
            {
                var login = Insta.DoLogin();
                if (login.Satus == 1)
                {
                    await Task.Delay(1487);
                    var profile = await Insta.GetMyProfileAsync();
                    if (profile.Status == 1)
                    {
                        var logins = await Insta.GetSuspiciousLoginAsync();
                        if (logins.Json.Count > 0)
                        {
                            try
                            {
                                int max = logins.Json.Count >= 3 ? 3 : logins.Json.Count;
                                for (int j = 0; j < max; j++)
                                {
                                    string id = logins.Json[j].id;
                                    var confirm = await Insta.AllowSuspiciosLoginByIdAsync(id);
                                }
                            }
                            catch (Exception err)
                            {
                                LogMessage($"Erro ao verificar acesso: {err.Message}");
                            }
                        }
                        Ret.Status = 1;
                        Ret.Response = "Login realizado com sucesso";
                        return Ret;
                    }
                    else
                    {
                        if (profile.Status != -2)
                        {
                            if (profile.Status == -3)
                            {
                                var check = await IsBlockOrChallenge(Insta);
                                if (check.Status == 0)
                                {
                                    if (await IsLogged(Insta))
                                    {
                                        Ret.Status = 1;
                                        Ret.Response = "Login realizado com sucesso";
                                        return Ret;
                                    }
                                    else
                                    {
                                        return await Login(Insta);
                                    }
                                }
                                else
                                {
                                    return check;
                                }
                            }
                            else
                            {
                                Ret.Status = 12;
                                Ret.Response = "Login: Usuario ou Senha inválida";
                                return Ret;
                            }
                        }
                        else
                        {
                            var challenge = await Insta.GetChallengeRequestAsync();
                            if (challenge.Status == 1)
                            {
                                switch (challenge.Response)
                                {
                                    case "ReviewLoginForm":
                                        Ret.Status = 7;
                                        Ret.Response = "Bloqueio: Email/SMS";
                                        return Ret;
                                    case "SelfieCaptchaChallengeForm":
                                        Ret.Status = 11;
                                        Ret.Response = "Bloqueio: Selfie";
                                        return Ret;
                                    case "SelectContactPointRecoveryForm":
                                        Ret.Status = 7;
                                        Ret.Response = "Bloqueio: Email/SMS";
                                        return Ret;
                                    case "RecaptchaChallengeForm":
                                        Ret.Status = 5;
                                        Ret.Response = "Bloqueio: Recaptcha e SMS";
                                        return Ret;
                                    case "IeForceSetNewPasswordForm":
                                        Ret.Status = 6;
                                        Ret.Response = "Bloqueio: Troca de senha";
                                        return Ret;
                                    case "SubmitPhoneNumberForm":
                                        Ret.Status = 5;
                                        Ret.Response = "Bloqueio: SMS";
                                        return Ret;
                                    case "EscalationChallengeInformationalForm":
                                        var res = await Insta.ReplyChallengeByChoiceAsync("0");
                                        if (res.Satus == 1)
                                        {
                                            if (await IsLogged(Insta))
                                            {
                                                Ret.Status = 1;
                                                Ret.Response = "Login realizado com sucesso";
                                                return Ret;
                                            }
                                            else
                                            {
                                                return await Login(Insta);
                                            }
                                        }
                                        else
                                        {
                                            LogMessage(res.Response);
                                            Ret.Status = 8;
                                            Ret.Response = "Bloqueio: Foto";
                                        }
                                        return Ret;
                                    case "SelectVerificationMethodForm":
                                        Ret.Status = 7;
                                        Ret.Response = "Bloqueio: Email/SMS";
                                        return Ret;
                                    default:
                                        LogMessage($"Tipo de bloqueio não detectado: {challenge.Response}");
                                        Ret.Status = 9;
                                        Ret.Response = "Bloqueio: " + challenge.Response;
                                        return Ret;
                                }
                            }
                            else
                            {
                                LogMessage(challenge.Response);
                                Ret.Status = 10;
                                Ret.Response = "Bloqueio: Erro ao detectar";
                                return Ret;
                            }
                        }
                    }
                }
                else
                {
                    if (login.Satus != -2)
                    {
                        Ret.Status = 12;
                        Ret.Response = "Login: Usuario ou Senha inválida";
                        return Ret;
                    }
                    else
                    {
                        var challenge = await Insta.GetChallengeRequestByChallengeUrlAsync();
                        if (challenge.Status == 1)
                        {
                            switch (challenge.Response)
                            {
                                case "ReviewLoginForm":
                                    Ret.Status = 7;
                                    Ret.Response = "Bloqueio: Email/SMS";
                                    return Ret;
                                case "SelfieCaptchaChallengeForm":
                                    Ret.Status = 11;
                                    Ret.Response = "Bloqueio: Selfie";
                                    return Ret;
                                case "SelectContactPointRecoveryForm":
                                    Ret.Status = 7;
                                    Ret.Response = "Bloqueio: Email/SMS";
                                    return Ret;
                                case "RecaptchaChallengeForm":
                                    Ret.Status = 5;
                                    Ret.Response = "Bloqueio: Recaptcha e SMS";
                                    return Ret;
                                case "IeForceSetNewPasswordForm":
                                    Ret.Status = 6;
                                    Ret.Response = "Bloqueio: Troca de senha";
                                    return Ret;
                                case "SubmitPhoneNumberForm":
                                    Ret.Status = 5;
                                    Ret.Response = "Bloqueio: SMS";
                                    return Ret;
                                case "EscalationChallengeInformationalForm":
                                    var res = await Insta.ReplyChallengeByChoiceAsync("0");
                                    if (res.Satus == 1)
                                        return await Login(Insta);
                                    else
                                    {
                                        LogMessage(res.Response);
                                        Ret.Status = 8;
                                        Ret.Response = "Bloqueio: Foto";
                                    }
                                    return Ret;
                                case "SelectVerificationMethodForm":
                                    Ret.Status = 7;
                                    Ret.Response = "Bloqueio: Email/SMS";
                                    return Ret;
                                default:
                                    LogMessage($"Tipo de bloqueio não detectado: {challenge.Response}");
                                    Ret.Status = 9;
                                    Ret.Response = "Bloqueio: " + challenge.Response;
                                    return Ret;
                            }
                        }
                        else
                        {
                            LogMessage(challenge.Response);
                            Ret.Status = 10;
                            Ret.Response = "Bloqueio: Erro ao detectar";
                            return Ret;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LogMessage($"Erro login insta: {err.Message}");
            }
            return Ret;
        }

        /// <summary>
        /// Checa se uma conta está conectada
        /// </summary>
        /// <param name="Insta">Instagram</param>
        /// <returns>True caso sim e False caso não</returns>
        async Task<bool> IsLogged(InstagramWeb Insta)
        {
            try
            {
                var profile = await Insta.GetMyProfileAsync();
                if (profile.Status == 1)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion
        #region Funções
        private string DateString()
        {
            var data = DateTime.Today;
            var dia = data.Day.ToString();
            var mes = data.Month.ToString();
            var ano = data.Year.ToString();
            return $"{dia}-{mes}-{ano}";
        }

        private string HorarioString()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        private void LogMessage(string message)
        {
            try
            {
                var dir = Directory.GetCurrentDirectory();
                if (Directory.Exists($@"{dir}\logs"))
                {
                    var data = DateString();
                    if (File.Exists($@"{dir}\logs\{data}.txt"))
                    {
                        string[] linhas = File.ReadAllLines($@"{dir}\logs\{data}.txt");
                        var list = linhas.ToList();
                        list.Add($"GNIAPI {HorarioString()} {message}");
                        File.WriteAllLines($@"{dir}\logs\{data}.txt", list);
                        return;
                    }
                    else
                    {
                        string[] linhas = { $"GNIAPI {HorarioString()} {message}" };
                        File.WriteAllLines($@"{dir}\logs\{data}.txt", linhas);
                        return;
                    }
                }
                else
                {
                    Directory.CreateDirectory($@"{dir}\logs");
                    var data = DateString();
                    if (File.Exists($@"{dir}\logs\{data}.txt"))
                    {
                        string[] linhas = File.ReadAllLines($@"{dir}\logs\{data}.txt");
                        var list = linhas.ToList();
                        list.Add($"GNIAPI {HorarioString()} {message}");
                        File.WriteAllLines($@"{dir}\logs\{data}.txt", list);
                        return;
                    }
                    else
                    {
                        string[] linhas = { $"GNIAPI {HorarioString()} {message}" };
                        File.WriteAllLines($@"{dir}\logs\{data}.txt", linhas);
                        return;
                    }
                }
            }
            catch { }
        }

        private async Task AddTarefa(int type, int i)
        {
            Variaveis.Total++;
            Variaveis.TotalMeta++;
            if (type == 0)
            {
                await AddSaldo(0.006);
                Contas[Index].Seguir++;
                Contas[Index].AdicionarSeguir();
                InstaController[i].Seguir++;
                InstaController[i].Total++;
                Variaveis.TotalContaAtual = InstaController[i].Total;
                Variaveis.TotalSeguir = InstaController[i].Seguir;
                tx_tconta.Text = Variaveis.TotalContaAtual.ToString();
                tx_seguir.Text = Variaveis.TotalSeguir.ToString();
            }
            else
            {
                await AddSaldo(0.002);
                Contas[Index].Curtir++;
                Contas[Index].AdicionarCurtir();
                InstaController[i].Curtir++;
                InstaController[i].Total++;
                Variaveis.TotalContaAtual = InstaController[i].Total;
                Variaveis.TotalCurtir = InstaController[i].Curtir;
                tx_tconta.Text = Variaveis.TotalContaAtual.ToString();
                tx_curtir.Text = Variaveis.TotalCurtir.ToString();
            }
            tx_total.Text = Variaveis.Total.ToString();
            tx_meta.Text = $"{Variaveis.TotalMeta}/{Global.Meta}";
            if (Variaveis.TotalMeta >= Variaveis.Meta)
            {
                Variaveis.MetaAlcancada++;
                await ConsoleMessage2("Meta de tarefas alcançada.", Variaveis.VerdeEscuro);
                await ConsoleMessage2($"Aguardando {Global.Timer_Meta} minutos para continuar", Variaveis.Ciano);
                Console.Title = $"Grupo: {Variaveis.Grupo} - Meta alcançada, aguardando para continuar";
                await Task.Delay(TimeSpan.FromMinutes(Global.Timer_Meta));
                Variaveis.TotalMeta = 0;
            }
        }
        #endregion

    }
}
