namespace EngemixAnaliseAutomaizador
{
    partial class Form1
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.painelsup = new System.Windows.Forms.Panel();
            this.textfilenameselect = new System.Windows.Forms.TextBox();
            this.btrinserir = new System.Windows.Forms.Button();
            this.btrprocessar = new System.Windows.Forms.Button();
            this.painelinferior = new System.Windows.Forms.Panel();
            this.textboxlog = new System.Windows.Forms.RichTextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.FluxoAutomatizador_Link = new System.Windows.Forms.LinkLabel();
            this.painelsup.SuspendLayout();
            this.painelinferior.SuspendLayout();
            this.SuspendLayout();
            // 
            // painelsup
            // 
            this.painelsup.Controls.Add(this.textfilenameselect);
            this.painelsup.Controls.Add(this.btrinserir);
            this.painelsup.Controls.Add(this.btrprocessar);
            this.painelsup.Location = new System.Drawing.Point(12, 12);
            this.painelsup.Name = "painelsup";
            this.painelsup.Size = new System.Drawing.Size(597, 42);
            this.painelsup.TabIndex = 0;
            // 
            // textfilenameselect
            // 
            this.textfilenameselect.Location = new System.Drawing.Point(3, 12);
            this.textfilenameselect.Name = "textfilenameselect";
            this.textfilenameselect.ReadOnly = true;
            this.textfilenameselect.Size = new System.Drawing.Size(350, 20);
            this.textfilenameselect.TabIndex = 1;
            // 
            // btrinserir
            // 
            this.btrinserir.Location = new System.Drawing.Point(359, 10);
            this.btrinserir.Name = "btrinserir";
            this.btrinserir.Size = new System.Drawing.Size(75, 23);
            this.btrinserir.TabIndex = 1;
            this.btrinserir.Text = "Inserir";
            this.btrinserir.UseVisualStyleBackColor = true;
            this.btrinserir.Click += new System.EventHandler(this.InserirArquivo);
            // 
            // btrprocessar
            // 
            this.btrprocessar.Location = new System.Drawing.Point(476, 3);
            this.btrprocessar.Name = "btrprocessar";
            this.btrprocessar.Size = new System.Drawing.Size(118, 36);
            this.btrprocessar.TabIndex = 0;
            this.btrprocessar.Text = "Processar";
            this.btrprocessar.UseVisualStyleBackColor = true;
            this.btrprocessar.Click += new System.EventHandler(this.ProcessarArquivo);
            // 
            // painelinferior
            // 
            this.painelinferior.Controls.Add(this.textboxlog);
            this.painelinferior.Location = new System.Drawing.Point(12, 76);
            this.painelinferior.Name = "painelinferior";
            this.painelinferior.Size = new System.Drawing.Size(597, 297);
            this.painelinferior.TabIndex = 1;
            // 
            // textboxlog
            // 
            this.textboxlog.Location = new System.Drawing.Point(3, 4);
            this.textboxlog.Name = "textboxlog";
            this.textboxlog.Size = new System.Drawing.Size(591, 291);
            this.textboxlog.TabIndex = 0;
            this.textboxlog.Text = "";
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // FluxoAutomatizador_Link
            // 
            this.FluxoAutomatizador_Link.AutoSize = true;
            this.FluxoAutomatizador_Link.Location = new System.Drawing.Point(12, 60);
            this.FluxoAutomatizador_Link.Name = "FluxoAutomatizador_Link";
            this.FluxoAutomatizador_Link.Size = new System.Drawing.Size(79, 13);
            this.FluxoAutomatizador_Link.TabIndex = 2;
            this.FluxoAutomatizador_Link.TabStop = true;
            this.FluxoAutomatizador_Link.Text = "Visualizar Fluxo";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(621, 385);
            this.Controls.Add(this.FluxoAutomatizador_Link);
            this.Controls.Add(this.painelinferior);
            this.Controls.Add(this.painelsup);
            this.Name = "Automatizador Análise Engemix";
            this.Text = "Análise Ofensores";
            this.painelsup.ResumeLayout(false);
            this.painelsup.PerformLayout();
            this.painelinferior.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel painelsup;
        private System.Windows.Forms.Panel painelinferior;
        private System.Windows.Forms.Button btrprocessar;
        private System.Windows.Forms.Button btrinserir;
        private System.Windows.Forms.RichTextBox textboxlog;
        private System.Windows.Forms.TextBox textfilenameselect;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.LinkLabel FluxoAutomatizador_Link;
    }
}

