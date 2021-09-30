using Compilador.Enums;

namespace Compilador.Services
{
    public class Token
    {
        public Token(TokenType tipo, string termo)
        {
            this.Tipo = tipo;
            this.Termo = termo;
        }
        public TokenType Tipo { get; set; }
        public string Termo { get; set; }
    }
}
