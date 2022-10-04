namespace EmailRecorCobro
{
    partial class EmailCobro
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.EjecutarCada = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.EjecutarCada)).BeginInit();
            // 
            // EjecutarCada
            // 
            this.EjecutarCada.Enabled = true;
            this.EjecutarCada.Interval = 60000D;
            this.EjecutarCada.Elapsed += new System.Timers.ElapsedEventHandler(this.Timer1_Elapsed);
            // 
            // EmailCobro
            // 
            this.ServiceName = "EnvioMasivoEmailCobros";
            ((System.ComponentModel.ISupportInitialize)(this.EjecutarCada)).EndInit();

        }

        #endregion

        private System.Timers.Timer EjecutarCada;
    }
}
