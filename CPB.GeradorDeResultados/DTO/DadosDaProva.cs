namespace CPB.GeradorDeResultados.DTO
{
    public class DadosDaProva
    {
        public string CodigoDaProva { get; set; }
        public string CodigoEtapa { get; set; }
        public string CodigoSerie { get; set; }
        public string NomeDaProva { get; set; }
        public string VelocidadeDoVento { get; set; }
        public string MetrosPorSegundo { get; set; }
        public string HoraPartida { get; set; }

        public override string ToString()
        {
            return NomeDaProva;
        }
    }
}
