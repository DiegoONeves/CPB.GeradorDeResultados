using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CPB.GeradorDeResultados.DTO
{
    public class ResultadoDaProva
    {
        public int QtdParticipantes { get { return Participantes.Count; } }
        public IList<Participante> ObterEmOrdemDeColocacao { get { return Participantes.OrderBy(x => x.Colocacao).ToList(); } }

        public ResultadoDaProva()
        {
            Prova = new DadosDaProva();
            Participantes = new List<Participante>();
        }
        public DadosDaProva Prova { get; set; }
        public IList<Participante> Participantes { get; set; }

        public void ResolverDadosDaProva(string[] dadosPrimeiraLinha)
        {
            Prova.CodigoDaProva = dadosPrimeiraLinha[0];
            Prova.CodigoEtapa = dadosPrimeiraLinha[1];
            Prova.CodigoSerie = dadosPrimeiraLinha[2];
            Prova.NomeDaProva = dadosPrimeiraLinha[3];
            Prova.VelocidadeDoVento = dadosPrimeiraLinha[4] != string.Empty ? "- Vento: " + dadosPrimeiraLinha[4].Replace("(Manual)", "") + " M/S" : "";
            Prova.HoraPartida = dadosPrimeiraLinha[10];
        }

        public void ResolverParticipante(string[] dadosParticipante)
        {
            Participantes.Add(new Participante
            {
                Colocacao = dadosParticipante[0],
                Identificacao = dadosParticipante[1],
                Raia = dadosParticipante[2],
                Nome = Capitalize(ObterString(dadosParticipante[4], 10)),
                Sobrenome = Capitalize(ObterString(dadosParticipante[3],10)),
                Clube = Capitalize(ObterString(dadosParticipante[5],20)),
                Tempo = dadosParticipante[6]
            });
        }

        private string ObterString(string nome, int tam)
        {
            if (!string.IsNullOrWhiteSpace(nome) && nome.Length > tam)
                return nome.Substring(0, tam);

            return nome;
        }

        private string Capitalize(string title)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }
    }
}
