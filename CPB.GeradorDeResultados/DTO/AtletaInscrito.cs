namespace CPB.GeradorDeResultados.DTO
{
    public class AtletaInscrito
    {
        public string Desafio { get; set; }
        public string CFP { get; set; }
        public string Prova { get; set; }
        public string Classe { get; set; }
        public string Genero { get; set; }
        public string Clube { get; set; }
        public string ID_Pessoa { get; set; }
        public string Atleta { get; set; }
        public int ITC { get; set; }

        public override string ToString()
        {
            return Atleta;
        }
    }
}
