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

namespace EmailRecorCobro
{
    public partial class EmailCobro : ServiceBase
    {
        //Declaración de variables
        OracleCommand cmd;
        OracleConnection conn;
        OracleDataReader dr;
        string cadena = "";
        bool ejecutarEnvio;
        string htmlPlanitlla = string.Empty;
        string modulos;


        public EmailCobro()
        {
            InitializeComponent();
        }

        public void onDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                WriteToFile("Servicio Iniciado: : " + DateTime.Now);
                modulos = "";
                EjecutarCada.Interval = Convert.ToDouble(ConfigurationManager.AppSettings["CantMilisegundos"].ToString());
                cadena = ConfigurationManager.ConnectionStrings["OracleString"].ConnectionString;
                
                ejecutarEnvio = true;

                //Obtener parametro de plantilla HTML para notificación de correo electronico
                using (CoopeBankEntities1 bdContexto = new CoopeBankEntities1())
                {
                    htmlPlanitlla = bdContexto.Parametros.Where(x => x.IdKey.Equals("htmlSms")).FirstOrDefault().Valor;
                }

                //Se inicia con la ejecución de la funcionalidad
                EjecutarCada.Start();
                
            }
            catch (Exception ex)
            {
                WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + " - " + "Error iniciando el servicio ->" + ex.Message);


            }
        }

        protected override void OnStop()
        {
            WriteToFile("Servicio Detenido: : " + DateTime.Now);
        }

        string formatoFecha(string p1)
        {
            if (p1.Length == 1)
            {
                p1 = p1.PadLeft(2, '0');
            }
            return p1;
        }


        //actualiza 
        void actualizarRegistro(MensajeEmail pMensajeEmail, string pEstado, string MensajeRegistro = "")
        {
            using (conn = new OracleConnection())
            {
                try
                {
                    string parametroAuxiliar = pMensajeEmail.NUM_MENSAJE.ToString() + "-" + pEstado + "-" + pMensajeEmail.COD_MODULO + "-" + "S";
                    conn.ConnectionString = cadena;
                    cmd = new OracleCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "DB_UTILIDADES.PKG_INTEGRACIONES_PROD.PROC_GESTIONAR_MENSAJES_SMS";
                    cmd.CommandTimeout = 0;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new OracleParameter("cod_Proceso", "02"));
                    cmd.Parameters.Add(new OracleParameter("parametroAuxiliar", parametroAuxiliar));
                    cmd.Parameters.Add(new OracleParameter("ref_MensajesSMS", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    //conn.Close();
                    //conn.Dispose();
                    cmd.Dispose();
                }
                catch (Exception ex)
                {
                    WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + " - " + "Error actualizarRegistro : " + ex.Message);

                }
                finally
                {
                    using (CoopeBankEntities1 contextoBD = new CoopeBankEntities1())
                    {
                        CL_MENSAJES_EMAIL_BIT objRegistro = new CL_MENSAJES_EMAIL_BIT();
                        objRegistro.NUM_MENSAJE = pMensajeEmail.NUM_MENSAJE;
                        objRegistro.EMAIL = pMensajeEmail.DESTINO;
                        objRegistro.MENSAJE = pMensajeEmail.TEXTO.Substring(1, 400) + "...";
                        objRegistro.FEC_REGISTRO = DateTime.Now;
                        objRegistro.IND_ESTADO = pEstado;
                        objRegistro.COD_MODULO = pMensajeEmail.ESTADO;
                        objRegistro.DETALLE = MensajeRegistro;
                        contextoBD.CL_MENSAJES_EMAIL_BIT.Add(objRegistro);
                        contextoBD.SaveChanges();
                    }
                }
            }
        }


        void notificacionCorreo(MensajeEmail pMensajeEmail)
        {
            try
            {
                string to = pMensajeEmail.DESTINO; //Correo del asociado a notificar                
                string from = ConfigurationManager.AppSettings["Correo"].ToString(); 
                MailMessage message = new MailMessage(from, to);
                message.Subject = "Gestión de cobros Coopecaja";
                message.IsBodyHtml = true;
                string[] valoresPlantilla = new string[2];
                valoresPlantilla[0] = pMensajeEmail.NOM_CLIENTE;
                valoresPlantilla[1] = pMensajeEmail.TEXTO.Replace("&lt;br&gt;", "</br>").Replace("&lt;br&gt;.", "</br>");
                message.Body = String.Format(htmlPlanitlla, valoresPlantilla);
                SmtpClient client = new SmtpClient();
                client.Host = ConfigurationManager.AppSettings["ServidorCorreo"].ToString();
                client.Port = Convert.ToInt32(ConfigurationManager.AppSettings["PuertoCorreo"].ToString()); 
                client.Send(message);

                actualizarRegistro(pMensajeEmail, "2", "Notificación de correo exitosa"); //Notificacion existosa
            }
            catch (Exception ex)
            {
                actualizarRegistro(pMensajeEmail, "3", "Error en notificacion mensaje [: " + pMensajeEmail.NUM_MENSAJE.ToString() + "] :" + ex.Message); //Notificacion fallida
                WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + " - " + "Error en notificacionCorreo() -> NUM_MENSAJE: " + pMensajeEmail.NUM_MENSAJE.ToString() + " -> " + ex.Message);
            }
        }

        void obtenerMesanjesEmailEnviar()
        {
            
            //ejecutarEnvio = true; //Se activa nuevamente para que cada Tick se pueda ejecutar
            using (conn = new OracleConnection())
            {
                try
                {
                    modulos = ConfigurationManager.AppSettings["Modulos"].ToString();
                    conn.ConnectionString = cadena;
                    cmd = new OracleCommand();
                    cmd.CommandText = "DB_UTILIDADES.PKG_INTEGRACIONES_PROD.PROC_GESTIONAR_MENSAJES_SMS";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new OracleParameter("cod_Proceso", "03"));
                    cmd.Parameters.Add(new OracleParameter("parametroAuxiliar", modulos));
                    cmd.Parameters.Add(new OracleParameter("ref_MensajesSMS", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));
                    conn.Open();

                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        MensajeEmail oMensajeEmail = new MensajeEmail();
                        oMensajeEmail.NUM_MENSAJE = dr.GetInt32(0);
                        oMensajeEmail.ORIGEN = dr.GetString(1);
                        oMensajeEmail.DESTINO = dr.GetString(2);
                        oMensajeEmail.ASUNTO = dr.GetString(3);
                        oMensajeEmail.TEXTO = dr.GetString(4);
                        oMensajeEmail.FEC_MENSAJE = dr.GetDateTime(5);
                        oMensajeEmail.ESTADO = dr.GetString(6);
                        oMensajeEmail.COD_MODULO = dr.GetString(7);
                        oMensajeEmail.COD_CLIENTE = dr.GetInt32(8);
                        oMensajeEmail.NOM_CLIENTE = dr.GetString(9);
                        notificacionCorreo(oMensajeEmail);

                    }
                    //conn.Close();
                    //conn.Dispose();
                    cmd.Dispose();
                    ejecutarEnvio = true; //Se activa nuevamente para que cada Tick se pueda ejecutar

                }
                catch (Exception ex)
                {
                    ejecutarEnvio = true;
                    WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + " - " + "Error en obtenerMesanjesEmailEnviar -> NUM_MENSAJE: " + dr.GetInt32(0).ToString() + " -> " + ex.Message + " -> " + ex.InnerException.Message);
                }
            }

        }

        private void Timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (ejecutarEnvio)
                {
                    ejecutarEnvio = false;
                    obtenerMesanjesEmailEnviar();
                }
            }
            catch (Exception ex)
            {
                WriteToFile(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + " - " + "Error en EjecutarCada_Elapsed ->" + ex.Message);
            }

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
                string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\EmailServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
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
    }
}
