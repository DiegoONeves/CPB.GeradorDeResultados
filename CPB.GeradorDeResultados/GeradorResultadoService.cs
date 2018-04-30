using CPB.GeradorDeResultados.DTO;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using Excel;

namespace CPB.GeradorDeResultados
{
    public class GeradorResultadoService
    {
        #region Propriedades
        string _pasta = string.Empty;
        string _pastaResultadosExibidos = string.Empty;
        string _pastaPaginas = string.Empty;
        private const string INDEX_HTML = "index.html";
        Process _internetExplorer = null;
        DateTime _horaInicio;
        string[] _linhasArquivo;
        private readonly System.Timers.Timer _timerResultados;
        private readonly System.Timers.Timer _timerPrincipal;
        #endregion

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

        private void GerarIMG_Classificacao()
        {
            var resultado = GerarResultado();
            string path = ConfigurationManager.AppSettings["PathImagensPng"].ToString() + "CLASSIFICACAO.png"; 
            Font font = new Font(FontFamily.GenericSansSerif, 30);
            Font fontBold = new Font(FontFamily.GenericSansSerif, 30,FontStyle.Bold);
            Font fontTitulo = new Font(FontFamily.GenericSerif, 40);
            int maxWidth = 1920;
            
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);
            
            StringFormat sf = new StringFormat();
            
            sf.Trimming = StringTrimming.Word;
            
            img.Dispose();
            drawing.Dispose();
            
            img = new Bitmap(1920, 1080);
            drawing = Graphics.FromImage(img);
            
            drawing.CompositingQuality = CompositingQuality.HighQuality;
            drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
            drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
            drawing.SmoothingMode = SmoothingMode.HighQuality;
            drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            
            drawing.Clear(Color.Transparent);
            
            Brush textBrush = new SolidBrush(Color.Black);
            Brush textBrushBranco = new SolidBrush(Color.White);
            Brush textBrushAzul = new SolidBrush(Color.DarkSlateBlue);
            SizeF textSizeProva = drawing.MeasureString(resultado.Prova.NomeDaProva, fontTitulo, maxWidth);
            drawing.DrawString(resultado.Prova.NomeDaProva, fontTitulo, textBrushBranco, new RectangleF(500, 230, textSizeProva.Width, textSizeProva.Height), sf);
            
            int y = 310;
            foreach (var item in resultado.Participantes)
            {
                SizeF textSizeColocacao = drawing.MeasureString(item.Colocacao.ToString(), font, maxWidth);
                SizeF textSizeNome = drawing.MeasureString(item.Nome, font, maxWidth);
                SizeF textSizeSobrenome = drawing.MeasureString(item.Sobrenome, font, maxWidth);
                //SizeF textSizeClube = drawing.MeasureString(item.Clube, font, maxWidth);
                SizeF textSizeTempo = drawing.MeasureString(item.Tempo, font, maxWidth);
                drawing.DrawString(item.Colocacao.ToString(), fontBold, textBrushAzul, new RectangleF(345, y, textSizeColocacao.Width, textSizeColocacao.Height), sf);
                
                Image bandeira = ObterBandeiraPais(item.Clube.ToUpper()); //Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"].ToString() + "br.png");
                drawing.DrawImage(bandeira, 410, y + 2, 58, 41);

                drawing.DrawString(item.Nome, font, textBrushBranco, new RectangleF(455, y, textSizeNome.Width, textSizeNome.Height), sf);
                drawing.DrawString(item.Sobrenome, font, textBrushBranco, new RectangleF(445 + textSizeNome.Width, y, textSizeSobrenome.Width, textSizeSobrenome.Height), sf);
                //drawing.DrawString(item.Clube, font, textBrushBranco, new RectangleF(880, y, textSizeClube.Width, textSizeClube.Height), sf);
                drawing.DrawString(item.Tempo, font, textBrushBranco, new RectangleF(1430, y, textSizeTempo.Width, textSizeTempo.Height), sf);
                y = y + 80;
            }
            drawing.Save();
            textBrush.Dispose();
            drawing.Dispose();
            img.Save(path, ImageFormat.Png);
            img.Dispose();
        }

        private void GerarIMG_Balizamento()
        {
            var resultado = GerarResultado();
            string path = ConfigurationManager.AppSettings["PathImagensPng"].ToString() + "BALIZAMENTO.png"; //@"C:\Users\franklinbarros\Desktop\PNG\teste.png";
            //Color textColor = Color.Black;
            Font font = new Font(FontFamily.GenericSansSerif, 30);
            Font fontBold = new Font(FontFamily.GenericSansSerif, 30, FontStyle.Bold);
            Font fontTitulo = new Font(FontFamily.GenericSerif, 40);
            int maxWidth = 1920;

            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            StringFormat sf = new StringFormat();

            sf.Trimming = StringTrimming.Word;

            img.Dispose();
            drawing.Dispose();

            img = new Bitmap(1920, 1080);
            drawing = Graphics.FromImage(img);

            drawing.CompositingQuality = CompositingQuality.HighQuality;
            drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
            drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
            drawing.SmoothingMode = SmoothingMode.HighQuality;
            drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            drawing.Clear(Color.Transparent);

            Brush textBrush = new SolidBrush(Color.Black);
            Brush textBrushBranco = new SolidBrush(Color.White);
            Brush textBrushAzul = new SolidBrush(Color.DarkSlateBlue);
            SizeF textSizeProva = drawing.MeasureString(resultado.Prova.NomeDaProva, fontTitulo, maxWidth);
            drawing.DrawString(resultado.Prova.NomeDaProva, fontTitulo, textBrushBranco, new RectangleF(500, 230, textSizeProva.Width, textSizeProva.Height), sf);

            int y = 310;
            foreach (var item in resultado.Participantes)
            {
                SizeF textSizeColocacao = drawing.MeasureString(item.Raia.ToString(), font, maxWidth);
                SizeF textSizeNome = drawing.MeasureString(item.Nome, font, maxWidth);
                SizeF textSizeSobrenome = drawing.MeasureString(item.Sobrenome, font, maxWidth);
                //SizeF textSizeClube = drawing.MeasureString(item.Clube, font, maxWidth);
                SizeF textSizeTempo = drawing.MeasureString(item.Tempo, font, maxWidth);
                drawing.DrawString(item.Colocacao.ToString(), fontBold, textBrushAzul, new RectangleF(345, y, textSizeColocacao.Width, textSizeColocacao.Height), sf);

                Image bandeira = ObterBandeiraPais(item.Clube.ToUpper());//Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"].ToString() + "br.png");
                drawing.DrawImage(bandeira, 410, y + 2, 58, 41);

                drawing.DrawString(item.Nome, font, textBrushBranco, new RectangleF(455, y, textSizeNome.Width, textSizeNome.Height), sf);
                drawing.DrawString(item.Sobrenome, font, textBrushBranco, new RectangleF(445 + textSizeNome.Width, y, textSizeSobrenome.Width, textSizeSobrenome.Height), sf);
                //drawing.DrawString(item.Clube, font, textBrushBranco, new RectangleF(880, y, textSizeClube.Width, textSizeClube.Height), sf);
                y = y + 80;
            }
            drawing.Save();
            textBrush.Dispose();
            drawing.Dispose();
            img.Save(path, ImageFormat.Png);
            img.Dispose();

            GerarIMG_Balizamento_Individual();
        }

        private void GerarIMG_Balizamento_Individual()
        {
            var resultado = GerarResultado();
            foreach (var item in resultado.Participantes)
            {
                string path = ConfigurationManager.AppSettings["PathImagensPng"].ToString() + "INDIVIDUAL_RAIA_" + item.Raia + ".png"; //@"C:\Users\franklinbarros\Desktop\PNG\teste.png";
                                                                                                                                              //Color textColor = Color.Black;
                Font font = new Font(FontFamily.GenericSansSerif, 30);
                Font fontBold = new Font(FontFamily.GenericSansSerif, 30, FontStyle.Bold);
                Font fontTitulo = new Font(FontFamily.GenericSerif, 40);
                int maxWidth = 1920;

                Image img = new Bitmap(1, 1);
                Graphics drawing = Graphics.FromImage(img);

                StringFormat sf = new StringFormat();

                sf.Trimming = StringTrimming.Word;

                img.Dispose();
                drawing.Dispose();

                img = new Bitmap(1920, 1080);
                drawing = Graphics.FromImage(img);

                drawing.CompositingQuality = CompositingQuality.HighQuality;
                drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
                drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
                drawing.SmoothingMode = SmoothingMode.HighQuality;
                drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                drawing.Clear(Color.Transparent);

                Brush textBrush = new SolidBrush(Color.Black);
                Brush textBrushBranco = new SolidBrush(Color.White);
                Brush textBrushAzul = new SolidBrush(Color.DarkSlateBlue);
                //SizeF textSizeProva = drawing.MeasureString(resultado.Prova.NomeDaProva, fontTitulo, maxWidth);
                //drawing.DrawString(resultado.Prova.NomeDaProva, fontTitulo, textBrushBranco, new RectangleF(500, 230, textSizeProva.Width, textSizeProva.Height), sf);

                int y = 935;
                
                SizeF textSizeColocacao = drawing.MeasureString(item.Raia.ToString(), font, maxWidth);
                SizeF textSizeNome = drawing.MeasureString(item.Nome, font, maxWidth);
                SizeF textSizeSobrenome = drawing.MeasureString(item.Sobrenome, font, maxWidth);
                //SizeF textSizeClube = drawing.MeasureString(item.Clube, font, maxWidth);
                SizeF textSizeTempo = drawing.MeasureString(item.Tempo, font, maxWidth);
                drawing.DrawString(item.Raia.ToString(), fontBold, textBrushAzul, new RectangleF(175, y, textSizeColocacao.Width, textSizeColocacao.Height), sf);

                Image bandeira = ObterBandeiraPais(item.Clube.ToUpper());//Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"].ToString() + "br.png");
                drawing.DrawImage(bandeira, 240, y + 2, 58, 41);

                drawing.DrawString(item.Nome, font, textBrushBranco, new RectangleF(285, y, textSizeNome.Width, textSizeNome.Height), sf);
                drawing.DrawString(item.Sobrenome, font, textBrushBranco, new RectangleF(275 + textSizeNome.Width, y, textSizeSobrenome.Width, textSizeSobrenome.Height), sf);
                //drawing.DrawString(item.Clube, font, textBrushBranco, new RectangleF(880, y, textSizeClube.Width, textSizeClube.Height), sf);

                drawing.Save();
                textBrush.Dispose();
                drawing.Dispose();
                img.Save(path, ImageFormat.Png);
                img.Dispose();
            }            
        }

        private void AtualizarPaginaDeResultados()
        {
            var resultado = GerarResultado();
            
            var atletasInscritos = Atletas();

            foreach (var item in resultado.Participantes)
            {
                var atletaInscrito = atletasInscritos.FirstOrDefault(a => a.ID_Pessoa == item.Identificacao);
                if (atletaInscrito != null)
                {
                    double itcProva = atletaInscrito.ITC;
                    double itcResultado = Util.ConverterMarcaParaMS(item.Tempo);
                    item.ITC = Convert.ToDouble(((itcProva / itcResultado) * 100).ToString("###,##0.000"));
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><meta charset=\"utf-8\" />");
            sb.Append("<head>");
            sb.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"styles.css\">");
            sb.Append("</head>");
            sb.Append($"<body>");
            sb.Append($@"<div style=""position:relative;"">
                        <div class=""top""><div class=""logo""><img src=""Imagens/logo_cpb.png"" /></div>
                        </div><div class=""conteudo""><p style=""margin-bottom:0"">{resultado.Prova.NomeDaProva} {resultado.Prova.VelocidadeDoVento}
                        </p>
                        <div style=""overflow-y:scroll;height:370px"" >
                        <table class=""tabela"">");
            foreach (var item in resultado.Participantes)
            {
                sb.Append("<tr>");
                sb.Append($@"<td width=""5%"">{item.Colocacao}</td><td width=""5%"">{item.Identificacao}</td><td width=""5%"">{item.Raia}</td><td width=""20%"" style=""white-space:nowrap;"">{item.Nome} {item.Sobrenome}</td><td width=""20%"" style=""white-space:nowrap;"">{item.Clube}</td><td width=""10%"">{item.Tempo}</td></td><td width=""10%"">{item.ITC}</td>");
                sb.Append("</tr>");
            }
            sb.Append(@"</table></div></div><div class=""rodape""><img src=""Imagens/logo_loterias.png"" style=""margin-top:120px;margin-left:50px"" />
                        <img src=""Imagens/logo_braskem.png"" style=""margin-left:30px;margin-bottom:30px"" /></div></div>");
            sb.Append("</body>");
            sb.Append("</html>");

            CriarPaginaHtml(sb.ToString());

            AtualizarPagina();

            if (resultado.Participantes.Where(p => p.Tempo == "").Count() > 0)
            {
                GerarIMG_Balizamento();
            }
            else
            {
                GerarIMG_Classificacao();
            }
        }

        //private void AtualizarPaginaDeResultados()
        //{

        //    StringBuilder sb = new StringBuilder();
        //    sb.Append("<!DOCTYPE html><html><meta charset=\"utf-8\" />");
        //    sb.Append("<head>");
        //    sb.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"styles.css\">");
        //    sb.Append("</head>");
        //    sb.Append($"<body>");
        //    sb.Append($@"<div style=""position:relative;"">
        //  <div class=""top""><div class=""logo""><img src=""Imagens/logo_cpb.png"" /></div>
        //</div><div class=""conteudo""><p style=""margin-bottom:80px"">{resultado.Prova.NomeDaProva} {resultado.Prova.VelocidadeDoVento}
        //</p>
        //    <table class=""tabela"" >
        //        <tr>
        //            <th width=""5%"">Col</th>
        //            <th width=""5%"">Num</th>
        //            <th width=""5%"">Raia</th>
        //            <th width=""20%"">Nome</th>
        //            <th width=""20%"">Equipe</th>
        //            <th width=""10%"">Res</th>
        //        </tr>");
        //    foreach (var item in resultado.Participantes)
        //    {
        //        sb.Append("<tr>");
        //        sb.Append($"<td>{item.Colocacao}</td><td>{item.Identificacao}</td><td>{item.Raia}</td><td>{item.Nome} {item.Sobrenome}</td><td>{item.Clube}</td><td>{item.Tempo}</td>");
        //        sb.Append("</tr>");
        //    }
        //    sb.Append(@"</table></div><div class=""rodape""><img src=""Imagens/logo_loterias.png"" style=""margin-top:120px;margin-left:50px"" />
        //                <img src=""Imagens/logo_braskem.png"" style=""margin-left:30px;margin-bottom:30px"" /></div></div>");
        //    sb.Append("</body>");
        //    sb.Append("</html>");

        //    CriarPaginaHtml(sb.ToString());

        //    AtualizarPagina();
        //}

        private string ObterLocalPagina() => string.Concat(_pastaPaginas, INDEX_HTML);

        private void CriarPaginaHtml(string conteudo)
        {
            using (FileStream fs = File.Create(ObterLocalPagina()))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(conteudo);
                fs.Write(info, 0, info.Length);
            }
        }

        private Image ObterBandeiraPais(string sigla)
        {
            Image retorno;
            switch (sigla)
            {
                case "ARG":// ARGENTINA
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "argentina.png");
                    break;
                case "CHI":// CHILE
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "chile.png");
                    break;
                case "CUB":// CUBA
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "cuba.png");
                    break;
                case "ECU":// EQUADOR
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "equador.png");
                    break;
                case "ESA":// EL SALVADOR
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "elsalvador.png");
                    break;
                case "GHA":// GHANA
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "gana.png");
                    break;
                case "ISR":// ISRAEL
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "israel.png");
                    break;
                case "MEX":// MÉXICO
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "mexico.png");
                    break;
                case "PER":// PERU
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "peru.png");
                    break;
                case "POR":// PORTUGAL
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "portugal.png");
                    break;
                case "RSA":// AFRICA DO SUL
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "africasul.png");
                    break;
                case "TUR":// TURQUIA
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "turquia.png");
                    break;
                default:
                    retorno = Image.FromFile(ConfigurationManager.AppSettings["PathBandeiras"] + "brazil.png");
                    break;
            }

            return retorno;
        }

        private List<AtletaInscrito> Atletas()
        {
            List<AtletaInscrito> retorno = new List<AtletaInscrito>();

            foreach (var worksheet in Workbook.Worksheets(ConfigurationManager.AppSettings["PathPaginas"] +  @"Excel\ATLETAS_DESAFIOS.xlsx"))
            {
                foreach (var row in worksheet.Rows)
                {
                    AtletaInscrito atleta = new AtletaInscrito();
                    atleta.Desafio = row.Cells[0].Text;
                    atleta.CFP = row.Cells[1].Text;
                    atleta.Classe = row.Cells[3].Text;
                    atleta.Genero = row.Cells[4].Text;
                    atleta.Clube = row.Cells[5].Text;
                    atleta.ID_Pessoa = row.Cells[6].Text;
                    atleta.Atleta = row.Cells[7].Text;
                    atleta.ITC = Convert.ToInt32(row.Cells[11].Text.Replace(".", ""));
                    retorno.Add(atleta);
                }
            }
            return retorno;
        }
    }
}
