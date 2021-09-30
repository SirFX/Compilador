using Compilador.Enums;
using System;
using System.IO;

namespace Compilador.Services
{
    public class Lexico
    {
        private char[] conteudo;
        private int pos;
        private int estado;
        public Lexico(String arq)
        {
            try
            {
                string currentDirectory = Environment.CurrentDirectory;
                string projectDirectory = Directory.GetParent(currentDirectory).Parent.Parent.FullName;
                conteudo = File.ReadAllText(Path.Combine(projectDirectory, arq)).ToCharArray();
                pos = 0;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
        private bool isLetra(char? c)
        {
            return ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'));
        }

        private bool isDigito(char? c)
        {
            return ((c >= '0' && c <= '9'));
        }

        private bool isEspaco(char? c)
        {
            return (c == ' ' || c == '\n' || c == '\t' || c == '\r');
        }

        private bool isEOF()
        {
            return pos >= conteudo.Length;
        }

        private char? nextChar()
        {
            if (isEOF())
            {
                return null;
            }
            return conteudo[pos++];
        }
        private void back()
        {
            pos--;
        }

        public Token NextToken()
        {
            if (isEOF())
                return null;

            estado = 0;
            char? c;
            string termo = "";

            while(true)
            {
                if (isEOF())
                {
                    pos = conteudo.Length + 1;
                }

                c = nextChar();
                switch (estado)
                {
                    case 0:
                        if (isEspaco(c))
                        {
                            estado = 0;
                        }
                        else if (isDigito(c))
                        {
                            estado = 1;
                            termo += c;
                        }
                        else if (isLetra(c))
                        {
                            estado = 3;
                            termo += c;
                        }
                        else if (c == '<' || c == '>' || c == ':')
                        {
                            estado = 4;
                            termo += c;
                        }
                        else
                        {
                            if (c == 0)
                            {
                                return null;
                            }
                            termo += c;
                            return new Token(TokenType.SIMBOLO, termo);
                        }
                        break;

                    case 1:
                        if (isDigito(c))
                        {
                            estado = 1;
                            termo += c;
                        }
                        else if (c == '.')
                        {
                            estado = 2;
                            termo += c;
                        }
                        else
                        {
                            back();
                            return new Token(TokenType.NUMERO_INTEIRO, termo);
                        }
                        break;

                    case 2:
                        if (isDigito(c))
                        {
                            estado = 2;
                            termo += c;
                        }
                        else
                        {
                            back();
                            return new Token(TokenType.NUMERO_REAL, termo);
                        }
                        break;

                    case 3:
                        if (isLetra(c) || isDigito(c))
                        {
                            estado = 3;
                            termo += c;
                        }
                        else
                        {
                            back();
                            return new Token(TokenType.IDENTIFICADOR, termo);
                        }
                        break;

                    case 4:

                        if(!isDigito(c) && !isLetra(c) && isEspaco(c))
                        {
                            estado = 4;
                            termo += c;
                        }
                        else
                        {
                            back();
                            return new Token(TokenType.SIMBOLO, termo);
                        }
                        break;
                }
            }
        }
    }
}
