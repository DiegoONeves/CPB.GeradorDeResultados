namespace CPB.GeradorDeResultados.DTO
{
    public class Participante
    {
        public int Colocacao { get; set; }
        public string Identificacao { get; set; }
        public int Raia { get; set; }
        public string Nome { get; set; }
        public string Sobrenome { get; set; }
        public string Clube { get; set; }
        public string Tempo { get; set; }

        public override string ToString()
        {
            return Nome;
        }
    }
}
