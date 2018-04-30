using System;

namespace CPB.GeradorDeResultados
{
    public class Util
    {
        public static int ConverterMarcaParaMS(string resultado)
        {
            string minuto;
            string segundo;
            string milesegundos;

            minuto = resultado.Substring(0, 1);
            milesegundos = resultado.Substring(5, 2);
            segundo = resultado.Substring(2, 2);

            return (((Convert.ToInt32(minuto) * 60) + Convert.ToInt32(segundo)) * 100 + Convert.ToInt32(milesegundos)) * 10;
        }
    }
}
