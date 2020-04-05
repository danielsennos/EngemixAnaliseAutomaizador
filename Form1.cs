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
                        if (sheet.Cells[1, column].Text == "CODIGO_BETONEIRA")
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

                            #region Dados necessários do banco para análise

                            string queryTableTKC = $@"SELECT TICKET_CODE, STATUS, TIME_READ ,TIME_WRITE, LATITUDE, LONGITUDE FROM GOTO_ENGEMIX.AVL_COMMAND_HISTORY 
                                                            WHERE ID_VIATURA = (SELECT id FROM GOTO_ENGEMIX.AVL_VIATURA WHERE placa = 'CB{CodigoCB_AnaliseINT}') 
                                                            AND TIME_READ >= TO_DATE('{dataTKC_AnaliseINT}/2020 00:00:00', 'dd/MM/yyyy HH24:mi:ss')
                                                            AND TIME_READ <= TO_DATE('{dataTKC_AnaliseINT}/2020 23:59:59', 'dd/MM/yyyy HH24:mi:ss') 
                                                            AND STATUS = 'TKC'
                                                            ORDER BY TIME_READ ASC";
                            DataTable TableTKC = con.ReadDataTable(queryTableTKC);

                            List<string> ListaTKC = new List<string>();
                            foreach (DataRow rw in TableTKC.Rows)
                            {
                                ListaTKC.Add(rw["TICKET_CODE"].ToString());
                                ListaTKC.Add(rw["TIME_READ"].ToString());

                            }

                            DateTime TempoInicioTKC = Convert.ToDateTime(ListaTKC[(ListaTKC.IndexOf(numTKC_AnaliseINT) + 1)]);
                            DateTime TempoFimTKC;
                            try { TempoFimTKC = Convert.ToDateTime(ListaTKC[(ListaTKC.IndexOf(numTKC_AnaliseINT) + 3)]); } catch { TempoFimTKC = Convert.ToDateTime($"{dataTKC_AnaliseINT}/2020 23:59:59"); };                         

                              
                            string queryRelIntegracaoTKC = $@"SELECT STATUS,TIME_READ ,TIME_WRITE, LATITUDE, LONGITUDE FROM GOTO_ENGEMIX.AVL_COMMAND_HISTORY 
                                                            WHERE ID_VIATURA = (SELECT id FROM GOTO_ENGEMIX.AVL_VIATURA WHERE placa = 'CB{CodigoCB_AnaliseINT}') 
                                                            AND TIME_READ <= TO_DATE('{dataTKC_AnaliseINT}/2020 23:59:59', 'dd/MM/yyyy HH24:mi:ss')
                                                            AND TIME_READ >= TO_DATE('{dataTKC_AnaliseINT}/2020 00:00:00', 'dd/MM/yyyy HH24:mi:ss')
                                                            AND TICKET_CODE = '{numTKC_AnaliseINT}'
                                                            ORDER BY TIME_READ ASC";
                            DataTable RelIntegracaoTKC = con.ReadDataTable(queryRelIntegracaoTKC);

                            string queryUltimaTransmissao = $"SELECT TIME_READ FROM GOTO_ENGEMIX.AVL_POSITION WHERE ID_VIATURA = (SELECT id FROM GOTO_ENGEMIX.AVL_VIATURA WHERE placa = 'CB{CodigoCB_AnaliseINT}')";
                            var UltimaTransmissao = con.ReadDataDateTime(queryUltimaTransmissao);

                            string queryRotaCriada = $@"SELECT ID, NAME, PERIOD_FROM, PERIOD_TO  FROM GOTO_ENGEMIX.GOTO_ROUTE 
                                                        WHERE ID_MONITORED_POINT = (SELECT id FROM GOTO_ENGEMIX.AVL_VIATURA WHERE placa = 'CB{CodigoCB_AnaliseINT}') 
                                                        AND PERIOD_FROM >= TO_DATE('{dataTKC_AnaliseINT}/2020 00:00:00', 'dd/MM/yyyy HH24:mi:ss')
                                                        AND PERIOD_TO <= TO_DATE('{dataTKC_AnaliseINT}/2020 23:59:59', 'dd/MM/yyyy HH24:mi:ss')
                                                        AND STATUS = 'A' AND ID_CLIENT =134";
                            DataTable RotaCriada = con.ReadDataTable(queryRotaCriada);

                            string queryDadosVeiculo = $@"SELECT PLACA, ID_CLIENTE, STATUS FROM GOTO_ENGEMIX.AVL_VIATURA WHERE placa = 'CB{CodigoCB_AnaliseINT}'";
                            DataTable DadosVeiculo = con.ReadDataTable(queryDadosVeiculo);

                            object VeiculoAtivo = null;
                            try { VeiculoAtivo = DadosVeiculo.Select("STATUS = 'A'"); } catch { }

                            
                            string queryAtraso = $@"SELECT count(1) FROM GOTO_ENGEMIX.AVL_POSITION_HISTORY_{TempoFimTKC.ToString("yyyyMM")} 
                                                    WHERE ID_VIATURA = (SELECT id FROM GOTO_ENGEMIX.AVL_VIATURA WHERE placa = 'CB{CodigoCB_AnaliseINT}') 
                                                    AND TIME_READ >= TO_DATE('{TempoInicioTKC}', 'dd/MM/yyyy HH24:mi:ss')
                                                    AND TIME_READ <= TO_DATE('{TempoFimTKC}', 'dd/MM/yyyy HH24:mi:ss')
                                                    AND (TIME_WRITE - TIME_READ) > 0.02";
                            int Atraso = con.ReadDataInt(queryAtraso);

                            string queryTransmissao_para_Tiquete = $@"SELECT count(1) FROM GOTO_ENGEMIX.AVL_POSITION_HISTORY_{TempoFimTKC.ToString("yyyyMM")} 
                                                    WHERE ID_VIATURA = (SELECT id FROM GOTO_ENGEMIX.AVL_VIATURA WHERE placa = 'CB{CodigoCB_AnaliseINT}') 
                                                    AND TIME_READ >= TO_DATE('{TempoInicioTKC}', 'dd/MM/yyyy HH24:mi:ss')
                                                    AND TIME_READ <= TO_DATE('{TempoFimTKC}', 'dd/MM/yyyy HH24:mi:ss')
                                                    AND IGNITION = 1";
                            int Transmissao_para_Tiquete = con.ReadDataInt(queryTransmissao_para_Tiquete);

                            string queryVelocidade_para_Tiquete = $@"SELECT count(1) FROM GOTO_ENGEMIX.AVL_POSITION_HISTORY_{TempoFimTKC.ToString("yyyyMM")} 
                                                    WHERE ID_VIATURA = (SELECT id FROM GOTO_ENGEMIX.AVL_VIATURA WHERE placa = 'CB{CodigoCB_AnaliseINT}') 
                                                    AND TIME_READ >= TO_DATE('{TempoInicioTKC}', 'dd/MM/yyyy HH24:mi:ss')
                                                    AND TIME_READ <= TO_DATE('{TempoFimTKC}', 'dd/MM/yyyy HH24:mi:ss')
                                                    AND IGNITION = 1 AND (VELOCIDADE > 10 OR VELOCIDADE_REAL > 10)";
                            int Velocidade_para_Tiquete = con.ReadDataInt(queryVelocidade_para_Tiquete);

                            


                            List<string> ListaStatus = new List<string>();
                            foreach (DataRow rw in RelIntegracaoTKC.Rows)
                            {
                                ListaStatus.Add(rw["STATUS"].ToString());
                            }

                            string queryTimeReadTJB = $@"SELECT TIME_READ FROM GOTO_ENGEMIX.AVL_COMMAND_HISTORY
                                                            WHERE ID_VIATURA = (SELECT id FROM GOTO_ENGEMIX.AVL_VIATURA WHERE placa = 'CB{CodigoCB_AnaliseINT}') 
                                                            AND TIME_READ <= TO_DATE('{dataTKC_AnaliseINT}/2020 23:59:59', 'dd/MM/yyyy HH24:mi:ss')
                                                            AND TIME_READ >= TO_DATE('{dataTKC_AnaliseINT}/2020 00:00:00', 'dd/MM/yyyy HH24:mi:ss')
                                                            AND TICKET_CODE = '{numTKC_AnaliseINT}'
                                                            AND STATUS = 'TJB'
                                                            ORDER BY TIME_READ ASC";
                            string queryTimeReadAJB = $@"SELECT TIME_READ FROM GOTO_ENGEMIX.AVL_COMMAND_HISTORY
                                                            WHERE ID_VIATURA = (SELECT id FROM GOTO_ENGEMIX.AVL_VIATURA WHERE placa = 'CB{CodigoCB_AnaliseINT}') 
                                                            AND TIME_READ <= TO_DATE('{dataTKC_AnaliseINT}/2020 23:59:59', 'dd/MM/yyyy HH24:mi:ss')
                                                            AND TIME_READ >= TO_DATE('{dataTKC_AnaliseINT}/2020 00:00:00', 'dd/MM/yyyy HH24:mi:ss')
                                                            AND TICKET_CODE = '{numTKC_AnaliseINT}'
                                                            AND STATUS = 'AJB'
                                                            ORDER BY TIME_READ ASC";
                            DateTime TimeReadTJB = con.ReadDataDateTime(queryTimeReadTJB);
                            DateTime TimeReadAJB = con.ReadDataDateTime(queryTimeReadAJB);

                            string queryUltimaDescarga = $@"SELECT max(cmd.DATA_CREATE)
                                                            FROM GOTO_ENGEMIX.AVL_STATUS_COMMAND cmd 
                                                            INNER JOIN GOTO_ENGEMIX.avl_viatura av ON cmd.ID_VIATURA = av.id 
                                                            WHERE cmd.STATUS = 6
                                                            AND av.placa  = 'CB{CodigoCB_AnaliseINT}'
                                                            GROUP BY av.PLACA";
                            DateTime UltimaDescarga = con.ReadDataDateTime(queryUltimaDescarga);
                            string queryLATLONGJOB = $@"SELECT LATITUDE, LONGITUDE FROM GOTO_ENGEMIX.AVL_COMMAND_HISTORY
                                                            WHERE ID_VIATURA = (SELECT id FROM GOTO_ENGEMIX.AVL_VIATURA WHERE placa = 'CB{CodigoCB_AnaliseINT}') 
                                                            AND TIME_READ <= TO_DATE('{dataTKC_AnaliseINT}/2020 23:59:59', 'dd/MM/yyyy HH24:mi:ss')
                                                            AND TIME_READ >= TO_DATE('{dataTKC_AnaliseINT}/2020 00:00:00', 'dd/MM/yyyy HH24:mi:ss')
                                                            AND TICKET_CODE = '{numTKC_AnaliseINT}'
                                                            AND STATUS = 'AJB'";
                            var LATLONGJOB = con.ReadDataCollum_to_List(queryLATLONGJOB);

                            var x = ((LATLONGJOB[0]).ToString().Replace("," , "."));
                            var y = ((LATLONGJOB[1]).ToString().Replace(",", "."));
                           

                            var queryProximidadeObra = $@"SELECT count(1) FROM ( SELECT aph.id, av.placa, aph.time_read data, aph.out_area, NVL(rua, '') rua,
                                                        NVL(numero, '') numero, NVL(bairro, '') bairro, NVL(municipio, '') municipio, 
                                                        ROUND(GOTO_ENGEMIX.avl_calc_dist_coord({((LATLONGJOB[0]).ToString().Replace(",", "."))}, {((LATLONGJOB[1]).ToString().Replace(",", "."))}, aph.pos_y, aph.pos_x), 2) AS distancia, 
                                                        aph.pos_y latitude, aph.pos_x longitude, aph.pos_y LAT, aph.pos_x LON
                                                        FROM (SELECT * FROM GOTO_ENGEMIX.AVL_POSITION_HISTORY_{TempoFimTKC.ToString("yyyyMM")}) aph
                                                        JOIN GOTO_ENGEMIX.avl_viatura av ON av.id = aph.id_viatura
                                                        WHERE
                                                        aph.time_read BETWEEN TO_DATE ('{TempoInicioTKC}', 'dd/MM/yyyy HH24:mi:ss') AND TO_DATE ('{TempoFimTKC}', 'dd/MM/yyyy HH24:mi:ss')
                                                        AND av.placa = 'CB{CodigoCB_AnaliseINT}' AND aph.gps  = 1 AND av.status='A')
                                                        WHERE distancia <= (SELECT RAY/1000 FROM GOTO_ENGEMIX.AVL_STATUS_COMMAND WHERE TICKET_CODE ='{numTKC_AnaliseINT}' AND STATUS = 1)";
                            int ProximidadeObra = con.ReadDataInt(queryProximidadeObra);

                            #endregion





                            #region Encadeamento de análise
                            {
                                if (RelIntegracaoTKC.Rows.Count == 0) { sheet.Cells[row, ColunaStatus].Value = "Tíquete não Recebido"; }
                                else if (UltimaTransmissao == null) { sheet.Cells[row, ColunaStatus].Value = "Não Transmitiu"; }
                                else if (Transmissao_para_Tiquete == 0 ) { sheet.Cells[row, ColunaStatus].Value = "Não Transmitiu para o Tíquete"; }
                                else if (RotaCriada.Rows.Count == 0 || RotaCriada == null) { sheet.Cells[row, ColunaStatus].Value = "Rota não Encontrada - Regra Aplicação - Cadastro Tivit";}
                                else if (VeiculoAtivo == null) { sheet.Cells[row, ColunaStatus].Value = "Veículo Desativado"; }
                                else if (Atraso > 3) { sheet.Cells[row, ColunaStatus].Value = "Atraso Transmissão"; }
                                else if (ListaStatus.Contains("TJB") && ListaStatus.Contains("AJB") && ListaStatus.Contains("POU") && ListaStatus.Contains("TPL") && ListaStatus.Contains("WSH") && ListaStatus.Contains("IYD")) { sheet.Cells[row, ColunaStatus].Value = "Command Não Consumiu os Status"; }
                                else if (TimeReadAJB < TimeReadTJB) { sheet.Cells[row, ColunaStatus].Value = "Ordenação AJB/TJB"; }
                                else if (UltimaDescarga.AddDays(3) < DateTime.Now) { sheet.Cells[row, ColunaStatus].Value = "Não detectando descarga - Verificar Equipamento"; }
                                else if ((TempoFimTKC - TempoInicioTKC) < new TimeSpan(0, 20, 0)) { sheet.Cells[row, ColunaStatus].Value = "Não saiu da Base - Tíquete fechado com < 20min"; }
                                else if (ListaStatus.Contains("TKC")  && ListaStatus.Count == 1 && Velocidade_para_Tiquete < 2) { sheet.Cells[row, ColunaStatus].Value = "Não Saiu da Base (Somente TKC, Pouco Registro de Velocidade)"; }
                                else if (!ListaStatus.Contains("POU") && ListaStatus.Contains("TJB") && ListaStatus.Contains("AJB")) { sheet.Cells[row, ColunaStatus].Value = "Não detectou descarga para o tíquete"; }
                                else if (ListaStatus.Contains("TKC_PRÉ") && ListaStatus.Contains("TJB") && ListaStatus.Contains("AJB") && ListaStatus.Contains("POU")) { sheet.Cells[row, ColunaStatus].Value = "Pré-Tíquete"; }
                                else if (ListaStatus.Contains("TKC_PRÉ") && ListaStatus.Contains("TJB") && ListaStatus.Contains("AJB") && ListaStatus.Contains("POU") && ListaStatus.Contains("TPL")) { sheet.Cells[row, ColunaStatus].Value = "Pré-Tíquete após TPL - Regra Aplicação"; }
                                else if (LATLONGJOB[0].ToString() == "" || LATLONGJOB[0].ToString() == "0" || LATLONGJOB[1].ToString() == "" || LATLONGJOB[1].ToString() == "0") { sheet.Cells[row, ColunaStatus].Value = "Coordenadas da Obra não definidas"; }
                                else if (ProximidadeObra == 0) { sheet.Cells[row, ColunaStatus].Value = "Veículo não foi para Obra (Local do TKC)"; }

                            }
                            #endregion

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
