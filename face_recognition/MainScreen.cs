using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tensorflow;

namespace face_recognition
{
    public partial class MainScreen : Form
    {
        private bool isRun = false;
        private VideoCapture capture;
        private Mat image;
        private Thread cameraThread;

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

        private void ViewClosed(object sender, EventArgs e)
        {
            cameraThread.Interrupt();
            isRun = false;
            //capture.Release();
        }

        private void ViewLoad(object sender, EventArgs e)
        {

        }

        private void MainScreen_Load(object sender, EventArgs e)
        {
            image = new Mat();
            cameraThread = new Thread(new ThreadStart(CaptureCameraCallback));
            cameraThread.Start();
            isRun = true;
        }

        /*
        StreamReader get_file()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
        }
        */

        private void CaptureCameraCallback()
        {
            var rnd = new Random();
            var color = Scalar.FromRgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
            CascadeClassifier faceCascasde = new CascadeClassifier("haarcascade_frontalface_default.xml");
            CascadeClassifier faceSideCascasde = new CascadeClassifier("haarcascade_profileface.xml");

            using (capture = new VideoCapture("rtsp://admin:abcd1234@192.168.0.88/1"))
            {
                while (capture.IsOpened() && isRun)
                {
                    try
                    {
                        capture.Read(image);
                        if (!image.Empty())
                        {
                            GC.Collect();
                            var imageRes = new Mat();
                            Cv2.Resize(image, imageRes, new OpenCvSharp.Size(700, 500));

                            var grayImage = new Mat();
                            Cv2.CvtColor(imageRes, grayImage, ColorConversionCodes.BGR2GRAY);
                            Cv2.EqualizeHist(grayImage, grayImage);

                            var faces = faceCascasde.DetectMultiScale(
                                image: grayImage,
                                scaleFactor: 2.1,
                                minNeighbors: 3,
                                flags: HaarDetectionType.ScaleImage,
                                minSize: new OpenCvSharp.Size(30, 30));

                            var faces_side = faceSideCascasde.DetectMultiScale(
                                image: grayImage,
                                scaleFactor: 2.1,
                                minNeighbors: 3,
                                flags: HaarDetectionType.ScaleImage,
                                minSize: new OpenCvSharp.Size(30, 30));

                            // found faces
                            foreach (var face in faces)
                            {
                                Cv2.Rectangle(imageRes, face, Scalar.Red, 2);
                                Cv2.PutText(imageRes, "Unknown", new OpenCvSharp.Point(face.Left + 2, face.Top + face.Width + 20),
                                    HersheyFonts.HersheyComplexSmall, 1, Scalar.Red, 2);
                            }

                            if (faces.Length == 0)
                                foreach (var face in faces_side)
                                {
                                    Cv2.Rectangle(imageRes, face, Scalar.Red, 2);
                                    Cv2.PutText(imageRes, "Unknown", new OpenCvSharp.Point(face.Left + 2, face.Top + face.Width + 20),
                                        HersheyFonts.HersheyComplexSmall, 1, Scalar.Red, 2);
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
            }
        }
    }
}
