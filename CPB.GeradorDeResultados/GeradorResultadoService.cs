using CPB.GeradorDeResultados.DTO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using System.Linq;
using System.Threading;

namespace CPB.GeradorDeResultados
{
    public class GeradorResultadoService
    {
        FileSystemWatcher _watcher = null;
        string _pasta = string.Empty;
        string _pastaResultadosExibidos = string.Empty;
        string _pastaPaginas = string.Empty;
        private const string INDEX_HTML = "index.html";
        Process _internetExplorer = null;
        DateTime _horaInicio;
        string[] _linhasArquivo;
        private readonly System.Timers.Timer _timer;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public GeradorResultadoService()
        {
            _internetExplorer = new Process();
            _pasta = ConfigurationManager.AppSettings["PathFiles"].ToString();
            _pastaResultadosExibidos = ConfigurationManager.AppSettings["PathResultadosExibidos"].ToString();
            _pastaPaginas = ConfigurationManager.AppSettings["PathPaginas"].ToString();

            if (!Directory.Exists(_pastaResultadosExibidos))
                Directory.CreateDirectory(_pastaResultadosExibidos);

            if (!Directory.Exists(_pastaPaginas))
                Directory.CreateDirectory(_pastaPaginas);

            foreach (var item in Directory.GetFiles(_pasta))
                File.Delete(item);

            _timer = new System.Timers.Timer();

            _timer.Enabled = true;
            _timer.Interval = 3000;
            _timer.Elapsed += new ElapsedEventHandler(timer_Tick);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (_horaInicio != default(DateTime) && (DateTime.Now - _horaInicio).Minutes > 0)
            {
                _timer.Stop();
                CriarPaginaStandBy();

                AtualizarPagina();
            }

        }

        public void Iniciar()
        {
            _watcher = new FileSystemWatcher(_pasta);
            _watcher.Created += new FileSystemEventHandler(_watcher_Created);
            _watcher.EnableRaisingEvents = true;
            _watcher.IncludeSubdirectories = true;

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

        private void Processar()
        {
            _horaInicio = DateTime.Now;
            AtualizarPaginaDeResultados();
        }


        private ResultadoDaProva GerarResultado()
        {
            var resultadoDaProva = new ResultadoDaProva();

            for (int i = 0; i < _linhasArquivo .Length; i++)
            {
                if (i == 0)
                    resultadoDaProva.ResolverDadosDaProva(_linhasArquivo[i].Split(','));
                else
                    resultadoDaProva.ResolverParticipante(_linhasArquivo[i].Split(','));
            }

            return resultadoDaProva;
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
           
            List<String> lst = new List<string>();
            using (FileStream fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                        lst.Add(sr.ReadLine());
                }
                fs.Flush();
            }                
          
            //_linhasArquivo = File.ReadAllLines(e.FullPath, Encoding.UTF8);
            string arquivoParaCopiar = string.Concat(_pastaResultadosExibidos, e.Name);
            File.Move(e.FullPath, arquivoParaCopiar);
            Processar();
            _timer.Start();



            //File.Delete(e.FullPath);
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
