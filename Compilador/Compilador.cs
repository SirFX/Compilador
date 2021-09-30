using Compilador.Services;
using System;

namespace Compilador
{
    public class Compilador
    {
        static void Main(string[] args)
        {
            Sintatico sintatico = new Sintatico("exemplo.lalg.txt");
            sintatico.analisar();

            foreach (var linha in sintatico.CodigoFormatado)
            {
                Console.WriteLine(linha);
            }
        }
    }
}
