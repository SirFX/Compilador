using Compilador.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compilador.Services
{
    public class Sintatico
    {
        private Lexico _lexico;
        private Token simbolo;
        private TokenType _tipo;
        private string[] palavrasReservadas = { "program", "begin", "end", "real", "integer", "read", "write", "if", "then", "else" };
        public string[] CodigoFormatado;
        IDictionary<string, Simbolo> TabelaSimbolo = new Dictionary<string, Simbolo>();
        private string codigo = "operador;arg1;arg2;result\n";
        private int temp = 1;

        private int posListaIfs = -1;
        private bool tlmode = false;
        private bool flmode = false;

        public Sintatico(string arq)
        {
            _lexico = new Lexico(arq);
        }

        private string geratemp()
        {
            var t = $"t {temp}";
            temp++;
            return t;
        }

        private string code(string op, string arg1, string arg2, string result)
        {
            return $"{op};{arg1};{arg2};{result}\n";
        }

        private void obtemSimbolo()
        {
            simbolo = _lexico.NextToken();
        }

        private bool verificaSimbolo(string termo)
        {
            return simbolo != null && simbolo.Termo == termo;
        }

        private TokenType tipo_var()
        {
            if (verificaSimbolo("real"))
            {
                obtemSimbolo();
                return TokenType.NUMERO_REAL;
            }
            else if (verificaSimbolo("integer"))
            {
                obtemSimbolo();
                return TokenType.NUMERO_INTEIRO;
            }
            else
            {
                throw new Exception($"Erro sintatico. Tipo '{simbolo.Termo}' incorreto, esperado 'real' ou 'integer'");
            }
        }

        private string mais_var(TokenType mais_varEsq)
        {
            if (verificaSimbolo(","))
            {
                obtemSimbolo();

                var variaveisCodigo = variaveis(mais_varEsq);
                return variaveisCodigo;
            }
            else
                return "";
        }

        private string variaveis(TokenType variaveisEsq)
        {
            string codigoGerado;

            if (simbolo.Tipo == TokenType.IDENTIFICADOR && (!palavrasReservadas.Contains(simbolo.Termo)))
            {
                if (TabelaSimbolo.ContainsKey(simbolo.Termo))
                {
                    throw new Exception($"Erro Semantico. Já existe uma váriavel com o nome '{simbolo.Termo}'");
                }
                else
                {
                    TabelaSimbolo[simbolo.Termo] = new Simbolo(variaveisEsq, simbolo.Termo);

                    if (variaveisEsq == TokenType.NUMERO_INTEIRO)
                    {
                        codigoGerado = code("ALME", "0", "", simbolo.Termo);
                    }
                    else
                    {
                        codigoGerado = code("ALME", "0.0", "", simbolo.Termo);
                    }
                }

                obtemSimbolo();

                var mais_varCodigo = mais_var(variaveisEsq);
                return codigoGerado + mais_varCodigo;
            }
            else
            {
                throw new Exception($"Erro sintatico. Encontrado '{simbolo.Termo}', mas era esperado '{TokenType.IDENTIFICADOR}'");
            }
        }

        private string dc_v()
        {
            var tipo_varDir = tipo_var();

            if (verificaSimbolo(": "))
            {
                obtemSimbolo();

                var variaveisCodigo = variaveis(tipo_varDir);
                return variaveisCodigo;
            }
            else
            {
                throw new Exception($"Erro sintatico. Econtrado '{simbolo.Termo}', mas era esperado ':'");
            }
        }

        private string mais_dc()
        {
            if (verificaSimbolo(";"))
            {
                obtemSimbolo();

                var dcCodigo = dc();
                return dcCodigo;
            }
            else
            {
                return "";
            }
        }

        private string dc()
        {
            if ( verificaSimbolo("real") || verificaSimbolo("integer"))
            {
                var dc_vCodigo = dc_v();

                var mais_dcCodigo = mais_dc();

                return dc_vCodigo + mais_dcCodigo;
            }
            else
            {
                return "";
            }

        }
        
        private string mais_comandos(string codigoGerado)
        {
            if(verificaSimbolo(";"))
            {
                obtemSimbolo();

                var comandosCodigo = comandos(codigoGerado);
                codigoGerado = comandosCodigo;
                return codigoGerado;
            }
            else
            {
                return codigoGerado;
            }
        }

        private string comandos(string codigoGerado)
        {
            var comandoCodigo = comando(codigoGerado);

            var mais_comandosCodigo = mais_comandos(comandoCodigo);

            return mais_comandosCodigo;
        }

        private string comando(string codigoGerado)
        {
            if (verificaSimbolo("read"))
            {
                obtemSimbolo();

                if (verificaSimbolo("("))
                {
                    obtemSimbolo();

                    if (simbolo.Tipo == TokenType.IDENTIFICADOR && (!palavrasReservadas.Contains(simbolo.Termo)))
                    {
                        if (!TabelaSimbolo.ContainsKey(simbolo.Termo))
                        {
                            throw new Exception($"Erro Semantico. Variavel {simbolo.Termo} não foi declarada");
                        }

                        codigoGerado += code("read", "", "", simbolo.Termo);
                        obtemSimbolo();

                        if (verificaSimbolo(")"))
                        {
                            obtemSimbolo();
                            return codigoGerado;
                        }
                        else
                        {
                            throw new Exception($"Erro sintatico. econtrado '{simbolo.Termo}' esperado: ')' ");
                        }
                    }
                    else
                    {
                        throw new Exception($"Erro sintatico. econtrado '{simbolo.Termo}', era esperado '{TokenType.IDENTIFICADOR}'");
                    }
                }
                else
                {
                    throw new Exception($"Erro sintático. Econtrado {simbolo.Termo}, era esperado: ')'");
                }
            }
            else
            {
                if (verificaSimbolo("write"))
                {
                    obtemSimbolo();

                    if (verificaSimbolo("("))
                    {
                        obtemSimbolo();

                        if (simbolo.Tipo == TokenType.IDENTIFICADOR && (!palavrasReservadas.Contains(simbolo.Termo)))
                        {
                            if (!TabelaSimbolo.ContainsKey(simbolo.Termo))
                            {
                                throw new Exception($"Erro semantico. Variavel {simbolo.Termo} não foi declarada");
                            }

                            codigoGerado += code("write", simbolo.Termo, "", "");
                            obtemSimbolo();

                            if (verificaSimbolo(")"))
                            {
                                obtemSimbolo();
                                return codigoGerado;
                            }
                            else
                            {
                                throw new Exception($"Erro sintatico. econtrado '{simbolo.Termo}' esperado: ')' ");
                            }
                        }
                        else
                        {
                            throw new Exception($"Erro sintatico. econtrado '{simbolo.Termo}', era esperado '{TokenType.IDENTIFICADOR}'");
                        }
                    }
                    else
                    {
                        throw new Exception($"Erro sintático. Econtrado {simbolo.Termo}, era esperado: ')'");
                    }
                }
                else
                {
                    if (verificaSimbolo("if"))
                    {
                        obtemSimbolo();

                        var condicoes = condicao();
                        var condicaoCodigo = condicoes.Item1;
                        var condicaoDir = condicoes.Item2;

                        if (verificaSimbolo("then"))
                        {
                            obtemSimbolo();

                            codigoGerado += condicaoCodigo;
                            codigoGerado += code("JF", condicaoDir, "__", "");

                            var comandosCodigo = comandos(codigoGerado);

                            var comandoSomente = comandosCodigo.Replace(codigoGerado, "");
                            var tamanhoThen = comandoSomente.Where(c => c.Equals('\n')).Count();
                            var index = codigoGerado.LastIndexOf("__");

                            codigoGerado = codigoGerado.Substring(index) + $"_{tamanhoThen + 2}" + codigo[index + 2];

                            codigoGerado += comandoSomente;

                            codigoGerado += code("goto", "__", "", "");
                            var pfalsacodigo = pfalsa(codigoGerado);

                            var pfalsaSomente = pfalsacodigo.Replace(codigoGerado, "");
                            var tamanhoElse = pfalsaSomente.Where(c => c.Equals('\n')).Count();
                            index = codigoGerado.LastIndexOf("__");
                            codigoGerado = codigoGerado.Substring(index) + $"_{tamanhoElse + 1}" + codigo[index + 2];

                            codigoGerado += pfalsaSomente;

                            if(verificaSimbolo("$"))
                            {
                                obtemSimbolo();
                                return codigoGerado;
                            }
                            else
                            {
                                throw new Exception("Erro sintatico. esperado '$'");
                            }
                        }
                        else
                        {
                            throw new Exception("Erro sintatico. esperado 'then'");
                        }

                    }
                    else
                    {
                        if (simbolo.Tipo == TokenType.IDENTIFICADOR && (!palavrasReservadas.Contains(simbolo.Termo)))
                        {
                            if(!TabelaSimbolo.ContainsKey(simbolo.Termo))
                            {
                                throw new Exception($"Erro semantico. Variavel {simbolo.Termo} não foi declarada");
                            }

                            var id = simbolo.Termo;
                            obtemSimbolo();

                            if(verificaSimbolo(":"))
                            {
                                obtemSimbolo();
                                if (verificaSimbolo("="))
                                {
                                    obtemSimbolo();
                                    var expressoes = expressao();
                                    var expressaoCodigo = expressoes.Item1;
                                    var expressaoDir = expressoes.Item2;

                                    codigoGerado += expressaoCodigo;
                                    codigoGerado += code(":=", expressaoDir, "", id);

                                    return codigoGerado;
                                }
                                throw new Exception("Erro sintatico, esperado =");
                            }
                            else
                            {
                                throw new Exception($"Erro sintatico. Esperado ':=' foi encontrado {simbolo.Termo}");
                            }
                        }
                        else
                        {
                            throw new Exception($"Erro sintatico, esperado 'read', 'write', 'if' ou {TokenType.IDENTIFICADOR}");
                        }
                    }
                }
            }
        }

        private Tuple<string, string> condicao()
        {
            var expressoes = expressao();
            var expressaoCodigo = expressoes.Item1;
            var expressaoDir = expressoes.Item2;

            var relacaoDir = relacao();

            var expressoeslinha = expressao();
            var expressaoLinhaCodigo = expressoeslinha.Item1;
            var expressaoLinhaDir = expressoeslinha.Item2;

            var t = geratemp();

            var codigoGerado = expressaoCodigo + expressaoLinhaCodigo;
            codigoGerado += code(relacaoDir, expressaoDir, expressaoLinhaDir, t);

            return new Tuple<string, string>(codigoGerado, t);

        }

        private string relacao()
        {
            if (verificaSimbolo("="))
            {
                obtemSimbolo();
                return "=";
            }
            if (verificaSimbolo("<>"))
            {
                obtemSimbolo();
                return "<>";
            }
            if (verificaSimbolo(">="))
            {
                obtemSimbolo();
                return ">=";
            }
            if (verificaSimbolo("<="))
            {
                obtemSimbolo();
                return "<=";
            }
            if (verificaSimbolo(">"))
            {
                obtemSimbolo();
                return ">";
            }
            if (verificaSimbolo("<"))
            {
                obtemSimbolo();
                return "<";
            }
            throw new Exception($"Erro sintatico, esperado '=', '<>', '>=', '<=', '>' ou '<'");
        }

        private Tuple<string, string> expressao()
        {
            var termos = termo();

            var termoCodigo = termos.Item1;
            var termoDir = termos.Item2;

            var outrosTermos = outros_termos(termoDir);
            var outros_termosCodigo = outrosTermos.Item1;
            var expressaoDir = outrosTermos.Item2;

            var codigoGerado = termoCodigo + outros_termosCodigo;

            return new Tuple<string, string>(codigoGerado, expressaoDir);
        }

        private Tuple<string, string> termo()
        {
            var op_unDir = op_un();

            var fatores = fator(op_unDir);
            var fatorCodigo = fatores.Item1;
            var fatorDir = fatores.Item2;

            var maisFatores = mais_fatores(fatorDir);
            var mais_fatoresCodigo = maisFatores.Item1;
            var termoDir = maisFatores.Item2;

            return new Tuple<string, string>(fatorCodigo + mais_fatoresCodigo, termoDir);
        }

        private string op_un()
        {
            if(verificaSimbolo("-"))
            {
                obtemSimbolo();
                return "-";
            }
            return "";
        }

        private Tuple<string, string> mais_fatores(string mais_fatoresEsq)
        {
            if (verificaSimbolo("*") || verificaSimbolo("/"))
            {
                var op_mulDir = op_mul();

                var fatores = fator("");
                var fatorCodigo = fatores.Item1;
                var fatorDir = fatores.Item2;

                var t = geratemp();

                var codigoGerado = fatorCodigo;

                codigoGerado += code(op_mulDir, mais_fatoresEsq, fatorDir, t);

                var maisfatores = mais_fatores(t);

                var mais_fatorescodigo = maisfatores.Item1;
                var mais_fatoresDir = maisfatores.Item2;

                codigoGerado += mais_fatorescodigo;

                return new Tuple<string, string>(codigoGerado, mais_fatoresDir);
            }
            return new Tuple<string, string>("", mais_fatoresEsq);
        }

        private Tuple<string, string> outros_termos(string outros_termosEsq)
        {
            if (verificaSimbolo("+") || verificaSimbolo("-"))
            {
                var op_adDir = op_ad();

                var termos = termo();
                var termoCodigo = termos.Item1;
                var termoDir = termos.Item2;

                var t = geratemp();

                var codigoGerado = termoCodigo;

                codigoGerado += code(op_adDir, outros_termosEsq, termoDir, t);

                var outrosTermos = outros_termos(t);
                var outros_termosCodigo = outrosTermos.Item1;
                var outros_termosDir = outrosTermos.Item2;

                codigoGerado += outros_termosCodigo;

                return new Tuple<string, string>(codigoGerado, outros_termosDir);
            }
            return new Tuple<string, string>("", outros_termosEsq);
        }

        private string op_ad()
        {
            if (verificaSimbolo("+"))
            {
                obtemSimbolo();
                return "+";
            }
            else
            {
                if (verificaSimbolo("-"))
                {
                    obtemSimbolo();
                    return "-";
                }
                else
                {
                    throw new Exception($"Erro Sintatico. Esperado '+' ou '-'");
                }
            }
        }

        private string op_mul()
        {
            if (verificaSimbolo("*"))
            {
                obtemSimbolo();
                return "*";
            }
            else
            {
                if (verificaSimbolo("/"))
                {
                    obtemSimbolo();
                    return "/";
                }
                else
                {
                    throw new Exception($"Erro Sintatico. Esperado '*' ou '/'");
                }
            }
        }

        private string pfalsa(string codigoGerado)
        {
            if (verificaSimbolo("else"))
            {
                obtemSimbolo();

                var codigosComandos = comandos(codigoGerado);

                codigoGerado = codigosComandos;

                return codigoGerado;
            }

            return codigoGerado;
        }

        private Tuple<string, string> fator(string fatorEsq)
        {
            string fatorDir;
            string codigoGerado;
            if (simbolo.Tipo == TokenType.IDENTIFICADOR && (!palavrasReservadas.Contains(simbolo.Termo)))
            {
                if (!TabelaSimbolo.ContainsKey(simbolo.Termo))
                {
                    throw new Exception($"Erro semantico. variavel {simbolo.Termo} não foi declarada");
                }

                if (fatorEsq == "-")
                {
                    var t = geratemp();
                    codigoGerado = code("uminus", simbolo.Termo, "", t);
                    fatorDir = t;
                }
                else
                {
                    fatorDir = simbolo.Termo;
                    codigoGerado = "";
                }
                obtemSimbolo();

                return new Tuple<string, string>(codigoGerado, fatorDir);
            }
            else
            {
                if(simbolo.Tipo == TokenType.NUMERO_INTEIRO || simbolo.Tipo == TokenType.NUMERO_REAL)
                {
                    if (fatorEsq == "-")
                    {
                        var t = geratemp();
                        codigoGerado = code("uminus", simbolo.Termo, "", t);
                        fatorDir = t;
                    }
                    else
                    {
                        fatorDir = simbolo.Termo;
                        codigoGerado = "";
                    }
                    return new Tuple<string, string>(codigoGerado, fatorDir);
                }
                else
                {
                    if (verificaSimbolo("("))
                    {
                        obtemSimbolo();

                        var expressoes = expressao();
                        var expressaoCodigo = expressoes.Item1;
                        var expressaoDir = expressoes.Item2;

                        if (fatorEsq == "-")
                        {
                            var t = geratemp();
                            codigoGerado = code("uminus", expressaoDir, "", t);
                            fatorDir = t;
                        }
                        else
                        {
                            fatorDir = expressaoDir;
                            codigoGerado = "";
                        }

                        if (verificaSimbolo(")"))
                        {
                            obtemSimbolo();
                            return new Tuple<string, string>(codigoGerado, fatorDir);
                        }
                        else
                        {
                            throw new Exception($"Erro sintatico. econtrado '{simbolo.Termo}', era esperado ')'");
                        }
                    }
                    else
                    {
                        throw new Exception($"Erro sintatico. esperado '{TokenType.IDENTIFICADOR}', '{TokenType.NUMERO_INTEIRO}', '{TokenType.NUMERO_REAL}' ou '('");
                    }
                }
            }
        }



        private string corpo()
        {
            var dcCodigo = dc();

            if (verificaSimbolo("begin"))
            {
                obtemSimbolo();

                var comandosCodigo = comandos(dcCodigo);

                if(verificaSimbolo("end"))
                {
                    obtemSimbolo();
                    return comandosCodigo;
                }
                else
                {
                    throw new Exception($"Erro sintatico, esperado 'end' {simbolo.Termo}");
                }
            }
            else
            {
                throw new Exception("Erro sintatico, esperado 'begin'");
            }
        }

        private string programa()
        {
            if (verificaSimbolo("program"))
            {
                obtemSimbolo();

                if (simbolo.Tipo == TokenType.IDENTIFICADOR && (!palavrasReservadas.Contains(simbolo.Termo)))
                {
                    obtemSimbolo();

                    var corpoCodigo = corpo();

                    if (verificaSimbolo("."))
                    {
                        var codigoGerado = corpoCodigo;
                        codigoGerado += code("PARA", "", "", "");
                        obtemSimbolo();
                        return codigoGerado;
                    }
                    else
                    {
                        throw new Exception("Erro sintatico. Esperado '.'");
                    }
                }
                else
                {
                    throw new Exception($"Erro sintatico. Esperado {TokenType.IDENTIFICADOR}");
                }
            }
            else
            {
                throw new Exception("Erro sintatico. Esperado 'program'");
            }
                
        }

        public void analisar()
        {
            obtemSimbolo();

            var codigoPrograma = programa();

            if (simbolo == null)
            {
                var Codigo = codigoPrograma;

                var nLinha = 0;
                foreach (var linha in Codigo.Split("\n"))
                {
                    nLinha += 1;

                    var underlineIndex = linha.IndexOf("_");

                    if (underlineIndex == -1)
                    {
                        CodigoFormatado.Append(linha);
                        continue;
                    }

                    var semicolonIndex = linha.IndexOf(";", underlineIndex);
                    var size = linha.Substring(underlineIndex, semicolonIndex).Length;
                    CodigoFormatado.Append(linha.Substring(underlineIndex) + (size + nLinha) + linha.Substring(semicolonIndex + (size + nLinha).ToString().Length));
                }
            }
        }
    }
}
