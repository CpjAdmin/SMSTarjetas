using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Timers;

namespace SMSCobros
{
    public partial class SmsCobros : ServiceBase
    {
        //Declaración de variables

        string cadena = "";
        bool ejecutarEnvio;
        string idReferencia = string.Empty;
        string dicBitacora = string.Empty;
        string arcBitacora = string.Empty;
        string usuarioSMS = string.Empty;
        string pwdSMS = string.Empty;
        string rutaArchivo = string.Empty;
        string htmlPlanitlla = string.Empty;
        string modulos;
        System.Timers.Timer ejecutarCada = new System.Timers.Timer();


        string formatoFecha(string p1)
        {
            if (p1.Length == 1)
            {
                p1 = p1.PadLeft(2, '0');
            }
            return p1;
        }

        void actualizarRegistro(string codModulo, int pNum_Mesaje, wsSMSC.Message objMensaje, wsSMSC.SMSResponse objRespuesta)
        {
            string msgError = string.Empty;
            char Rs = ' ';
            try
            {

                using (OracleConnection conn = new OracleConnection(cadena))
                {
                    Rs = objRespuesta.status == "Success" ? 'A' : 'E';
                    msgError = objRespuesta.errordes.Length <= 0 ? "" : objRespuesta.errordes;
                    string parametroAuxiliar = pNum_Mesaje.ToString() + "-" + (Rs == 'E' ? "3" : "2") + "-" + codModulo.ToUpper().Trim();
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "DB_UTILIDADES.PKG_INTEGRACIONES_PROD.PROC_GESTIONAR_MENSAJES_SMS";
                    cmd.CommandTimeout = 0;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new OracleParameter("cod_Proceso", "02"));
                    cmd.Parameters.Add(new OracleParameter("parametroAuxiliar", parametroAuxiliar));
                    cmd.Parameters.Add(new OracleParameter("ref_MensajesSMS", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();

                }
            }
            catch (Exception ex)
            {
                msgError += "->" + ex.Message;
                registrarBitacoraEventos("Error en actualizarRegistro() ->Num_Mensaje:" + pNum_Mesaje.ToString() + "->" + ex.Message);

            }
            finally
            {
                using (CoopeBankEntities contextoBD = new CoopeBankEntities())
                {
                    CL_MENSAJES_SMS_BIT objRegistro = new CL_MENSAJES_SMS_BIT();
                    objRegistro.NUM_MENSAJE = Convert.ToDecimal(pNum_Mesaje);
                    objRegistro.MENSAJE = objMensaje.MessageMember;
                    objRegistro.NUM_TELEFONO = objMensaje.mobile;
                    objRegistro.FEC_REGISTRO = DateTime.Now;
                    objRegistro.COD_MODULO = codModulo;
                    objRegistro.DETALLE = msgError;
                    objRegistro.IND_ESTADO = (Rs == 'E' ? "3" : "2");
                    contextoBD.CL_MENSAJES_SMS_BIT.Add(objRegistro);
                    contextoBD.SaveChanges();
                }
            }
        }


        void enviarMensajeSMS(MensajeSMS pMensaje, int pNum_Mensaje, string codModulo)
        {
            try
            {
                using (wsSMSC.WSMSGSoapClient objRequest = new wsSMSC.WSMSGSoapClient())
                {
                    wsSMSC.SMSResponse objRespuesta;
                    wsSMSC.Message objMensaje = new wsSMSC.Message();
                    objMensaje.countrycode = "506";
                    objMensaje.mobile = pMensaje.NUM_TELEFONO;
                    objMensaje.MessageMember = pMensaje.MENSAJE;
                    if (objMensaje.mobile.Length <= 0 || objMensaje.mobile == null)
                    {
                        objRespuesta = new wsSMSC.SMSResponse();
                        objRespuesta.status = "Error";
                        objRespuesta.errordes = "No se cuenta con numero telefonico que notificar SMS";
                        actualizarRegistro(codModulo, pNum_Mensaje, objMensaje, objRespuesta);
                        return;
                    }
                    objRespuesta = objRequest.SendMessage(usuarioSMS, pwdSMS, objMensaje);
                    actualizarRegistro(codModulo, pNum_Mensaje, objMensaje, objRespuesta);
                }
            }
            catch (Exception ex)
            {

                registrarBitacoraEventos("Error en enviarMensajeSMS() -> NUM_MENSAJE: " + pNum_Mensaje.ToString() + " -> " + ex.Message);
            }
        }


        void obtenerMesanjesSMSEnviar()
        {
            string numMensaje = "";

            try
            {

                //Se determinan los modulos de los mesajes que se obtendran de la tabla CLIENTES.CL_MENSAJES_SMS
                modulos = ConfigurationManager.AppSettings["Modulos"].ToString();
                //string[] listaModulos = modulos.Split('-');                
                OracleDataReader dr;

                using (OracleConnection conn = new OracleConnection(cadena))
                {
                    OracleCommand cmd = new OracleCommand();
                    cmd.CommandText = "DB_UTILIDADES.PKG_INTEGRACIONES_PROD.PROC_GESTIONAR_MENSAJES_SMS";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new OracleParameter("cod_Proceso", "01"));
                    cmd.Parameters.Add(new OracleParameter("parametroAuxiliar", modulos));
                    cmd.Parameters.Add(new OracleParameter("ref_MensajesSMS", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));
                    conn.Open();

                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        MensajeSMS objMsj = new MensajeSMS();
                        numMensaje = dr.GetInt32(0).ToString();
                        objMsj.COUNTRYCODE = "506";
                        objMsj.NUM_TELEFONO = dr.GetString(1);
                        objMsj.MENSAJE = dr.GetString(2);
                        objMsj.COD_MODULO = dr.GetString(5);
                        objMsj.NOM_CLIENTE = dr.GetString(6);
                        objMsj.toEmail = dr.GetValue(7).ToString();
                        enviarMensajeSMS(objMsj, dr.GetInt32(0), dr.GetString(5));
                    }

                    dr.Close();
                    dr.Dispose();
                    cmd.Dispose();
                    conn.Close();
                    conn.Dispose();
                    ejecutarEnvio = true; //Se activa nuevamente para que cada Tick se pueda ejecutar
                }
            }
            catch (Exception ex)
            {
                ejecutarEnvio = true;
                registrarBitacoraEventos("Error en obtenerMesanjesSMSEnviar -> NUM_MENSAJE: " + numMensaje + " -> " + ex.Message);
            }

        }



        public SmsCobros()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //Se inicializa el objecto EventLog
            WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + " - " + "Servicio Iniciado: : " + DateTime.Now);

            modulos = "";
            cadena = ConfigurationManager.ConnectionStrings["OracleString"].ConnectionString;
            usuarioSMS = ConfigurationManager.AppSettings["usuarioSMS"].ToString();
            pwdSMS = ConfigurationManager.AppSettings["pwdSMS"].ToString();
            ejecutarCada.Elapsed += new ElapsedEventHandler(EjecutarCada_Elapsed);
            ejecutarCada.Enabled = true;
            ejecutarCada.AutoReset = true;
            ejecutarCada.Interval = Convert.ToDouble(ConfigurationManager.AppSettings["CantMilisegundos"].ToString());
           
            ejecutarEnvio = true;

            //Se inicia con la ejecución de la funcionalidad
            ejecutarCada.Start();
        }

        public void onDebug()
        {
            OnStart(null);
        }

        protected override void OnStop()
        {
            WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + " - " + "Servicio detenido: : " + DateTime.Now);
        }

        private void EjecutarCada_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (ejecutarEnvio)
                {
                    ejecutarEnvio = false;
                    obtenerMesanjesSMSEnviar();
                }
            }
            catch (Exception ex)
            {
                registrarBitacoraEventos("Error en EjecutarCada_Elapsed ->" + ex.Message);
            }



        }


        void registrarBitacoraEventos(string regMensaje)
        {
            WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + " - " + regMensaje);

        }

        public void WriteToFile(string Message)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\SMSServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(Message);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(Message);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void ejecutarCada_Tick(object sender, EventArgs e)
        {

        }
    }
}
