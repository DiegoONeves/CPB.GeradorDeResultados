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

            if (!Directory.Exists(_pasta))
                Directory.CreateDirectory(_pasta);

            if (!Directory.Exists(_pastaPaginas))
                Directory.CreateDirectory(_pastaPaginas);

            if (!Directory.Exists(_pastaResultadosExibidos))
                Directory.CreateDirectory(_pastaResultadosExibidos);

            foreach (var item in Directory.GetFiles(_pasta))
                File.Delete(item);

            _timerPrincipal = new System.Timers.Timer();
            _timerPrincipal.Enabled = true;
            _timerPrincipal.Interval = 300;
            _timerPrincipal.Elapsed += new ElapsedEventHandler(timerPrincipal_Tick);

            _timerResultados = new System.Timers.Timer();
            _timerResultados.Interval = 1000;
            //_timerResultados.Elapsed += new ElapsedEventHandler(timer_Tick);
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

        private void timerPrincipal_Tick(object sender, EventArgs e)
        {
            try
            {
                string[] files = Directory.GetFiles(_pasta);
                if (files.Length > 0)
                {
                    _timerPrincipal.Stop();
                    Processar(files[0]);
                }
            }
            catch { }
            finally
            {
                _timerPrincipal.Start();
            }

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
            sb.Append("<!DOCTYPE html><html><meta charset=\"utf-8\" />");
            sb.Append("<head>");
            sb.Append("</head>");
            sb.Append("<body style=\"background: black;\">");
            sb.Append("</body>");
            sb.Append("</html>");

            CriarPaginaHtml(sb.ToString());
        }

        private void AtualizarPaginaDeResultados()
        {
            var resultado = GerarResultado();

            StringBuilder sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><meta charset=\"utf-8\" />");
            sb.Append("<head>");
            sb.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"styles.css\">");
            sb.Append("</head>");
            sb.Append($"<body>");
            sb.Append($@"<div style=""position:relative;"">
          <div class=""top""><div class=""logo""><img src=""Imagens/logo_cpb.png"" /></div>
        </div><div class=""conteudo""><p style=""margin-bottom:80px"">{resultado.Prova.NomeDaProva} {resultado.Prova.VelocidadeDoVento}
        </p>
            <table class=""tabela"" >
                <tr>
                    <th width=""5%"">Col</th>
                    <th width=""5%"">Num</th>
                    <th width=""5%"">Raia</th>
                    <th width=""20%"">Nome</th>
                    <th width=""20%"">Equipe</th>
                    <th width=""10%"">Res</th>
                </tr>");
            foreach (var item in resultado.Participantes)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{item.Colocacao}</td><td>{item.Identificacao}</td><td>{item.Raia}</td><td>{item.Nome} {item.Sobrenome}</td><td>{item.Clube}</td><td>{item.Tempo}</td>");
                sb.Append("</tr>");
            }
            sb.Append(@"</table></div><div class=""rodape""><img src=""Imagens/logo_loterias.png"" style=""margin-top:120px;margin-left:50px"" />
                        <img src=""Imagens/logo_braskem.png"" style=""margin-left:30px;margin-bottom:30px"" /></div></div>");
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
