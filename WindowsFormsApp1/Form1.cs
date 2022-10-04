using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Diagnostics;
using System.Net.Mail;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        EventLog myLog;
        OracleCommand cmd;
        OracleConnection conn;
        OracleDataReader dr;
        string cadena = ConfigurationManager.ConnectionStrings["OracleString"].ConnectionString;
        List<MensajeSMS> ListadoMensajesSMS;
        List<MensajeEmail> ListadoMensaejsEmail;
        int numero = 0;
        string dicBitacora = ConfigurationManager.AppSettings["carpetaBitacora"].ToString();
        string arcBitacora = ConfigurationManager.AppSettings["fileBitacora"].ToString();
        string htmlPlanitlla = string.Empty;
        string modulos;
        public Form1()
        {
            InitializeComponent();
            //Obtener parametro de plantilla HTML para notificación de correo electronico
            using (CoopeBankEntities bdContexto = new CoopeBankEntities())
            {
                htmlPlanitlla = bdContexto.Parametros.Where(x => x.IdKey.Equals("htmlSms")).FirstOrDefault().Valor;
               
            }
           
        }

        void registrarBitacoraEventos(string regMensaje)
        {
            try
            {
                myLog = new EventLog();
                myLog.Source = "BitSMSTarjetas";
                if (!EventLog.SourceExists("BitSMSTarjetas"))
                {
                    EventLog.CreateEventSource("BitSMSTarjetas", "LogSMSTarjetas");

                }
                else
                {
                    myLog.WriteEntry(regMensaje);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }

        }

        void actualizarRegistro()
        {
            try
            {

                conn = new OracleConnection();
                conn.ConnectionString = cadena;
                cmd = new OracleCommand();
                //cmd.CommandText = "Update CL_MENSAJES_SMS " +
                //                  "set IND_ESTADO = " + (Rs == 'E' ? "'3'" : "'2'") +
                //                  "Where NUM_MENSAJE = " + pNum_Mesaje.ToString();
                //cmd.CommandType = CommandType.Text;
                int pNum_Mesaje = 9197;
                string resultado = "Success";
                char Rs = resultado == "Success" ? 'A' : 'E';
                string parametroAuxiliar = pNum_Mesaje.ToString() + "-" + (Rs == 'E' ? "3" : "2");
                cmd.CommandText = "DB_UTILIDADES.PKG_INTEGRACIONES_PROD.PROC_GESTIONAR_MENSAJES_SMS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new OracleParameter("cod_Proceso", "02"));
                cmd.Parameters.Add(new OracleParameter("parametroAuxiliar", parametroAuxiliar));
                cmd.Parameters.Add(new OracleParameter("ref_MensajesSMS", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));


                cmd.Connection = conn;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
                cmd.Dispose();

            }
            catch (Exception ex)
            {



            }
        }

        void obtenerMesanjesSMSEnviar()
        {
            try
            {
                modulos = ConfigurationManager.AppSettings["Modulos"].ToString();
                //string[] listaModulos = modulos.Split('-');                

                conn = new OracleConnection();
                conn.ConnectionString = cadena;
                cmd = new OracleCommand();
                cmd.CommandText = "DB_UTILIDADES.PKG_INTEGRACIONES_PROD.PROC_GESTIONAR_MENSAJES_SMS";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                cmd.CommandTimeout = 0;
                cmd.Parameters.Add(new OracleParameter("cod_Proceso", "01"));
                cmd.Parameters.Add(new OracleParameter("parametroAuxiliar", modulos));
                cmd.Parameters.Add(new OracleParameter("ref_MensajesSMS", OracleDbType.RefCursor, DBNull.Value, ParameterDirection.Output));
                conn.Open();
                //DataTable dr = (DataTable)cmd.ExecuteReader();
                OracleDataReader dr = cmd.ExecuteReader();
                ListadoMensajesSMS = new List<MensajeSMS>();
                //DataTable dt = new DataTable();
                //dt.Load(dr);

                while (dr.Read())
                {
                    object x = dr[1];

                    MensajeSMS objMsj = new MensajeSMS();

                    objMsj.COUNTRYCODE = "506";
                    objMsj.NUM_TELEFONO = dr.GetString(1);
                    objMsj.MENSAJE = dr.GetString(2);
                    objMsj.COD_MODULO = dr.GetString(5);
                    objMsj.NOM_CLIENTE = dr.GetString(6);
                    //objMsj.toEmail = Convert.ToString(dr.GetString(7));
                    //objMsj.toEmail = dr["EMAIL"].ToString();
                    //Console.WriteLine(dr.GetValue(7));
                    objMsj.toEmail = dr.GetValue(7).ToString();
                    //objMsj.toEmail = dr.GetString(7);



                    //Console.WriteLine(dr.GetString(7)); --> ese mae viene NULL desde BD Oracle cambiar SELECT
                    //objMsj.toEmail = "fsfs";
                    //objMsj.toEmail = dr.GetOracleValue(7) == null  ? "" : dr.GetString(7);
                    ListadoMensajesSMS.Add(objMsj);
                }
                conn.Close();
                conn.Dispose();
                cmd.Dispose();
                dataGridView1.DataSource = ListadoMensajesSMS;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                //registrarBitacoraEventos(ex.Message);
            }




        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //obtenerMesanjesSMSEnviar();
            obtenerMesanjesEmailEnviar();

        }

        string formatoFecha(string p1)
        {
            if (p1.Length == 1)
            {
                p1 = p1.PadLeft(2, '0');
            }
            return p1;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            obtenerMesanjesSMSEnviar();
            return;


            //for (int i = 0; i < 100000000; i++)
            //{
            //    Random rand = new Random(Guid.NewGuid().GetHashCode());
            //    string referencia = rand.Next().ToString();
            //    if (referencia.Length >= 8)
            //    {
            //        referencia = referencia.Substring(0, 7);
            //    }
            //    else
            //    {
            //        referencia.PadRight((8 - referencia.Length), '0');
            //    }

            //}

            //< add key = "pwdSMS" value = "3c9700985a792287fb6915ba65994df2" />
            //MessageBox.Show(referencia);
            //serviAseSMS.SendMessageRequest objMsg = new serviAseSMS.SendMessageRequest();
            //objMsg.Body.username = "ccnotificaciones";
            //objMsg.Body.password = "3c9700985a792287fb6915ba65994df2";
            //serviAseSMS.Message objMensaje = new serviAseSMS.Message();
            //objMensaje.countrycode = "506";
            //objMensaje.idreference = "5746";
            //objMensaje.mobile = "71115299";
            //objMensaje.MessageMember = "Prueba Christopher 1234";            
            //serviAseSMS.WSMSGSoapClient ojb = new serviAseSMS.WSMSGSoapClient();
            //ojb.SendMessage("ccnotificaciones", "3c9700985a792287fb6915ba65994df2", objMensaje);

            using (serviAseSMS.WSMSGSoapClient objRequest = new serviAseSMS.WSMSGSoapClient())
            {
                serviAseSMS.SMSResponse objRespuesta;
                serviAseSMS.Message objMensaje = new serviAseSMS.Message();
                objMensaje.countrycode = "506";
                objMensaje.idreference = "5746";
                objMensaje.mobile = "71115299";
                objMensaje.MessageMember = "Prueba Christopher 19 junio";
                objRespuesta = objRequest.SendMessage("ccnotificaciones", "3c9700985a792287fb6915ba65994df2", objMensaje);

                Console.WriteLine(objRespuesta.status);


            }

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (serviAseSMS.WSMSGSoapClient objRequest = new serviAseSMS.WSMSGSoapClient())
                {

                    serviAseSMS.SMSResponse objRespuesta;
                    serviAseSMS.Message objMensaje = new serviAseSMS.Message();
                    objMensaje.countrycode = "506";
                    objMensaje.mobile = "71115299";
                    objMensaje.MessageMember = "Prueba";

                    objRespuesta = objRequest.SendMessage("ccnotificaciones", "3c9700985a792287fb6915ba65994df2", objMensaje);

                    using (SqlConnection conn = new SqlConnection("Data Source=RAGNAR;Initial Catalog=CoopeBank;User ID=CoopeBank;password=C00p38Ank*8D;"))
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "INSERT INTO[dbo].[CL_MENSAJES_SMS_BIT] " +
                                          "(NUM_MENSAJE,NUM_TELEFONO,MENSAJE,FEC_REGISTRO,IND_ESTADO,COD_MODULO,DETALLE) Values " +
                                          "(666666,'71115299','pruebaCar',GetDate(),8,'CB','Prueba')";
                                                                      
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        
                    }

                   
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
          
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            enviarCorreo();
        }
        private void enviarCorreo()
        {

            try
            {


                using (SqlConnection conn = new SqlConnection("Data Source=Cefeo;Initial Catalog=CoopeBank;Integrated Security=True"))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Select * from Parametros";
                    SqlDataReader dr;
                    conn.Open();
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        htmlPlanitlla = dr.GetString(3);
                    }
                }

                string to = "gonzalezcarlos7684@gmail.com"; //Correo del asociado a notificar
                string from = "notificaciones@coopecaja.fi.cr";
                MailMessage message = new MailMessage(from, to);
                message.Subject = "Gestión de cobros Coopecaja";
                message.IsBodyHtml = true;
                string[] valoresPlantilla = new string[2];
                valoresPlantilla[0] = "Carlos Gonzalez Romero";
                valoresPlantilla[1] = "COOPECAJA R.L le informa que sus créditos muestran atrasos. Favor reportar su pago al correo   pagos@coopecaja.fi.cr o al teléfono 25421000.. Por favor omitir el mensaje si efectuó el pago.";
                message.Body = String.Format(htmlPlanitlla, valoresPlantilla);
                SmtpClient client = new SmtpClient();
                client.Host = "172.28.39.118";//"lana.coopecaja.local"; 172.28.39.118
                client.Port = 25;
                client.Credentials = new System.Net.NetworkCredential("notificaciones@coopecaja.fi.cr", "qj@C8dc7EK");
                client.Send(message);


            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }

        }

        #region "Notificacion Email"
        void obtenerMesanjesEmailEnviar()
        {
            try
            {

                //Se determinan los modulos de los mesajes que se obtendran de la tabla CLIENTES.CL_MENSAJES_SMS
                modulos = ConfigurationManager.AppSettings["Modulos"].ToString();
                //string[] listaModulos = modulos.Split('-');                

                conn = new OracleConnection();
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
                ListadoMensaejsEmail = new List<MensajeEmail>();
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
                    ListadoMensaejsEmail.Add(oMensajeEmail);
                }


                conn.Close();
                conn.Dispose();
                cmd.Dispose();
                dataGridView1.DataSource = ListadoMensaejsEmail;


            }
            catch (Exception ex)
            {
                registrarBitacoraEventos("Error en obtenerMesanjesSMSEnviar -> NUM_MENSAJE: " + dr.GetInt32(0).ToString() + " -> " + ex.Message);
            }

        }
        #endregion

        private void Button4_Click(object sender, EventArgs e)
        {
            obtenerMesanjesEmailEnviar();
        }


        void notificacionCorreo(MensajeEmail pMensajeEmail)
        {
            try
            {
                string to = pMensajeEmail.DESTINO; //Correo del asociado a notificar                
                string from = "notificaciones@coopecaja.fi.cr";
                MailMessage message = new MailMessage(from, to);
                message.Subject = "Gestión de cobros Coopecaja";
                message.IsBodyHtml = true;
                string[] valoresPlantilla = new string[2];
                valoresPlantilla[0] = pMensajeEmail.NOM_CLIENTE;
                //valoresPlantilla[1] = pMensajeEmail.TEXTO.Replace("&lt;br&gt;", "</br>").Replace("&lt;br&gt;.", "</br>");
                valoresPlantilla[1] = pMensajeEmail.TEXTO;
                message.Body = String.Format(htmlPlanitlla, valoresPlantilla);
                SmtpClient client = new SmtpClient();
                client.Host = "172.28.39.118";//"lana.coopecaja.local"; 172.28.39.118
                client.Port = 25;
                client.Credentials = new System.Net.NetworkCredential("notificaciones@coopecaja.fi.cr", "qj@C8dc7EK");
                client.Send(message);
            }
            catch (Exception ex)
            {

                //registrarBitacoraEventos("Error en notificacionCorreo() -> NUM_MENSAJE: " + pNum_Mensaje.ToString() + " -> " + ex.Message);
            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            using (CoopeBankEntities bdContexto = new CoopeBankEntities())
            {
                htmlPlanitlla = bdContexto.Parametros.Where(x => x.IdKey.Equals("htmlSms")).FirstOrDefault().Valor;

            }


            MensajeEmail oMensajeEmail = new MensajeEmail();
            oMensajeEmail.NUM_MENSAJE = 100;
            oMensajeEmail.ORIGEN = "sdfasdf";
            oMensajeEmail.DESTINO = "cgonzalez@coopecaja.fi.cr";
            oMensajeEmail.ASUNTO = "Coopecaja le informa del atraso de  su crédito:";
            oMensajeEmail.TEXTO = "Señor(a) Rueda Vindas Paula Liliana</br> </br> Estimado Asociado (a):</br></br>Un cordial saludo.</br></br>Coopecaja le informa que según nuestros registros de crédito su operación presenta dos cuotas de atraso, le solicitamos cancelar lo antes posible, para evitarle inconvenientes. En caso contrario nos obliga aplicarle un cobro administrativo al fiador(a), si la operación cuenta con garantía fiduciaria.</br></br>Estar al día le permite mantener un buen récord crediticio en la Cooperativa y en el Sistema Financiero Nacional.</br></br>Favor comuníquese al Departamento de Cobros al 2542-1000. Si ya canceló, por favor omitir este aviso.";
            oMensajeEmail.FEC_MENSAJE = DateTime.Now;
            oMensajeEmail.ESTADO = "1";
            oMensajeEmail.COD_MODULO = "CB";
            oMensajeEmail.COD_CLIENTE = 27132;
            oMensajeEmail.NOM_CLIENTE = "Carlos Gonzalez Romero";
            notificacionCorreo(oMensajeEmail);
        }

        private void Button6_Click(object sender, EventArgs e)
        {

            try
            {
                using (CoopeBankEntities contextoBD = new CoopeBankEntities())
                {
                    CL_MENSAJES_EMAIL_BIT objRegistro = new CL_MENSAJES_EMAIL_BIT();
                    objRegistro.NUM_MENSAJE = 100;
                    objRegistro.EMAIL = "cgonzalez@coopecaja.fi.cr";
                    objRegistro.MENSAJE = "hola mundo";
                    objRegistro.FEC_REGISTRO = DateTime.Now;
                    objRegistro.IND_ESTADO = "2";
                    objRegistro.COD_MODULO = "CB";
                    objRegistro.DETALLE = "dsfad";
                    contextoBD.CL_MENSAJES_EMAIL_BIT.Add(objRegistro);
                    contextoBD.SaveChanges();
                }

                MessageBox.Show("Lo registro");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
          
        }
    }
}


