using Compilador.Enums;

namespace Compilador.Services
{
    public class Simbolo
    {
        public TokenType Tipo { get; set; }
        public string Nome { get; set; }
        public Simbolo(TokenType tipo, string nome)
        {
            this.Nome = nome;
            this.Tipo = tipo;
        }        

        public string toString()
        {
            return $"{Nome} = {Tipo}";
        }
    }
}
