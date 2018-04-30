namespace CPB.GeradorDeResultados.DTO
{
    public class Participante
    {
        public string Colocacao { get; set; }
        public string Identificacao { get; set; }
        public string Raia { get; set; }
        public string Nome { get; set; }
        public string Sobrenome { get; set; }
        public string Clube { get; set; }
        public string Tempo { get; set; }
        public double ITC { get; set; }

        public override string ToString()
        {
            return Nome;
        }
    }
}
