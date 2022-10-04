namespace EmailNoti
{
    partial class EmailNoti
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
            this.ejecutarCada = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.ejecutarCada)).BeginInit();
            // 
            // ejecutarCada
            // 
            this.ejecutarCada.Enabled = true;
            this.ejecutarCada.Interval = 60000D;
            this.ejecutarCada.Elapsed += new System.Timers.ElapsedEventHandler(this.Timer1_Elapsed);
            // 
            // EmailNoti
            // 
            this.ServiceName = "Service1";
            ((System.ComponentModel.ISupportInitialize)(this.ejecutarCada)).EndInit();

        }

        #endregion

        private System.Timers.Timer ejecutarCada;
    }
}
