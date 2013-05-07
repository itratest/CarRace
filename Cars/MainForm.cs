using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ShadowEngine;
using Tao.OpenGl;
using ShadowEngine.OpenGL;
using ShadowEngine.ContentLoading;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets; 

namespace CarRace
{
    public partial class MainForm : Form
    {
        uint hdc;
        int selectedCamara = 1;
        int count;
        Controller control = new Controller();
        int mostrado = 0;
        int moving;

        public MainForm()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
            //InitializationGame();
   
        }

        private void InitializationGame()
        {
            tmrPaint.Enabled = true;
            //identificador del lugar en donde voy a dibujar
            hdc = (uint)this.Handle;
            //toma el error que sucedio
            string error = "";
            //Comando de inicializacion de la ventana grafica
            OpenGLControl.OpenGLInit(ref hdc, this.Width, this.Height, ref error);

            //inicia la posicion de la camara asi como define en angulo de perspectiva,etc etc
            control.Camara.SetPerspective();
            if (error != "")
            {
                MessageBox.Show("Ocurrio un error al inicializar openGL");
                this.Close();
            }

            //Habilita las luces

            //float[] lightAmbient = { 0.15F, 0.15F, 0.15F, 0.0F };

            //Lighting.LightAmbient = lightAmbient; 

            Lighting.SetupLighting();  // encapsulado en el sahdow engine 

            ContentManager.SetTextureList("texturas\\");
            ContentManager.LoadTextures();
            ContentManager.SetModelList("modelos\\");
            ContentManager.LoadModels();
            control.CreateObjects();

            //Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);
        }

        public void UpdateLogic()
        {
         /*   if (moving == 1)
            {
                Gl.glTranslatef(0, 0, 0.35f);
            }
            else
                if (moving == -1)
                {
                    Gl.glTranslatef(0, 0, -0.35f);
                }*/

            if (Controller.FinishedRace != true)
            Gl.glTranslatef(0, 0, control.Cars[0].Speed-0.01F);
            count++;
            if (Controller.FinishedRace == true && mostrado == 0)
            {
                mostrado = 1;
                moving = 0;
                control.Cars[0].Speed = 0;
                MessageBox.Show("The winner was the: " + lblPrimero.Text);
            }

            if (count == 10)
            {
                if (Controller.StartedRace == true && mostrado == 0)
                {
                    int primero = control.GetFirstPlace();
                    float distanciaRecorrida = control.GetDistanceInMeters(primero);
                    lblDistancia.Text = Convert.ToString((int)distanciaRecorrida);
                    switch (primero)
                    {
                        case 0:
                            {
                                lblPrimero.Text = "Blue car";
                                lblPrimero.ForeColor = Color.Blue;
                                break;
                            }
                        case 1:
                            {
                                lblPrimero.Text = "Red car";
                                lblPrimero.ForeColor = Color.Red;
                                break;
                            }
                        case 2:
                            {
                                lblPrimero.Text = "Black car";
                                lblPrimero.ForeColor = Color.Black;
                                break;
                            }
                    }
                }
                count = 0;
            }
        }


        private void tmrPaint_Tick(object sender, EventArgs e)
        {
            UpdateLogic(); 
            // clean opengl to draw
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            //draws the entire scene
            control.DrawScene();
            //change buffers
            Winapi.SwapBuffers(hdc);
            //tell opengl to drop any operation he is doing and to prepare for a new frame
            Gl.glFlush(); 
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            selectedCamara--;
            if (selectedCamara == 0)
            {
                selectedCamara = 4;
            }
            lblCamara.Text = Convert.ToString(selectedCamara);
            control.Camara.SelectCamara(selectedCamara - 1);    
        }

        private void button2_Click(object sender, EventArgs e)
        {
            selectedCamara++;
            if (selectedCamara == 5)
            {
                selectedCamara = 1;
            }
            lblCamara.Text = Convert.ToString(selectedCamara);
            control.Camara.SelectCamara(selectedCamara-1);   
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Controller.StartedRace = true;   
        }

        private void btnReiniciar_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
                control.Cars[i].StopSpeed();
            selectedCamara = 1;
            lblCamara.Text = Convert.ToString(selectedCamara);
            control.Camara.SelectCamara(selectedCamara-1); 
         //   MessageBox.Show(control.Cars[0].Speed.ToString());
            lblPrimero.Text = "None";
            lblDistancia.Text = "0";
            control.ResetRace();
            mostrado = 0;
            count = 0;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            Gl.glViewport(0, 0, this.Width, this.Height);
            //select the projection matrix
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            //la reseteo
            Gl.glLoadIdentity();
            //45 = angulo de vision
            //1  = proporcion de alto por ancho
            //0.1f = distancia minima en la que se pinta
            //1000 = distancia maxima
            Glu.gluPerspective(55, this.Width/(float)this.Height  , 0.1f, 1000);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            control.Camara.SelectCamara(selectedCamara - 1); 
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.W)
            {
                moving = 1;
            }
            if (e.KeyData == Keys.S)
            {
                moving = -1;
            }

            if (e.KeyData == Keys.D1)
            {
                control.Cars[0].UpSpeed();
            }
            if (e.KeyData == Keys.D2)
            {
                control.Cars[1].UpSpeed();
            }
            if (e.KeyData == Keys.D3)
            {
                control.Cars[2].UpSpeed();
            }

        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            moving = 0;
        }
        
        #region temp
        // Will hold the user name
        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;
        // Needed to update the form with messages from another thread
        private delegate void UpdateLogCallback(string strMessage);
        // Needed to set the form to a "disconnected" state from another thread
        private delegate void CloseConnectionCallback(string strReason);
        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool Connected;



        // The event handler for application exit
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected == true)
            {
                // Closes the connections, streams, etc.
                Connected = false;
                swSender.Close();
                srReceiver.Close();
                tcpServer.Close();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // If we are not currently connected but awaiting to connect
            if (Connected == false)
            {
                // Initialize the connection
                try
                {
                    InitializeConnection();
                //   ConnectOnServerGame();
                }
                catch (Exception error)
                {
                    txtLog.Text += error.Message;
                }
            }
            else // We are connected, thus disconnect
            {
                CloseConnection("Disconnected at user's request.");
                
            }
        }

        private void InitializeConnection()
        {
            // Parse the IP address from the TextBox into an IPAddress object
            ipAddr = IPAddress.Parse(txtIp.Text);
            // Start a new TCP connections to the chat server
            tcpServer = new TcpClient();
            tcpServer.Connect(ipAddr, 2013);

            // Helps us track whether we're connected or not
            Connected = true;
            // Prepare the form
            UserName = txtUser.Text;

            // Disable and enable the appropriate fields
            txtIp.Enabled = false;
            txtUser.Enabled = false;
            txtMessage.Enabled = true;
            btnSend.Enabled = true;
            btnConnect.Text = "Disconnect";

            // Send the desired username to the server
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(txtUser.Text);
            swSender.Flush();

            // Start the thread for receiving messages and further communication
            thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging.Start();
        }

        private void ReceiveMessages()
        {
            // Receive the response from the server
            srReceiver = new StreamReader(tcpServer.GetStream());
            // If the first character of the response is 1, connection was successful
            string ConResponse = srReceiver.ReadLine();
            // If the first character is a 1, connection was successful
            if (Convert.ToInt32(ConResponse[0]) - Convert.ToInt32('0') > 0)
            {
                // Update the form to tell it we are now connected
               // PropertiesSnake.idSnake = Convert.ToInt32(ConResponse[0]) - Convert.ToInt32('0') - 1;
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected Successfully!" });
            }
            else // If the first character is not a 1 (probably a 0), the connection was unsuccessful
            {
                string Reason = "Not Connected: ";
                // Extract the reason out of the response message. The reason starts at the 3rd character
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);
                // Update the form with the reason why we couldn't connect
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });
                // Exit the method
                return;
            }
            // While we are successfully connected, read incoming lines from the server
            while (Connected)
            {
                // Show the messages in the log TextBox
                try
                {
                    this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { srReceiver.ReadLine() });
                }
                catch { }
            }
        }

        // This method is called from a different thread in order to update the log TextBox
        private void UpdateLog(string strMessage)
        {
            // Append text also scrolls the TextBox to the bottom each time
            txtLog.AppendText(strMessage + "\r\n");
        }

        // Closes a current connection
        private void CloseConnection(string Reason)
        {
            // Show the reason why the connection is ending
            txtLog.AppendText(Reason + "\r\n");
            // Enable and disable the appropriate controls on the form
            txtIp.Enabled = true;
            txtUser.Enabled = true;
            txtMessage.Enabled = false;
            btnSend.Enabled = false;
            btnConnect.Text = "Connect";

            // Close the objects
            Connected = false;
            swSender.Close();
            srReceiver.Close();
            tcpServer.Close();
        }

        // Sends the message typed in to the server
        private void SendMessage()
        {
            if (txtMessage.Lines.Length >= 1)
            {
                swSender.WriteLine(txtMessage.Text);
                swSender.Flush();
                txtMessage.Lines = null;
            }
            txtMessage.Text = "";
        }

        // We want to send the message when the Send button is clicked
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        // But we also want to send the message once Enter is pressed
        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            // If the key is Enter
            if (e.KeyChar == (char)13)
            {
                SendMessage();
            }
        }

        #endregion

   
    }
}
