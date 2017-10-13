using CPB.GeradorDeResultados.DTO;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows.Forms;

namespace CPB.GeradorDeResultados
{
    public class GeradorResultadoService
    {
        string _pasta = string.Empty;
        string _pastaResultadosExibidos = string.Empty;
        string _pastaPaginas = string.Empty;
        private const string INDEX_HTML = "index.html";
        Process _internetExplorer = null;
        DateTime _horaInicio;
        string[] _linhasArquivo;
        private readonly System.Timers.Timer _timerResultados;
        private readonly System.Timers.Timer _timerPrincipal;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public GeradorResultadoService()
        {
            _internetExplorer = new Process();
            _pasta = ConfigurationManager.AppSettings["PathFiles"].ToString();
            _pastaResultadosExibidos = ConfigurationManager.AppSettings["PathResultadosExibidos"].ToString();
            _pastaPaginas = ConfigurationManager.AppSettings["PathPaginas"].ToString();

            if (!Directory.Exists(_pastaPaginas))
                Directory.CreateDirectory(_pastaPaginas);

            foreach (var item in Directory.GetFiles(_pasta))
                File.Delete(item);

            _timerPrincipal = new System.Timers.Timer();
            _timerPrincipal.Enabled = true;
            _timerPrincipal.Interval = 100;
            _timerPrincipal.Elapsed += new ElapsedEventHandler(timerEver_Tick);

            _timerResultados = new System.Timers.Timer();
            _timerResultados.Interval = 1000;
            _timerResultados.Elapsed += new ElapsedEventHandler(timer_Tick);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (_horaInicio != default(DateTime) && (DateTime.Now - _horaInicio).Minutes > 0)
            {
                _timerResultados.Stop();
                CriarPaginaStandBy();
                AtualizarPagina();
            }

        }

        private void timerEver_Tick(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles(_pasta);
            if (files.Length > 0)
            {
                _timerPrincipal.Stop();
                Processar(files[0]);
            }

            _timerPrincipal.Start();
        }

        public void Iniciar()
        {
            CriarPaginaStandBy();
            AbrirPaginaInicial();
        }

        private void AtualizarPagina()
        {
            SetForegroundWindow(_internetExplorer.MainWindowHandle);
            SendKeys.SendWait("{F5}");
        }

        private void AbrirPaginaInicial()
        {
            foreach (var pr in Process.GetProcessesByName("iexplore"))
                pr.Kill();

            ProcessStartInfo startInfo = new ProcessStartInfo("IExplore.exe");
            startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            startInfo.Arguments = "-k " + ObterLocalPagina();
            _internetExplorer.StartInfo = startInfo;
            _internetExplorer.Start();
        }

        private void Processar(string fullPath)
        {
            FileInfo fileInfo = new FileInfo(fullPath);
            _linhasArquivo = File.ReadAllLines(fileInfo.FullName, Encoding.UTF8);

            string arquivoParaCopiar = string.Concat(_pastaResultadosExibidos, fileInfo.Name);
            if (File.Exists(arquivoParaCopiar))
                File.Delete(arquivoParaCopiar);

            fileInfo.MoveTo(arquivoParaCopiar);


            _horaInicio = DateTime.Now;
            _timerResultados.Start();
            AtualizarPaginaDeResultados();

        }


        private ResultadoDaProva GerarResultado()
        {
            var resultadoDaProva = new ResultadoDaProva();

            for (int i = 0; i < _linhasArquivo.Length; i++)
            {
                if (i == 0)
                    resultadoDaProva.ResolverDadosDaProva(_linhasArquivo[i].Split(','));
                else
                    resultadoDaProva.ResolverParticipante(_linhasArquivo[i].Split(','));
            }

            return resultadoDaProva;
        }


        private void CriarPaginaStandBy()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("</head>");
            sb.Append("<body style=\"background: black;\">");
            sb.Append("</body>");
            sb.Append("</html>");

            CriarPaginaHtml(sb.ToString());
        }

        private void AtualizarPaginaDeResultados()
        {
            var resultados = GerarResultado();

            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("</head>");
            sb.Append($"<body style=\"\">");

            sb.Append("</body>");
            sb.Append("</html>");

            CriarPaginaHtml(sb.ToString());

            AtualizarPagina();
        }

        private string ObterLocalPagina() => string.Concat(_pastaPaginas, INDEX_HTML);

        private void CriarPaginaHtml(string conteudo)
        {
            using (FileStream fs = File.Create(ObterLocalPagina()))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(conteudo);
                fs.Write(info, 0, info.Length);
            }
        }
    }
}
