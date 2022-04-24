using face_recognition.Model;
using Newtonsoft.Json;
using Numpy;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Websocket.Client;

namespace face_recognition
{
    public partial class MainScreen : Form
    {
        private bool isRun = false;
        private VideoCapture capture;
        private Mat image;
        private Thread cameraThread;

        string URL_WEB_SOCKET = "ws://localhost:8000";

        List<MdlRenameFace> LIST_RENAME_FACE { get; set; }

        public MainScreen()
        {
            InitializeComponent();

            try
            {
                Load += ViewLoad;
                Closed += ViewClosed;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ViewLoad(object sender, EventArgs e)
        {

        }

        void find_most_image()
        {
            try
            {
                Console.WriteLine("-----------------############################");

                string rootFolder = Path.GetDirectoryName(Application.ExecutablePath) + "\\Log_Image";

                string hasVerifyFolder = rootFolder + "\\Verify\\";
                hasVerifyFolder = hasVerifyFolder + (DateTime.Now.ToString("dd_MM_yyyy"));
                if (!Directory.Exists(hasVerifyFolder))
                    Directory.CreateDirectory(hasVerifyFolder);

                string calculatingFolder = rootFolder + "\\Calculating";
                DirectoryInfo[] cc = new DirectoryInfo(calculatingFolder).GetDirectories();


                List<MdlLogFile> listFile = new List<MdlLogFile>();
                foreach (DirectoryInfo fileinfo in cc)
                {
                    string dir_path = fileinfo.FullName;
                    FileInfo[] dd = new DirectoryInfo(dir_path).GetFiles("*.jpg");
                    foreach (FileInfo file in dd)
                    {
                        var split_name = file.Name.Split('_');
                        var name = split_name[0];

                        listFile.Add(new MdlLogFile
                        {
                            name = name,
                            path_folder = fileinfo.FullName,
                            full_path = file.FullName
                        });
                    }
                }

                //Console.WriteLine

                /// <summary>
                /// FIND MOST OCCURENCE
                /// </summary>
                var most = listFile.GroupBy(i => i.name).OrderByDescending(grp => grp.Count())
                    .Select(grp => grp.Key).FirstOrDefault();
                var ff = listFile.Find(i => i.name.Equals(most));

                if (ff != null)
                {
                    /// <summary>
                    /// COPY
                    /// </summary>
                    string fileToCopy = hasVerifyFolder + "\\" + ff.name + "_" + (DateTime.Now.ToString("HH_mm_ss")) + ".jpg";
                    if (File.Exists(fileToCopy))
                        File.Delete(fileToCopy);
                    File.Copy(ff.full_path, fileToCopy);

                    /// <summary>
                    /// DELETE
                    /// </summary>
                    foreach (MdlLogFile ll in listFile)
                    {
                        if (Directory.Exists(ll.path_folder))
                            Directory.Delete(ll.path_folder, true);
                    }
                }
            } finally
            {
                setImageList();
            }
        }

        ImageList getImageList()
        {
            DirectoryInfo directory = new DirectoryInfo(@"D:\RadiFolder\FR\face_recognition\face_recognition\bin\Debug\Log_Image\Verify\21_04_2022");
            FileInfo[] Archives = directory.GetFiles("*.jpg");

            ImageList il = new ImageList();
            foreach (FileInfo fileinfo in Archives)
            {
                GC.Collect();
                this.Invoke(new Action(() => il.Images.Add(fileinfo.Name, Image.FromFile(fileinfo.FullName))));
                //il.Images.Add(Image.FromFile(fileinfo.FullName));
            }
            list_view_img.Sorting = SortOrder.Descending;
            list_view_img.View = View.LargeIcon;
            il.ImageSize = new System.Drawing.Size(100, 100);

            return il;
        }
        ImageList il = new ImageList();
        void setImageList()
        {
            list_view_img.Invoke(new Action(() => list_view_img.Clear()));

            il = getImageList();

            list_view_img.Invoke(new Action(() => list_view_img.LargeImageList = il));

            for (int i = 0; i < il.Images.Count; i++)
            {
                var splitS = il.Images.Keys[i].Split('_');
                string name = splitS[0].Substring(0, 1).ToUpper() + splitS[0].Substring(1).ToLower();

                ListViewItem item = new ListViewItem();
                item.ImageIndex = i;
                item.Text = name;
                list_view_img.Invoke(new Action(() => list_view_img.Items.Add(item)));

                if (i == 10)
                    break;
            }

            list_view_img.Invoke(new Action(() =>
            {
                this.list_view_img.Refresh();
                this.list_view_img.Invalidate();
            }));

            isRunningLog = false;
        }

        void removeLastImage(ImageList imageList)
        {
            var lastIndex = imageList.Images.Count - 1;
            var lastImage = imageList.Images[lastIndex];

            this.Invoke(new Action(() => imageList.Images.RemoveAt(lastIndex)));

            lastImage.Dispose();
        }

        private void MainScreen_Load(object sender, EventArgs e)
        {
            setImageList();

            image = new Mat();
            cameraThread = new Thread(new ThreadStart(CaptureCameraCallback));
            cameraThread.Start();
            isRun = true;
        }

        private void ViewClosed(object sender, EventArgs e)
        {
            cameraThread.Interrupt();
            isRun = false;
        }

        void image_to_transpose(WebsocketClient ws, Mat image, Rect[] faces)
        {
            StringBuilder json = new StringBuilder();
            json.Append("[");
            int faceFoundLength = faces.Length - 1;

            int num = 0;
            foreach (var face_place in faces)
            {
                var extract_face = image[face_place];
                var extract_face_arr = extract_face.ToBytes();
                var base64 = Convert.ToBase64String(extract_face_arr, 0, extract_face_arr.Length);

                //Console.WriteLine("{" + face_place.Height + "," + face_place.Width + "}");
                if (face_place.Height < 300)
                {
                    var tempJson =
                    "{" +
                    "\"base_64_img\":" + "\"" + base64 + "\"," +
                    "\"X\":" + "\"" + face_place.X + "\"," +
                    "\"Y\":" + "\"" + face_place.Y + "\"" +
                    "}";

                    if (num++ != faceFoundLength)
                    {
                        tempJson += ",";
                    }
                    json.Append(tempJson);
                }
                else
                {
                    faceFoundLength -= 1;
                }
            }
            json.Append("]");

            var jsonArr = Encoding.ASCII.GetBytes(json.ToString());
            Task.Run(() => ws.Send(jsonArr));
        }

        List<MdlLogFolder> listLogFolder = new List<MdlLogFolder>();
        private bool isRunningLog = false;
        private void CaptureCameraCallback()
        {
            CascadeClassifier faceCascasde = new CascadeClassifier("haarcascade_frontalface_default.xml");
            CascadeClassifier faceSideCascasde = new CascadeClassifier("haarcascade_profileface.xml");

            var exitEvent = new ManualResetEvent(false);
            var URL = new Uri(URL_WEB_SOCKET);

            using (var client = new WebsocketClient(URL))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info => Console.WriteLine("Reconection: {0}", info.Type));

                client.Start();

                client.MessageReceived.Subscribe(msg =>
                {
                    if (!msg.Text.Equals("started"))
                    {
                        LIST_RENAME_FACE = JsonConvert.DeserializeObject<List<MdlRenameFace>>(msg.Text);
                    }
                });

                Task.Run(() => client.Send("started"));

                //exitEvent.WaitOne();

                var hog = new HOGDescriptor();
                hog.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());

                var recognizer_face = OpenCvSharp.Face.LBPHFaceRecognizer.Create();
                //recognizer_face.Read("");

                Stopwatch stLog = new Stopwatch();
                stLog.Start();

                int expectedProcessTimePerFrame = 1000 / 55; // 55 fps
                Stopwatch st = new Stopwatch();
                st.Start();

                using (capture = new VideoCapture("rtsp://admin:abcd1234@192.168.0.88/1"))
                {

                    Cv2.StartWindowThread();
                    while (capture.IsOpened() && isRun)
                    {
                        try
                        {
                            capture.Read(image);
                            long started = st.ElapsedMilliseconds;

                            if (!image.Empty())
                            {
                                GC.Collect();
                                var imageRes = new Mat();
                                //Cv2.Resize(image, imageRes, new OpenCvSharp.Size(700, 500));
                                Cv2.Resize(image, imageRes, new OpenCvSharp.Size(cv_box.Size.Width, cv_box.Size.Height));

                                int elapsed = (int)(st.ElapsedMilliseconds - started);
                                int delay = expectedProcessTimePerFrame - elapsed;

                                var _X = 0;
                                var _Y = 0;
                                var _WIDTH = imageRes.Width;
                                var _HEIGHT = imageRes.Height;
                                Cv2.PutText(img: imageRes, text: delay.ToString(), new OpenCvSharp.Point(_X, _Y + (_HEIGHT - 5)),
                                        fontFace: HersheyFonts.HersheyDuplex, fontScale: 1, color: Scalar.FloralWhite, thickness: 2);

                                if (delay > 0)
                                {
                                    try
                                    {
                                        this.Invoke(new Action(() =>
                                        {
                                            Thread.Sleep(delay);
                                        }));
                                    }
                                    catch (Exception) { }
                                }

                                var grayImage = new Mat();
                                Cv2.CvtColor(imageRes, grayImage, ColorConversionCodes.BGR2GRAY);
                                Cv2.EqualizeHist(grayImage, grayImage);

                                var faces_front = faceCascasde.DetectMultiScale(
                                    image: grayImage,
                                    scaleFactor: 2.1,
                                    minNeighbors: 3,
                                    flags: HaarDetectionType.ScaleImage,
                                    minSize: new OpenCvSharp.Size(100, 100));

                                var faces_side = faceSideCascasde.DetectMultiScale(
                                    image: grayImage,
                                    scaleFactor: 2.1,
                                    minNeighbors: 3,
                                    flags: HaarDetectionType.ScaleImage,
                                    minSize: new OpenCvSharp.Size(100, 100));

                                int listFaceLength = faces_front.Length + faces_side.Length;
                                if (listFaceLength > 0)
                                {
                                    try
                                    {
                                        //setImageList();
                                    }
                                    catch (Exception) { break; }

                                    Rect[] listFace = new Rect[listFaceLength];

                                    int indexListFace = 0;
                                    foreach (var face in faces_front)
                                        listFace[indexListFace++] = face;
                                    foreach (var face in faces_side)
                                        listFace[indexListFace++] = face;

                                    image_to_transpose(client, imageRes, listFace);

                                    foreach (var face in listFace)
                                    {
                                        if (face.Height < 250)
                                        {
                                            var x1 = face.X;
                                            var y1 = face.Y;
                                            var x2 = face.X + face.Width;
                                            var y2 = face.Y + face.Height;

                                            if (LIST_RENAME_FACE != null)
                                            {
                                                var closest = LIST_RENAME_FACE.OrderBy(v => Math.Abs((int)v.X - x1)).First();
                                                if (!closest.name_id.Equals("None"))
                                                {
                                                    if (!isRunningLog)
                                                    {
                                                        string folder_name = "Log_Image\\Calculating\\" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm") + "__" + closest.X.ToString();
                                                        int count_image_save = 1;
                                                        try
                                                        {
                                                            if (!Directory.Exists(folder_name))
                                                            {
                                                                Directory.CreateDirectory(folder_name);
                                                                listLogFolder.Add(new MdlLogFolder
                                                                {
                                                                    id = closest.X,
                                                                    name_folder = folder_name,
                                                                    count_current = 1
                                                                });
                                                            }
                                                            else
                                                            {
                                                                var log_folder = listLogFolder.FindIndex(x => x.name_folder.Equals(folder_name));
                                                                listLogFolder[log_folder].count_current += 1;
                                                                count_image_save = listLogFolder[log_folder].count_current;
                                                            }
                                                        }
                                                        catch (Exception) { }

                                                        //imageRes[face];
                                                        var faceImage = new Mat();
                                                        Cv2.Resize(imageRes[face], faceImage,new OpenCvSharp.Size(640, 480), 0, 0, interpolation: InterpolationFlags.Cubic);

                                                        string count_image_save_str = count_image_save.ToString();
                                                        count_image_save += 1;
                                                        Console.WriteLine("res: " + closest.name_id);

                                                        string name = "img";
                                                        if (closest.name_id.Contains("="))
                                                        {
                                                            var name_split = closest.name_id.Split('=');
                                                            name = name_split[0];
                                                            name = name.TrimEnd();
                                                        }



                                                        Cv2.ImWrite(folder_name + "\\" + name + "_" + count_image_save_str + ".jpg", faceImage);
                                                    }

                                                    TimeSpan timeTakeForLog = stLog.Elapsed;
                                                    if (timeTakeForLog.Seconds == 5 && !isRunningLog)
                                                    {
                                                        isRunningLog = true;
                                                        Console.WriteLine("------------------------------------------");
                                                        /*
                                                        try
                                                        {
                                                            setImageList();
                                                        } catch (Exception) { }
                                                        */
                                                        new Thread(new ThreadStart(() =>
                                                        {

                                                            find_most_image();

                                                        })).Start();
                                                        
                                                        stLog.Restart();
                                                    }

                                                    Cv2.Rectangle(imageRes, new OpenCvSharp.Point(x1, y1), new OpenCvSharp.Point(x2, y2), Scalar.Green, 2);
                                                    Cv2.PutText(imageRes, closest.name_id, new OpenCvSharp.Point(face.Left + 2, face.Top + face.Width + 20),
                                                        HersheyFonts.HersheyComplexSmall, 1, Scalar.LightBlue, 2);
                                                }
                                                else
                                                {
                                                    Cv2.Rectangle(imageRes, new OpenCvSharp.Point(x1, y1), new OpenCvSharp.Point(x2, y2), Scalar.Red, 2);
                                                    Cv2.PutText(imageRes, "Unknown", new OpenCvSharp.Point(face.Left + 2, face.Top + face.Width + 20),
                                                        HersheyFonts.HersheyComplexSmall, 1, Scalar.Red, 2);
                                                }
                                            }
                                            else
                                            {
                                                Cv2.Rectangle(imageRes, new OpenCvSharp.Point(x1, y1), new OpenCvSharp.Point(x2, y2), Scalar.Red, 2);
                                                Cv2.PutText(imageRes, "Unknown", new OpenCvSharp.Point(face.Left + 2, face.Top + face.Width + 20),
                                                    HersheyFonts.HersheyComplexSmall, 1, Scalar.Red, 2);
                                            }
                                        }
                                    }
                                }

                                TimeSpan timeTakeForLogs = stLog.Elapsed;
                                if (timeTakeForLogs.Seconds == 8) 
                                {
                                    Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\n^^^^^^^^^^^^^^^^^^\n<<<<<<<<<<<<<<<<<<<<<<<<\n");
                                    stLog.Restart();
                                }
                                
                                var bmpCam = BitmapConverter.ToBitmap(imageRes);
                                cv_box.Image = bmpCam;
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (System.AccessViolationException e)
                        {
                            break;
                        }
                    }
                    Console.WriteLine("Release");
                    capture.Release();
                    Cv2.DestroyAllWindows();

                    if (listLogFolder.Count > 0)
                    {
                        foreach (MdlLogFolder mdl in listLogFolder)
                        {
                            Console.WriteLine(mdl);
                        }
                    }
                }
            }
        }
    }
}
