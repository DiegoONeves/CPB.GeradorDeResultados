using System.Collections.Generic;
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
            Prova.HoraPartida = dadosPrimeiraLinha[10];
        }

        public void ResolverParticipante(string[] dadosParticipante)
        {
            Participantes.Add(new Participante
            {
                Colocacao = int.Parse(dadosParticipante[0]),
                Identificacao = dadosParticipante[1],
                Raia = int.Parse(dadosParticipante[2]),
                Nome = dadosParticipante[4],
                Sobrenome = dadosParticipante[3],
                Clube = dadosParticipante[5],
                Tempo = dadosParticipante[6]
            });
        }
    }
}
