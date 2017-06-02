using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace OFX2CSV
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write(args[0]);
            string path = "";
            try
            {
                path = Directory.GetCurrentDirectory();
                try
                {
                    string NomeArquivo = path + "\\" + args[0];

                    StreamReader file = new StreamReader(NomeArquivo);

                    string line, OFX = "", prefixo = "", sufixo = "";
                    bool printa = false;
                    int counter = 0;

                    while ((line = file.ReadLine()) != null)
                    {
                        

                        if (line == "<OFX>")
                        {
                            printa = true;
                        }

                        if (printa && line.Length > 0)
                        {
                            prefixo = line.Substring(0, line.IndexOf(">") + 1).Trim();
                            sufixo = "</" + prefixo.Substring(1, prefixo.Length - 2) + ">";

                            if (line.Substring(line.Length - 1, 1) != ">")
                            {
                                line += sufixo;
                            }

                            counter++;
                            //Console.WriteLine(line);
                            if (counter > 1)
                            {
                                OFX += Environment.NewLine;
                            }
                            OFX += line.Trim();
                        }
                    }
                    file.Close();

                    if (path != "")
                    {
                        bool bloco = false;
                        string linha_cab = "", linha_res = "", tipoDado = "";
                        int contador = 0;
                        string CSV = "";

                        File.WriteAllText(path + @"\temp.xml", OFX);
                        XmlTextReader reader = new XmlTextReader(path + @"\temp.xml");
                        while (reader.Read())
                        {
                            //Console.WriteLine(reader.NodeType);                   
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element: // The node is an element.                            
                                    if (reader.Name == "STMTTRN")
                                    {
                                        bloco = true;
                                        linha_cab = "";
                                        linha_res = "";
                                        contador++;
                                    }
                                    else if (bloco)
                                    {
                                        Console.Write(reader.Name + ": ");
                                        linha_cab += reader.Name + "; ";

                                        switch (reader.Name)
                                        {
                                            case "DTPOSTED":
                                                tipoDado = "Data";
                                                break;
                                            case "TRNAMT":
                                                tipoDado = "Moeda";
                                                break;
                                            default:
                                                tipoDado = "Texto";
                                                break;
                                        }

                                    }
                                    break;
                                case XmlNodeType.Text: //Display the text in each element.
                                    if (bloco)
                                    {
                                        Console.WriteLine(reader.Value);
                                        string linha = reader.Value;
                                        switch (tipoDado)
                                        {
                                            case "Data":
                                                linha = String.Format("{0:D}",
                                                    DateTime.ParseExact(linha.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture).ToShortDateString()
                                                    );
                                                break;
                                            case "Moeda":
                                                //linha = Decimal.Parse(linha.Replace(",")).ToString();
                                                break;
                                            default:
                                                break;
                                        }
                                        linha_res += linha + "; ";

                                    }
                                    break;
                                case XmlNodeType.EndElement: //Display the end of the element.
                                    if (reader.Name == "STMTTRN")
                                    {
                                        bloco = false;
                                        Console.WriteLine(linha_cab);
                                        Console.WriteLine(linha_res);
                                        Console.WriteLine("");

                                        if (contador == 1)
                                        {
                                            CSV += linha_cab.Substring(0, linha_cab.Length - 2);
                                        }
                                        CSV += Environment.NewLine + linha_res.Substring(0, linha_res.Length - 2);
                                    }

                                    break;
                            }
                        }
                        try
                        {
                            string nomeCSV = path + "\\arquivo_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                            File.WriteAllText(nomeCSV, CSV);
                            Console.WriteLine(nomeCSV + "\nArquivo criado com sucesso!");
                            //Console.WriteLine(path + @"\temp.xml");
                            try
                            {
                                reader.Close();
                                File.Delete(path + @"\temp.xml");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                            
                        }
                        catch
                        {
                            Console.WriteLine("Ocorreu um erro ao salvar o arquivo CSV.");
                        }

                    }
                }
                catch
                {
                    Console.WriteLine("Não foi possível abrir o arquivo especificado.");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("O processo falhou: {0}", e.ToString());
            }

            // Suspend the screen.
            // Console.ReadLine();
        }
    }
}
