using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;

namespace EngemixAnaliseAutomaizador
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void InserirArquivo(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textfilenameselect.Text = openFileDialog.FileName;
            }

            textboxlog.Text = $"Arquivo selecionado: {openFileDialog.FileName}";
        }
        private void ProcessarArquivo(object sender, EventArgs e)
        {
            ConnectionManager con = new ConnectionManager();

            var ColunaTkc = 0;
            var ColunaData = 0;
            var ColunaCodigoCB = 0;
            var dataTKC_AnaliseINT = "0";
            var CodigoCB_AnaliseINT = "0";
            var numTKC_AnaliseINT = "0";
            var ColunaStatus = 0;

            //Trata as informações do arquivo selecionado                     
            FileInfo fileInfo = new FileInfo(textfilenameselect.Text);
            ExcelPackage xlPackage = new ExcelPackage(fileInfo);

            //Percorre as abas da planilha
            foreach (ExcelWorksheet sheet in xlPackage.Workbook.Worksheets)
            {
                //Verifica se a aba a ser processada é a AnaliseINT
                if (sheet.Name == "AnaliseINT")
                {
                    textboxlog.AppendText("\n" + "\n" + "Processando Aba:" + sheet.Name);

                    //Percorre as colunas da aba da planilha para obter o indice das colunas a serem usadas
                    for (int column = 1; column <= sheet.Dimension.Columns; column++)
                    {
                        if (sheet.Cells[1, column].Text == "Código")
                        {
                            ColunaCodigoCB = column;
                        }
                        else if (sheet.Cells[1, column].Text == "TIQUETE")
                        {
                            ColunaTkc = column;
                        }
                        else if (sheet.Cells[1, column].Text == "Data")
                        {
                            ColunaData = column;
                        }
                        else if (sheet.Cells[1, column].Text == "Status")
                        {
                            ColunaStatus = column;
                        }
                    }

                    //Percorre as linhas fazendo a lógica de análise linha a linha
                    for (int row = 2; row <= sheet.Dimension.Rows; row++)
                    {
                        //Verifica se a célula já não está preenchida para não sobrescrever a análise
                        if (sheet.Cells[row, ColunaStatus].Text != "a")
                        {
                            //Pega os dados da linha a ser analisada
                            CodigoCB_AnaliseINT = sheet.Cells[row, ColunaCodigoCB].Text;
                            numTKC_AnaliseINT = sheet.Cells[row, ColunaTkc].Text;
                            dataTKC_AnaliseINT = sheet.Cells[row, ColunaData].Text;

                            //Seleciona os dados necessários do banco para análise
                            string queryRelIntegracao = $@"SELECT STATUS,TIME_READ ,TIME_WRITE, LATITUDE, LONGITUDE  FROM AVL_COMMAND_HISTORY 
                                                            WHERE ID_VIATURA = (SELECT id FROM AVL_VIATURA WHERE placa = 'CB3444') 
                                                            AND TIME_READ <= TO_DATE('17/02/2020 23:59:59', 'dd/MM/yyyy HH24:mi:ss')
                                                            AND TIME_READ >= TO_DATE('17/02/2020 00:00:00', 'dd/MM/yyyy HH24:mi:ss')
                                                            AND TICKET_CODE = '1434580'
                                                            ORDER BY TIME_READ ASC";
                            DataTable RelIntegracao = con.ReadDataTable(queryRelIntegracao);

                            string queryUltimaTransmissao = $"SELECT TIME_READ FROM AVL_POSITION WHERE ID_VIATURA = (SELECT id FROM AVL_VIATURA WHERE placa = 'CB3444')";
                            var UltimaTransmissao = con.ReadDataDateTime(queryUltimaTransmissao);

                            string queryRotaCriada = $@"SELECT ID, NAME, PERIOD_FROM, PERIOD_TO  FROM GOTO_ROUTE WHERE ID_MONITORED_POINT = (SELECT id FROM AVL_VIATURA WHERE placa = 'CB3444') 
                                                        AND PERIOD_FROM >= TO_DATE('17/2/2020 00:00:00', 'dd/MM/yyyy HH24:mi:ss')
                                                        AND PERIOD_TO <= TO_DATE('17/2/2020 23:59:59', 'dd/MM/yyyy HH24:mi:ss')
                                                        AND STATUS = 'A' AND ID_CLIENT =134";
                            DataTable RotaCriada = con.ReadDataTable(queryRotaCriada);

                            string queryDadosVeiculo = $@"SELECT PLACA, ID_CLIENTE, STATUS FROM AVL_VIATURA WHERE placa = 'CB3444'";
                            DataTable DadosVeiculo = con.ReadDataTable(queryDadosVeiculo);

                            object VeiculoAtivo = null;
                            try { VeiculoAtivo = DadosVeiculo.Select("STATUS = 'A'"); } catch (Exception ex) { }

                            string queryAtraso = $@"SELECT TIME_WRITE - TIME_READ FROM AVL_COMMAND_HISTORY 
                                                    WHERE ID_VIATURA = (SELECT id FROM AVL_VIATURA WHERE placa = 'CB3444') 
                                                    AND TIME_READ <= TO_DATE('17/2/2020 23:59:59', 'dd/MM/yyyy HH24:mi:ss')
                                                    AND TIME_READ >= TO_DATE('17/2/2020 00:00:00', 'dd/MM/yyyy HH24:mi:ss')
                                                    AND TICKET_CODE = '1434580'
                                                    AND (TIME_WRITE - TIME_READ) > 0.02";
                            int Atraso = con.ReadDataInt(queryAtraso); 

                            int StatusCount = 0;


                            List<string> ListaStatus = new List<string>();
                            foreach (DataRow rw in RelIntegracao.Rows)
                            {
                                ListaStatus.Add(rw["STATUS"].ToString());

                            }
                            if (ListaStatus.Contains("TJB")) { StatusCount++; }
                            if (ListaStatus.Contains("AJB")) { StatusCount++; }
                            if (ListaStatus.Contains("POU")) { StatusCount++; }
                            if (ListaStatus.Contains("TPL")) { StatusCount++; }
                            if (ListaStatus.Contains("WSH")) { StatusCount++; }
                            if (ListaStatus.Contains("IYD")) { StatusCount++; }

                            


                            //Encadeamento de análise
                            if (RelIntegracao.Rows.Count == 0) { sheet.Cells[row, ColunaStatus].Value = "Tíquete não Recebido"; }
                            else if (UltimaTransmissao == null) { sheet.Cells[row, ColunaStatus].Value = "Não Transmitiu"; }
                            else if (RotaCriada.Rows.Count == 0) { sheet.Cells[row, ColunaStatus].Value = "Rota não Encontrada - Regra Aplicação - Cadastro Tivit"; }
                            else if (VeiculoAtivo == null) { sheet.Cells[row, ColunaStatus].Value = "Veículo Desativado"; }
                            else if (StatusCount == 6) { sheet.Cells[row, ColunaStatus].Value = "Command Não Consumiu os Status"; }



                        }
                    }



                }

            }

            //Salvando o Arquivo
            xlPackage.Save();
            textboxlog.AppendText("\n" + "\n" + "Processamento Finalizado");

        }
    }
}
