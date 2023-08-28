using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using static System.Net.Mime.MediaTypeNames;



namespace cctvcsharp
{
    public partial class Form1 : Form
    {
        private VideoCaptureDevice? videoSource, videoSource1;
        int timerint, timerint2, timerint3, timerint2p, timerint3p, t3, t4, fps1, fps2, fps3, fps4;
        long t5, t6, t7, t8;
        int day2, month2, year2, minute2, second2, hour2, cnt;
        bool capture1, capture2, capture3, capture4, pic1process, pic2process, pic3process, pic4process, sensed, sensed1m, outputFileBool,
            boollisten, connected, boadcastbool, SensedsavingBool;
        string? path;
        string? outputFilePath;
        FileStream? outputFileStream;

        public string? receive;
        public string? TextToSend;

        private TcpListener? listener;
        private List<TcpClient>? clients;
        private List<StreamReader>? clientReaders;
        private List<StreamWriter>? clientWriters;
        private List<NetworkStream>? streams;
        private List<BackgroundWorker>? clientWorkers;

        public Form1()
        {
            /// Costura.Fody nuget 페키지 통합 단일파일
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        private static extern bool BlockInput(bool fBlockIt);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int SetWindowDisplayAffinity(IntPtr hWnd, int dwAffinity);

        public static void DisableScreenCapture()
        {
            //BlockInput(true);

            IntPtr hWnd = FindWindow(null, "Form1"); //IntPtr hWnd = FindWindow(null, "Form1");
            SetWindowDisplayAffinity(hWnd, 1);

        }

        public static void EnableScreenCapture()
        {
            //BlockInput(false);

            IntPtr hWnd = FindWindow(null, "Form1");
            //SetWindowDisplayAffinity(hWnd, 0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (ProcessSecurity.EnableDLLInjectionProtection())
            {
                richTextBox1.AppendText("EnableDLLInjectionProtection \n");
                ProcessSecurity.EnableDEP();
            }
            fps1 = 0; fps2 = 0; fps3 = 0; fps4 = 0;
            capture1 = false; capture2 = false; capture3 = false; capture4 = false;
            pic1process = false; pic2process = false; pic3process = false; pic4process = false; sensed = false; sensed1m = false;
            outputFileBool = false; SensedsavingBool = true;
            outputFilePath = null;
            timerint = 0; timerint3 = 0;
            timerint2p = 0; timerint3p = 0;
            textBox3.Text = "0";
            textBox4.Text = "0"; textBox5.Text = "0";
            timer1.Interval = (int)(1000 / int.Parse(textBox1.Text));
            textBox3.Text = timer1.Interval.ToString();

            //======================================================  Newtonsoft.Json  ======================================================
            textBox13.Text = "";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "data001.json");
            if (File.Exists(filePath))
            {
                string id = JsonConvert.DeserializeObject<string>(File.ReadAllText(filePath));
                // Use the retrieved ID value as needed
                textBox13.Text = id;

            }
            //textBox13.Text = "pakata@korea.com";
            textBox12.Text = "8888";

            timer1.Start();
            timer2.Start();
            timer3.Start();

            if (textBox13.Text != "")
            {
                // Public IP 주소 구하기
                WebClient wc = new WebClient();
                string result = wc.DownloadString("http://coreafactory.com/ip/index.php?email=" + textBox13.Text + "&port=" + textBox12.Text + "&side=server"); //Current IP Address: 119.202.146.249
                string ip = result.Split(':')[1].Split('<')[0].Trim();

                textBox11.Text = ip;
                textBox12.Text = result.Split(':')[2].Split('<')[0].Trim();
            }
            ProcessPriorityReal();

            DisableScreenCapture();
            richTextBox1.AppendText("DisabledScreenCapture \n");

            ShowActiveTcpConnections();
        }

        public void ShowActiveTcpConnections()
        {
            richTextBox1.AppendText("Active TCP Connections");
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation c in connections)
            {
                richTextBox1.AppendText(c.LocalEndPoint.ToString() + " : " + c.RemoteEndPoint.ToString() + "\n");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            VideoCaptureDeviceForm cameras = new VideoCaptureDeviceForm();
            textBox1.Text = "3";
            if (cameras.ShowDialog() == DialogResult.OK)
            {
                if (cameras.VideoDevice != null)
                {
                    // Use the selected video device
                    // Example: Start capturing video from the selected device
                    videoSource = cameras.VideoDevice;
                    videoSource.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame);
                    videoSource.Start();
                    pic1process = true;
                    cameras.Dispose();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                DateTime currentDate = DateTime.Today;
                day2 = currentDate.Day;
                month2 = currentDate.Month;
                year2 = currentDate.Year;

                string directoryPath = $"./{day2}_{month2}_{year2}";
                DirectoryInfo dir = new DirectoryInfo(directoryPath);

                if (!dir.Exists)
                {
                    dir.Create();
                }

                DateTime currentTime = DateTime.Now;
                hour2 = currentTime.Hour;
                minute2 = currentTime.Minute;
                second2 = currentTime.Second;
                cnt++;

                path = $"./{day2}_{month2}_{year2}/{hour2}_{minute2}_{second2}_{cnt}_s.jpg";

                using (FileStream file3 = System.IO.File.Create(path))
                {
                    //await Task.Delay(20);
                    try
                    {
                        //imagestream2.WriteTo(file3);
                        pictureBox2.Image.Save(file3, ImageFormat.Jpeg);
                    }
                    catch (Exception ex)
                    {
                        richTextBox1.AppendText(ex.ToString());
                    }
                }

                videoSource.SignalToStop();
                videoSource.WaitForStop();
                videoSource = null;
            }
        }
        private async void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if ((timerint > timerint2p) && pic1process)
            {
                pic1process = false;
                pic2process = false;

                ProcessPriorityReal();

                fps1++;
                timerint2p = timerint;
                MemoryStream imageStream = new MemoryStream();
                Bitmap tempbmp = (System.Drawing.Bitmap)eventArgs.Frame.Clone();
                pictureBox1.Image = tempbmp;// tempbmp;

                await Task.Delay(40);

                try
                {
                    // Create a new stream to save the image
                    tempbmp.Save(imageStream, ImageFormat.Jpeg);
                }
                catch (Exception ex)
                {
                    this.richTextBox1.Invoke(new MethodInvoker(delegate ()
                    {
                        richTextBox1.AppendText(ex.ToString());
                    }));
                }

                // Further processing with the image stream
                t3 = int.Parse(textBox2.Text);
                t4 = int.Parse(textBox4.Text);
                t5 = imageStream.Length;


                if (((t4 + t3) < t5) | ((t4 - t3) > t5))
                {
                    sensed = true;
                    pictureBox5.Image = tempbmp;

                    Broadcast(imageStream, 1);

                    if (capture1)
                    {
                        Sensedsaving(imageStream);

                        Console.Beep();
                    }

                }

                imageStream.Dispose();
                pic2process = true;
                pic1process = true;
            }
        }


        private async void VideoSource_NewFrame2(object sender, NewFrameEventArgs eventArgs)
        {
            if ((timerint3 > timerint3p) && pic2process) //(timerint > timerint2p) &&
            {
                pic2process = false;
                pic1process = false;

                ProcessPriorityReal();

                fps2++;
                timerint3p = timerint3;
                MemoryStream imageStream2 = new MemoryStream();
                //Bitmap tempbmp = (System.Drawing.Bitmap)eventArgs.Frame.Clone();
                Bitmap tempbmp = (System.Drawing.Bitmap)eventArgs.Frame.Clone();
                pictureBox2.Image = tempbmp;// tempbmp;

                //pictureBox2.Image = (System.Drawing.Bitmap)eventArgs.Frame.Clone();
                await Task.Delay(40);

                try
                {
                    // Create a new stream to save the image
                    tempbmp.Save(imageStream2, ImageFormat.Jpeg);
                }
                catch (Exception ex)
                {
                    this.richTextBox1.Invoke(new MethodInvoker(delegate ()
                    {
                        richTextBox1.AppendText(ex.ToString());
                    }));
                }
                // Further processing with the image stream
                t3 = int.Parse(textBox2.Text);
                t6 = int.Parse(textBox5.Text);
                t7 = imageStream2.Length;

                if (((t6 + t3) < t7) | ((t6 - t3) > t7))
                {
                    sensed = true;
                    pictureBox5.Image = tempbmp;

                    Broadcast(imageStream2, 2);

                    if (capture2)
                    {
                        Sensedsaving(imageStream2);

                        Console.Beep();
                    }
                }

                imageStream2.Dispose();
                pic2process = true;
                pic1process = true;
            }
        }

        private async void Broadcast(MemoryStream imageStream2, int deviceid)
        {
            if (connected && boadcastbool) //&& boadcastbool
            {
                if (imageStream2 != null)
                {
                    boadcastbool = false;
                    // Convert the image to a byte array
                    byte[] imageBytes;
                    imageBytes = imageStream2.ToArray();

                    string base64String = Convert.ToBase64String(imageBytes);
                    byte[] buffer = Encoding.UTF8.GetBytes(base64String);
                    await Task.Delay(10);

                    if (clients != null && streams != null)
                    {
                        for (int i = 0; i < clients.Count; i++)
                        {
                            try
                            {
                                NetworkStream stream = streams[i];
                                if (stream.CanWrite)
                                {
                                    await stream.WriteAsync(buffer, 0, buffer.Length);

                                    string flushString = "efig" + deviceid.ToString();//Convert.ToBase64String(temp);
                                    byte[] flushBytes = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(flushString)));
                                    await stream.WriteAsync(flushBytes, 0, flushBytes.Length);
                                    await stream.FlushAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                this.richTextBox1.Invoke(new MethodInvoker(delegate ()
                                {
                                    richTextBox1.AppendText(ex.ToString());
                                }));
                            }
                        }
                    }

                    boadcastbool = true;
                }
            }
        }

        private async void Sensedsaving(MemoryStream imagestream2)
        {
            if (SensedsavingBool)
            {
                SensedsavingBool = false;
                DateTime currentDate = DateTime.Today;
                day2 = currentDate.Day;
                month2 = currentDate.Month;
                year2 = currentDate.Year;

                string directoryPath = $"./{day2}_{month2}_{year2}";
                DirectoryInfo dir = new DirectoryInfo(directoryPath);

                if (!dir.Exists)
                {
                    dir.Create();
                }

                DateTime currentTime = DateTime.Now;
                hour2 = currentTime.Hour;
                minute2 = currentTime.Minute;
                second2 = currentTime.Second;
                cnt++;

                path = $"./{day2}_{month2}_{year2}/{hour2}_{minute2}_{second2}_{cnt}.jpg";

                using (FileStream file3 = System.IO.File.Create(path))
                {
                    await Task.Delay(20);
                    try
                    {
                        imagestream2.WriteTo(file3);
                        //pictureBox2.Image.Save(file3, ImageFormat.Jpeg);
                    }
                    catch (Exception ex)
                    {
                        richTextBox1.AppendText(ex.ToString());
                    }
                }

                if (outputFilePath == null)
                {
                    outputFilePath = $"./{day2}_{month2}_{year2}/{hour2}_{minute2}_{second2}_{cnt}.st1";
                    outputFileStream = System.IO.File.Create(outputFilePath);
                }

                if (!outputFileBool && (outputFileStream != null))
                {
                    outputFileBool = true;
                    try
                    {
                        // Convert the memory stream to a byte array
                        byte[] fileBytes = imagestream2.ToArray();

                        outputFileStream.Write(fileBytes, 0, fileBytes.Length);
                        string flushstring = "endofimage";
                        byte[] flushBytes = Encoding.UTF8.GetBytes(flushstring);
                        outputFileStream.Write(flushBytes, 0, flushBytes.Length);

                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions that occur during file reading
                        richTextBox1.AppendText(ex.ToString());
                    }
                    outputFileBool = false;
                }
                SensedsavingBool = true;
            }
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            capture1 = false; capture2 = false; capture3 = false; capture4 = false;
            pic1process = false; pic2process = false; pic3process = false; pic4process = false;
            boollisten = false;
            boadcastbool = false;

            if (outputFileStream != null)
            {
                outputFileStream.Flush();
                outputFileStream.Dispose();
            }
            //mailsmtpnaver("enemy at the gate!! warning!!", outputFilePath);
            outputFilePath = null;

            if ((videoSource != null && videoSource.IsRunning) || (videoSource1 != null && videoSource1.IsRunning))
            {
                if (path != null)
                {
                    mailsmtpnaver("enemy at the gate!! warning!!", path);
                    await Task.Delay(40);
                }
            }

            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
            if (videoSource1 != null && videoSource1.IsRunning)
            {
                videoSource1.SignalToStop();
                videoSource1.WaitForStop();
            }
            //base.OnFormClosing(e);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timerint = timerint + 1; timerint3 = timerint3 + 1;
            textBox4.Text = t5.ToString();
            textBox5.Text = t7.ToString();
            /*  CPU 사용률 확인
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            float cpuUsage = cpuCounter.NextSample().RawValue; // CPU 사용률 확인

            textBox7.Text = cpuUsage.ToString()+"%";
            if (cpuUsage > 90) // CPU 사용률이 90% 이상인 경우
            {
                // 적절한 조치를 취할 수 있음
            }
            */
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

            textBox3.Text = timerint.ToString();
            timerint2 = timerint;
            timerint2p = 0; timerint3p = 0;
            timerint = 0; timerint3 = 0;
            textBox3.Text = fps1.ToString(); textBox8.Text = fps2.ToString(); textBox9.Text = fps3.ToString(); textBox10.Text = fps4.ToString();
            fps1 = 0; fps2 = 0; fps3 = 0; fps4 = 0;

        }

        private void pictureBox1_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = "1";
            }
            if (int.Parse(textBox1.Text) <= 0)
            {
                textBox1.Text = "1";
            }
            if (int.Parse(textBox1.Text) > 20)
            {
                textBox1.Text = "20";
            }

            timer1.Interval = (int)(1000 / int.Parse(textBox1.Text));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (capture1)
            {
                button3.Text = "sensing";
                capture1 = false;
            }
            else
            {
                capture1 = true;
                button3.Text = "off sense";
            }
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox1.SelectAll();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DisableScreenCapture();
        }

        private void mailsmtpnaver(string args, string imagePath)
        {
            string emailTitle = "CCTV system Operation";
            string emailContent = args;
            string fromEmail = "pakata@naver.com";
            string fromName = "pakata";
            string toEmail = "pakata@korea.com";
            string toName = "pakata";

            // SMTP 서버 설정
            string smtpServer = "smtp.naver.com"; //ssl://  tls://
            int smtpPort = 587;
            string smtpUsername = "pakata";
            string smtpPassword = "sherif82";


            // 이메일 메시지 작성
            MailMessage mail = new MailMessage();
            mail.Subject = emailTitle;
            mail.Body = emailContent;
            mail.IsBodyHtml = true;
            mail.From = new MailAddress(fromEmail, fromName);
            mail.To.Add(new MailAddress(toEmail, toName));

            // 이미지 첨부
            if (imagePath != null)
            {
                Attachment attachment = new Attachment(imagePath);
                mail.Attachments.Add(attachment);
            }

            // SMTP 클라이언트 설정
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            try
            {
                // 이메일 보내기
                smtpClient.Send(mail);
                //richTextBox1.Text+="Email sent successfully.";
                Invoke((MethodInvoker)(() =>
                {
                    richTextBox1.Text += "Email sent successfully.";
                }));
            }
            catch (Exception ex)
            {
                //richTextBox1.Text+="An error occurred while sending the email: " + ex.Message;
                Invoke((MethodInvoker)(() =>
                {
                    richTextBox1.Text += "An error occurred while sending the email: " + ex.Message;
                }));
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            VideoCaptureDeviceForm cameras2 = new VideoCaptureDeviceForm();

            if (cameras2.ShowDialog() == DialogResult.OK)
            {
                if (cameras2.VideoDevice != null)
                {
                    // Use the selected video device
                    // Example: Start capturing video from the selected device
                    videoSource1 = cameras2.VideoDevice;
                    videoSource1.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame2);
                    videoSource1.Start();
                    pic2process = true;
                    cameras2.Dispose();
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (videoSource1 != null && videoSource1.IsRunning)
            {
                DateTime currentDate = DateTime.Today;
                day2 = currentDate.Day;
                month2 = currentDate.Month;
                year2 = currentDate.Year;

                string directoryPath = $"./{day2}_{month2}_{year2}";
                DirectoryInfo dir = new DirectoryInfo(directoryPath);

                if (!dir.Exists)
                {
                    dir.Create();
                }

                DateTime currentTime = DateTime.Now;
                hour2 = currentTime.Hour;
                minute2 = currentTime.Minute;
                second2 = currentTime.Second;
                cnt++;

                path = $"./{day2}_{month2}_{year2}/{hour2}_{minute2}_{second2}_{cnt}_s.jpg";

                using (FileStream file3 = System.IO.File.Create(path))
                {
                    //await Task.Delay(20);
                    try
                    {
                        //imagestream2.WriteTo(file3);
                        pictureBox2.Image.Save(file3, ImageFormat.Jpeg);
                    }
                    catch (Exception ex)
                    {
                        richTextBox1.AppendText(ex.ToString());
                    }
                }

                videoSource1.SignalToStop();
                videoSource1.WaitForStop();
                videoSource1 = null;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (capture2)
            {
                button7.Text = "sensing";
                capture2 = false;
            }
            else
            {
                capture2 = true;
                button7.Text = "off sense";
            }
        }

        private async void button18_Click(object sender, EventArgs e)
        {
            boollisten = false;
            boadcastbool = false;
            outputFileBool = true;
            path = null;

            //mailsmtpnaver("CCTV TERMINATING!!", path);
            await Task.Delay(40);
            Console.Beep();
            Form1.ActiveForm.Close();
        }

        private void button19_Click(object sender, EventArgs e)
        {

        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            // 1 minute interval
            if (!sensed && sensed1m)
            {
                sensed1m = false;

            }

            if (sensed)
            {
                sensed1m = true;
                sensed = false;
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            DateTime currentDate = DateTime.Today;
            day2 = currentDate.Day;
            month2 = currentDate.Month;
            year2 = currentDate.Year;

            string directoryPath = $"./{day2}_{month2}_{year2}";

            string[] videoFiles = Directory.GetFiles(directoryPath, "*.st1");



            foreach (string videoFile in videoFiles)
            {
                try
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(videoFile);

                    int markerIndex = 0;
                    byte[] markerBytes = Encoding.UTF8.GetBytes("endofimage");
                    long flushmarkerstart = 0;
                    for (int i = 0; i < fileBytes.Length - markerBytes.Length + 1; i++)
                    {
                        if (fileBytes[i] == markerBytes[markerIndex])
                        {
                            markerIndex++;
                            if (markerIndex == markerBytes.Length)
                            {
                                // Remove the marker from the bytes array
                                byte[] imageData = new byte[i - flushmarkerstart - markerBytes.Length + 1];
                                Array.Copy(fileBytes, flushmarkerstart, imageData, 0, imageData.Length);
                                flushmarkerstart = i + 1;
                                // Write the image data to a file
                                string outputFile = $"{directoryPath}/{cnt}.jpg";
                                System.IO.File.WriteAllBytes(outputFile, imageData);
                                cnt++;


                                // Reset marker index and continue to the next marker
                                markerIndex = 0;
                            }
                        }
                        else
                        {
                            markerIndex = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during file reading
                    richTextBox1.AppendText(ex.ToString());
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void button17_Click(object sender, EventArgs e)
        {
            if ((button17.Text == "Server On") && (listener != null))
            {
                button17.Text = "Server Off";
                boollisten = false;
                boadcastbool = false;
                listener.Stop();
            }
            else
            {
                if (textBox13.Text != null)
                {
                    // Public IP 주소 구하기
                    WebClient wc = new WebClient();
                    string result = wc.DownloadString("http://coreafactory.com/ip/index.php?email=" + textBox13.Text + "&port=" + textBox12.Text + "&side=server"); //Current IP Address: 119.202.146.249
                    string ip = result.Split(':')[1].Split('<')[0].Trim();

                    textBox11.Text = ip;
                    textBox12.Text = result.Split(':')[2].Split('<')[0].Trim();

                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "data001.json");
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(textBox13.Text));
                }
                boollisten = true;
                button17.Text = "Server On";
                listener = new TcpListener(IPAddress.Any, int.Parse(textBox12.Text));
                listener.Start();

                clients = new List<TcpClient>();
                clientReaders = new List<StreamReader>();
                clientWriters = new List<StreamWriter>();
                streams = new List<NetworkStream>();
                clientWorkers = new List<BackgroundWorker>();

                backgroundWorker1.RunWorkerAsync();
                backgroundWorker2.WorkerSupportsCancellation = true;

                boadcastbool = true;

                if (listener != null)
                {
                    if (richTextBox2.Text.IndexOf("Server") >= 0)
                    {
                        richTextBox2.Text = richTextBox2.Text.Replace("OFF", "listening");
                    }
                    else
                    {
                        richTextBox2.AppendText("Server listening... \n");
                    }
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (boollisten)
            {
                TcpClient client2 = new TcpClient();
                try
                {
                    if ((listener != null) && (clients != null))
                    {
                        client2 = listener.AcceptTcpClient();
                        clients.Add(client2);
                    }
                }
                catch (Exception ex)
                {
                    this.richTextBox1.Invoke(new MethodInvoker(delegate ()
                    {
                        richTextBox1.AppendText(ex.ToString() + "\n");
                    }));
                }

                if ((client2.Client.RemoteEndPoint != null) && (streams != null) && (clientWorkers != null))
                {
                    this.richTextBox2.Invoke(new MethodInvoker(delegate ()
                    {
                        richTextBox2.AppendText("Client : " + client2.Client.RemoteEndPoint.ToString() + "\n");
                    }));


                    //StreamReader clientReader2 = new StreamReader(client2.GetStream());
                    //clientReaders.Add(clientReader2);

                    //StreamWriter clientWriter2 = new StreamWriter(client2.GetStream());
                    //clientWriters.Add(clientWriter2);

                    NetworkStream stream = client2.GetStream();
                    //await Task.Delay(1000);
                    streams.Add(stream);

                    BackgroundWorker clientWorker = new BackgroundWorker();
                    clientWorker.DoWork += HandleClientCommunication;
                    clientWorkers.Add(clientWorker);
                    clientWorker.RunWorkerAsync(client2);
                }
            }

            if (!boollisten)
            {
                this.richTextBox2.Invoke(new MethodInvoker(delegate ()
                {
                    richTextBox2.Text = richTextBox2.Text.Replace("listening", "OFF");
                }));
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private async void HandleClientCommunication(object sender, DoWorkEventArgs e)
        {
            TcpClient client2 = e.Argument as TcpClient;
            int index = clients.IndexOf(client2);
            string tempend = client2.Client.RemoteEndPoint.ToString();
            //int index = e.Argument as int;
            //StreamReader clientReader = clientReaders[index];
            //StreamWriter clientWriter = clientWriters[index];
            if (client2 != null)
            {
                while (client2.Connected)
                {
                    connected = client2.Connected;
                    await Task.Delay(100);
                    /*
                    try
                    {
                        string receive = clientReader.ReadLine();
                        if (receive != null)
                        {
                            this.richTextBox1.Invoke(new MethodInvoker(delegate ()
                            {
                                richTextBox1.AppendText("Client " + index + ": " + receive + "\n");
                            }));

                            BroadcastMessage(receive, index);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that occurs during client communication
                        this.richTextBox1.Invoke(new MethodInvoker(delegate ()
                        {
                            richTextBox1.AppendText(ex.Message.ToString() + "\n");
                        }));
                        break;
                    }
                    */
                }

                string temp = "Client : " + tempend;
                this.richTextBox2.Invoke(new MethodInvoker(delegate ()
                {
                    richTextBox2.Text = richTextBox2.Text.Replace(temp, temp + " disconnected.");
                }));

                clients.Remove(client2);
                client2.Close();

                if (streams != null && index >= 0 && index < streams.Count && streams[index] != null)
                {
                    streams.RemoveAt(index);
                }

                if (clientWorkers != null && index >= 0 && index < clientWorkers.Count && clientWorkers[index] != null)
                {
                    clientWorkers.RemoveAt(index);
                }


                if (clients.Count <= 0)
                {
                    connected = false;
                }
            }
            // Cleanup resources for the disconnected client
            //clientReader.Close();
            //clientWriter.Close();
            //clientReaders.RemoveAt(index);
            //clientWriters.RemoveAt(index);

        }

        private void BroadcastMessage(string message, int senderIndex)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (i != senderIndex)
                {
                    try
                    {
                        clientWriters[i].WriteLine(message);
                        clientWriters[i].Flush();
                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that occurs during broadcast
                        richTextBox1.AppendText(ex.Message.ToString());
                    }
                }
            }
        }

        //////////////////////////////////////// protect from dll injection //////////////////////////////////////////////
        public static class ProcessSecurity
        {
            private const int PROCESS_MITIGATION_POLICY_DLL_INJECTION = 0x0000000A;
            private const int PROCESS_DEP_ENABLE = 0x00000001;

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool SetProcessMitigationPolicy(
                int mitigationPolicy,
                IntPtr lpBuffer,
                int dwLength
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool SetProcessDEPPolicy(
                int dwFlags
            );

            public static bool EnableDLLInjectionProtection()
            {
                // Set process mitigation policy for DLL injection protection
                PROCESS_MITIGATION_POLICY policy = new PROCESS_MITIGATION_POLICY
                {
                    Enable = 1 // Enable DLL injection protection
                };

                int size = Marshal.SizeOf(policy);
                IntPtr policyPtr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(policy, policyPtr, false);

                bool result = SetProcessMitigationPolicy(PROCESS_MITIGATION_POLICY_DLL_INJECTION, policyPtr, size);

                Marshal.FreeHGlobal(policyPtr);

                return result;
            }

            public static bool EnableDEP()
            {// Enable Data Execution Prevention (DEP)
                bool result = SetProcessDEPPolicy(PROCESS_DEP_ENABLE);
                if (!result)
                {
                    string errorCode = Marshal.GetLastWin32Error().ToString();
                    Console.WriteLine($"Failed to enable DEP. Error code: {errorCode}");
                }
                return result;
            }

            // Structure for PROCESS_MITIGATION_POLICY
            [StructLayout(LayoutKind.Sequential)]
            private struct PROCESS_MITIGATION_POLICY
            {
                public int Enable;
            }


        }
        /////////////////////////////////////////////  ProcessPriority  ////////////////////////////////////////////////
        private void ProcessPriorityReal()
        {
            Process currentProcess = Process.GetCurrentProcess();
            ProcessPriorityClass currentPriority = currentProcess.PriorityClass;

            try
            {
                // 프로세스 우선 순위 낮추기
                currentProcess.PriorityClass = ProcessPriorityClass.RealTime;

                // 변경된 프로세스 우선 순위 확인
                currentPriority = currentProcess.PriorityClass;
                this.richTextBox1.Invoke(new MethodInvoker(delegate ()
                {
                    richTextBox1.AppendText(currentPriority.ToString() + "\n");
                }));
            }
            catch (Exception ex)
            {
                this.richTextBox1.Invoke(new MethodInvoker(delegate ()
                {
                    richTextBox1.AppendText("프로세스 우선 순위를 변경하는 동안 오류가 발생했습니다: " + ex.Message);
                }));
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void button21_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (richTextBox1.TextLength > 30000)
            {
                richTextBox1.Text = "";
            }

            richTextBox1.ScrollToCaret();
        }


        private void button13_Click(object sender, EventArgs e)
        {


        }

        private void button14_Click(object sender, EventArgs e)
        {

        }


    }
}