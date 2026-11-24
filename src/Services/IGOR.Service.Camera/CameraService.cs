using IGOR.Interface.Camera;
using IGOR.Interface.Communication;
using IGOR.Interface.Game;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.Internal.Vectors;
using Prism.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace IGOR.Service.Camera
{
    public class CameraService : ICameraService
    {
        private static int ImageCounter = 0;

        private const double ScreenDiagonal = 19;
        private double _screenHeightmm;
        private double _screenWidthmm;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _log;
        private readonly IPlcCom _plcCom;
        //private const double SCALE = 1.5;
        private VideoCapture _camera;

        public double ScreenWidth { get; private set; }
        public double ScreenHeight { get; private set; }

        public Mat CameraMatrix { get; set; } = new Mat();
        public Mat DistortionCoefficients { get; set; } = new Mat();

        public CameraService(ILogger logger, IEventAggregator eventAggregator, IPlcCom plcCommunication)
        {
            _eventAggregator = eventAggregator;
            _log = logger.ForContext<CameraService>();
            _plcCom = plcCommunication;
        }


        public Mat ReadImage(bool undistortImage = true)
        {
            var image = new Mat();
            var undistortedImage = new Mat();


            // read 3 images to be sure the display shows the aruco codes
            for (int i = 0; i < 5; i++)
            {
                _camera.Read(image);
                if (undistortImage &&
                    CameraMatrix != null && CameraMatrix.Cols > 0 && CameraMatrix.Rows > 0 &&
                    DistortionCoefficients != null && DistortionCoefficients.Cols > 0 && DistortionCoefficients.Rows > 0)
                {
                    Cv2.Undistort(image, undistortedImage, CameraMatrix, DistortionCoefficients);
                }
            }

            if (image == null)
            {
                image = null;
                _log.Error("Es konnte kein Bild aufgenommen werden");
            }

            return image;
        }

        public IEnumerable<Stone> ReadStonePositions(Dictionary<FieldPosition, (double x, double y)> validPositions, uint tolerance)
        {
            ImageCounter++;

            var image = ReadImage();
            double meanValue = 100.0;

            if (image == null)
                return null;

            image.SaveImage(@"Images\" + $"{ImageCounter}_1_inputImage.jpg");
             
            // Get the mean value of the image
            var gray = image.CvtColor(ColorConversionCodes.BGR2GRAY);  
            meanValue = Cv2.Mean(gray).ToDouble();


            var refImage = GetRefImage(image);


            // save the images for debug reasons



            if (refImage != null)
            {
                refImage.SaveImage(@"Images\" + $"{ImageCounter}_2_refImage.jpg");
            }




            // Berechne das Histogramm
            Mat hist = new Mat();
            int histSize = 256;  // Anzahl der Bins im Histogramm
            Rangef[] range = { new Rangef(0, 256) };  // Wertebereich im Histogramm
            int[] channels = { 0 };
            Cv2.CalcHist(new Mat[] { image }, channels, null, hist, 1, new int[] { histSize }, range);



            // Optional: Zeige das Histogramm an
            int histWidth = 512;
            int histHeight = 400;
            int binWidth = histWidth / histSize;

            Mat histImage = new Mat(new Size(histWidth, histHeight), MatType.CV_8UC3, Scalar.Black);

            Cv2.Normalize(hist, hist, 0, histImage.Rows, NormTypes.MinMax, -1, null);

            for (int i = 1; i < histSize; i++)
            {
                Cv2.Line(histImage, new Point(binWidth * (i - 1), histHeight - Math.Round(hist.At<float>(i - 1))),
                         new Point(binWidth * i, histHeight - Math.Round(hist.At<float>(i))),
                         Scalar.White, 2, LineTypes.Link8, 0);
            }

            Cv2.Line(histImage, new Point(0, (histHeight / 255) * meanValue), new Point(histWidth, (histHeight / 255) * meanValue), Scalar.Red, 4, LineTypes.Link8, 0);


            //Cv2.ImShow("Histogramm", histImage);
            //Cv2.WaitKey(0);

            //histImage.SaveImage(@"Images\" + $"{ImageCounter}_Histogram.jpg");


            // use light, if meanValue of image is < 90
            bool useLight = meanValue < 90.0;  

            if (useLight)
            {
                _plcCom.SetLight(true);

                Thread.Sleep(100);

                var imageWithLight = ReadImage();

                var refImageLight = GetRefImage(imageWithLight);

                _plcCom.SetLight(false);

                if (imageWithLight != null)
                    //imageWithLight.SaveImage(@"Images\" + $"{ImageCounter}_3_inputImageLight.jpg");

                if (refImageLight != null)
                {
                    //refImageLight.SaveImage(@"Images\" + $"{ImageCounter}_3_refImageLight.jpg");
                    refImage = refImageLight;
                }

            }

            return refImage == null ? null : GetStones(refImage, validPositions, tolerance);
        }

        public void Initialize()
        {
            _camera = VideoCapture.FromCamera(0, VideoCaptureAPIs.DSHOW);
            _camera.Set(VideoCaptureProperties.FrameWidth, 10000);
            _camera.Set(VideoCaptureProperties.FrameHeight, 10000);
            _camera.Set(VideoCaptureProperties.Fps, 30);

            if (!_camera.IsOpened())
            {
                _log.Error("Kamera ist nicht angeschlossen");
                return;
            }
            var dummy = ReadImage();
            dummy.Dispose();

        }

        public (double x, double y) PixelsToPosition(double x, double y)
        {
            return (x / ScreenWidth * _screenWidthmm, y / ScreenHeight * _screenHeightmm);
        }

        public (double x, double y) PositionToPixels(double x, double y)
        {
            return (x / _screenWidthmm * ScreenWidth, y / _screenHeightmm * ScreenHeight);
        }

        static Mat DrawCircles(Mat image, CircleSegment[] circles)
        {
            // Kopiere das Bild, um es nicht direkt zu ändern
            Mat imgWithCircles = image.Clone();

            // Zeichne jeden erkannten Kreis
            if (circles != null)
            {
                foreach (var circle in circles)
                {
                    // Zeichne den äußeren Kreis
                    imgWithCircles.Circle((int)Math.Round(circle.Center.X), (int)Math.Round(circle.Center.Y), (int)circle.Radius, Scalar.Red, 2);
                    // Zeichne den Mittelpunkt des Kreises
                    imgWithCircles.Circle((int)Math.Round(circle.Center.X), (int)Math.Round(circle.Center.Y), 3, Scalar.Green, 2);
                }
            }

            return imgWithCircles;
        }

        private IEnumerable<Stone> GetStones(Mat image, Dictionary<FieldPosition, (double x, double y)> validPositions, uint tolerance)
        {
            var stones = new List<Stone>();

            Mat gray = image.Normalize(0, 255, NormTypes.MinMax);
            gray.SaveImage(@"Images\" + $"{ImageCounter}_4_grayImage.jpg");


            // Erkennung der Kreise im Bild mithilfe der HoughCircles-Funktion
            var circlesegements = gray.HoughCircles(HoughModes.Gradient, 1, 50, 100, 20, 35, 45);

            // TODO: Alternative mit DetectBlobs ausprobieren?

            Mat detectedCircles = DrawCircles(gray, circlesegements);

            if (circlesegements.Length <= 0)
                return stones;

            //var threshold = 70;
            var threshold = FindThreshold(circlesegements, gray);

            foreach (var segemnt in circlesegements)
            {
                var point = segemnt.Center;
                var fieldpos = ToValidPos(point, tolerance, validPositions);
                var value = gray.At<byte>((int)point.Y, (int)point.X);
                if (value > threshold)
                {
                    stones.Add(new Stone(point.X, point.Y, Player.White, fieldpos));
                }
                else
                {
                    stones.Add(new Stone(point.X, point.Y, Player.Black, fieldpos));
                }
            }

            var black = new Scalar(0);
            var white = new Scalar(255);
            foreach (Stone stone in stones)
            {
                detectedCircles.PutText(stone.Player.ToString(), new Point(stone.X, stone.Y), HersheyFonts.HersheyComplex, 1, stone.Player == Player.Black ? white : black);
            }

            detectedCircles.SaveImage(@"Images\" + $"{ImageCounter}_5_detectedCircles.jpg");

            return stones;
        }

        private int FindThreshold(CircleSegment[] circlesegements, Mat grayscaleImage)
        {
            // Grauwerte finden
            List<int> values = new List<int>();

            foreach (var segemnt in circlesegements)
            {
                var point = segemnt.Center;
                var value = grayscaleImage.At<byte>((int)point.Y, (int)point.X);

                values.Add(value);
            }

            // werte aufsteigend sortieren
            values.Sort();

            int maxGap = 0;
            int maxGapIndex = 0;
            if (values.Count > 1)
            {
                for (int i = 0; i < values.Count - 1; i++)
                {
                    int gap = values[i + 1] - values[i];
                    if (gap > maxGap)
                    {
                        maxGap = gap;
                        maxGapIndex = i;
                    }
                }
            }

            // default maxGap bei dunkelheit ist 40 (30-60 | 100-170)
            // default threshold ist also bei ca. 80
            int threshold = 80;

            if (maxGap < 30)
            {
                // threshold auf kleinsten bzw größten wert legen, wenn nur gleichfarbige Steine auftreten
                if (values.Max() > threshold)
                    // nur weiße steine
                    threshold = values.Min()-1;
                else if (values.Min() < threshold)
                    threshold = values.Max();
            }
            else
            {
                // threshold in die Mitte der Werte legen, wo der Sprung auftritt
                threshold = values[maxGapIndex] + maxGap / 2;
            }

            return threshold;
        }

        public FieldPosition ToValidPos(Point2f point, uint tolerance, Dictionary<FieldPosition, (double x, double y)> validPositions)
        {

            if (validPositions == null)
                return null;

            foreach (var (pos, (vx, vy)) in validPositions)
            {
                if (Math.Abs(point.X - vx) < tolerance && Math.Abs(point.Y - vy) < tolerance)
                {
                    return pos;
                }
            }

            return null;
        }

        private Mat GetRefImage(Mat image)
        {
            var dictonary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict5X5_50);
            var arucoParams = DetectorParameters.Create();

            CvAruco.DetectMarkers(image, dictonary, out var corners, out var ids, arucoParams, out var rejectedPoints);

            if (ids.Length != 4)
            {
                _log.Error("Nicht alle Referenzpunkte wurden erkannt");
                return null;
            }

            // get the positions of the aruco codes
            Point2f topRightCorner = new();
            Point2f topLeftCorner = new();
            Point2f bottomLeftCorner = new();
            Point2f bottomRightCorner = new();

            for (int i = 0; i < 4; i++)
            {
                var id = ids[i];
                switch (id)
                {
                    case 0:
                        topLeftCorner = corners[i][0];
                        break;

                    case 1:
                        topRightCorner = corners[i][1];
                        break;

                    case 2:
                        bottomRightCorner = corners[i][2];
                        break;

                    case 3:
                        bottomLeftCorner = corners[i][3];
                        break;
                    default:
                        break;
                }
            }

            // transform the image to the display size
            var outImg = new Mat();
            image.CopyTo(outImg);
            var srcPoints = new List<Point2f>()
            {
                new (topLeftCorner.X, topLeftCorner.Y),
                new (topRightCorner.X, topRightCorner.Y),
                new (bottomRightCorner.X, bottomRightCorner.Y),
                new (bottomLeftCorner.X,bottomLeftCorner.Y),
            };

            var offset = 20;
            var destPoints = new List<Point2f>()
            {
                new (offset,offset),
                new ((float)ScreenWidth - offset,offset),
                new ((float)ScreenWidth - offset, (float)ScreenHeight -offset),
                new (offset, (float)ScreenHeight -offset),
            };


            var matrix = Cv2.GetPerspectiveTransform(srcPoints, destPoints);
            Cv2.WarpPerspective(outImg, outImg, matrix, new Size(ScreenWidth, ScreenHeight));

            outImg = DistortImage(outImg);

            return outImg;
        }

        float squareEdgeLength = 0.0375f;

        protected List<List<Point3f>> GetObjectPoints(Size imageSize, float squareEdgeLength)
        {
            List<List<Point3f>> objectPoints = new List<List<Point3f>>();


            var list = new List<Point3f>();
            for (int i = 0; i < imageSize.Height; i++)
            {
                for (int j = 0; j < imageSize.Width; j++)
                {
                    var point = new Point3f(j * squareEdgeLength, i * squareEdgeLength, 0.0f);
                    list.Add(point);
                }
            }
            objectPoints.Add(list);


            return objectPoints;
        }

        public bool CalibrateCamera(Size patternSize, out Mat chessboardCornerImage, out Mat undistortedImage)
        {
            Mat lastImage = new Mat();

            double[,] sumCameraMatrix = new double[3, 3];
            double[] sumDistCoeffs = new double[5];

            // get CameraMatrices and DistortionCoefficients
            var imageCount = 20;
            for (int i = 0; i < imageCount; i++)
            {
                var newImage = ReadImage(false);
                if (newImage != null && !newImage.Empty())
                {
                    var gray = newImage.CvtColor(ColorConversionCodes.BGR2GRAY);
                    newImage = gray;
                }
                //var image = Cv2.ImRead(@"C:\Users\danie\OneDrive - Fachhochschule Bielefeld\06_Semester\02_DMS_Dokumentation Mechatronischer Systeme\IGOR\Fotos & Videos\Fotos Debug\Calibration_Input.png");

                if (newImage == null | newImage.Empty())
                {
                    chessboardCornerImage = new Mat();
                    undistortedImage = new Mat();
                    _log.Error("Kamera konnte nicht kalibriert werden, da kein Bild vorhanden ist.");
                    return false;
                }

                lastImage = newImage;
                Size imageSize = newImage.Size();
                List<List<Point2f>> imagePoints = new List<List<Point2f>>();
                List<List<Point3f>> objectPoints = GetObjectPoints(patternSize, squareEdgeLength);


                //Cv2.ImWrite("Calibration_Input.png", image);  
                Point2f[] corners;
                bool found = Cv2.FindChessboardCorners(newImage, patternSize, out corners);

                if (found)
                {
                    TermCriteria criteria = new TermCriteria(CriteriaTypes.MaxIter, 30, 0.1);
                    var subCorners = Cv2.CornerSubPix(newImage, corners, new Size(10, 10), new Size(-1, -1), criteria);
                    if (subCorners?.Length > 0)
                    {
                        // PYTHON:  img_points.append(corners2)
                        imagePoints.Add(subCorners.ToList());
                    }
                    else
                    {
                        imagePoints.Add(corners.ToList());
                    }


                    Cv2.DrawChessboardCorners(newImage, patternSize, corners, found);
                    //Cv2.ImWrite("Calibration_ChessboardCorners.png", image);


                    double[,] cameraMatrix = new double[3, 3];
                    double[] distCoeffs = new double[5];


                    Vec3d[] rvecs = new Vec3d[] { };
                    Vec3d[] tvecs = new Vec3d[] { };

                    var ret = Cv2.CalibrateCamera(objectPoints, imagePoints, imageSize,
                                                    cameraMatrix, distCoeffs, out rvecs, out tvecs, CalibrationFlags.FixPrincipalPoint);


                    for (int m = 0; m < 3; m++)
                    {
                        for (int n = 0; n < 3; n++)
                        {
                            sumCameraMatrix[m, n] += cameraMatrix[m, n];
                        }
                    }

                    for (int x = 0; x < 5; x++)
                    {
                        sumDistCoeffs[x] += distCoeffs[x];
                    }
                }
            }

            Mat sumCameraMatrixMat = new Mat(3, 3, MatType.CV_64F, sumCameraMatrix);
            Mat sumDistCoeffsMat = new Mat(5, 1, MatType.CV_64F, sumDistCoeffs);


            Mat averageCameraMatrixMat = sumCameraMatrixMat.Divide((float)imageCount);
            Mat averageDistCoeffsMat = sumDistCoeffsMat.Divide((float)imageCount);

            double[] cammat;
            averageCameraMatrixMat.GetArray<double>(out cammat);
            double[] distco;
            averageDistCoeffsMat.GetArray<double>(out distco);

            CameraMatrix = averageCameraMatrixMat;
            DistortionCoefficients = averageDistCoeffsMat;

            Mat resultImage = new Mat();
            Cv2.Undistort(lastImage, resultImage, CameraMatrix, DistortionCoefficients);
            Cv2.ImWrite("Calibration_UndistortedImage.png", resultImage);
            Mat subtractImages = new Mat();
            if (resultImage.Total() == lastImage.Total())
            {
                Cv2.Subtract(resultImage, lastImage, subtractImages);
                Cv2.ImWrite("Calibration_SubtractedImages.png", resultImage);
                undistortedImage = resultImage;
            }
            else
                undistortedImage = new Mat();

            chessboardCornerImage = lastImage;

            return true;
        }


        private Mat DistortImage(Mat image)
        {
            //Mat inputImage = Cv2.ImRead(@"C:\Users\danie\OneDrive - Fachhochschule Bielefeld\06_Semester\02_DMS_Dokumentation Mechatronischer Systeme\IGOR\Fotos & Videos\Fotos Debug\Images\25_undistortedImage.jpg");

            image = image.CvtColor(ColorConversionCodes.BGR2GRAY);

            // wird das nach oder vor RefImage ausgeführt?
            double f_x = image.Width;
            double f_y = image.Height * 1.1;
            double c_x = image.Width / 2.0;
            double c_y = image.Height / 2.0;
            double[,] cameraMatrix = new double[3, 3];
            cameraMatrix[0, 0] = f_x;
            cameraMatrix[1, 1] = f_y;
            cameraMatrix[0, 2] = c_x;
            cameraMatrix[1, 2] = c_y;
            cameraMatrix[2, 2] = 1;

            double[] distortionCoefficients = new double[] { 0.0008, 0.2, 0, 0, 0 };

            Mat cameraMatrixMat = new Mat(3, 3, MatType.CV_64F, cameraMatrix);
            Mat distortionCoeffsMat = new Mat(5, 1, MatType.CV_64F, distortionCoefficients);

            Mat distortedImage = new Mat();
            Cv2.Undistort(image, distortedImage, cameraMatrixMat, distortionCoeffsMat);

            return distortedImage;

            //cnt++;
            //distortedImage.ImWrite(@"C:\Users\danie\OneDrive - Fachhochschule Bielefeld\06_Semester\02_DMS_Dokumentation Mechatronischer Systeme\IGOR\Fotos & Videos\Fotos Debug\Test_Distortion\" + $"{cnt}_distortedPicture.png");


        }

        public void SetScreenSize(double width, double height)
        {
            ScreenWidth = width;
            ScreenHeight = height;
            CalculateScreenDimensions();
        }
        private void CalculateScreenDimensions()
        {
            var ar = ScreenWidth / ScreenHeight;
            _screenHeightmm = ScreenDiagonal / Math.Sqrt(ar * ar + 1) * 25.4;
            _screenWidthmm = ar * _screenHeightmm;
        }

    }
}





//public bool CalibrateCamera(Size patternSize, out Mat chessboardCornerImage, out Mat undistortedImage)
//{
//    List<Mat> images = new List<Mat>();
//    var imageCount = 20;
//    for (int i = 0; i < imageCount; i++)
//    {
//        var newImage = ReadImage(false);
//        if (newImage != null && !newImage.Empty())
//        {
//            var gray = newImage.CvtColor(ColorConversionCodes.BGR2GRAY);
//            images.Add(gray);
//        }
//    }
//    //var image = Cv2.ImRead(@"C:\Users\danie\OneDrive - Fachhochschule Bielefeld\06_Semester\02_DMS_Dokumentation Mechatronischer Systeme\IGOR\Fotos & Videos\Fotos Debug\Calibration_Input.png");

//    if (images.Count <= 0)
//    {
//        chessboardCornerImage = new Mat();
//        undistortedImage = new Mat();
//        _log.Error("Kamera konnte nicht kalibriert werden, da kein Bild vorhanden ist.");
//        return false;
//    }

//    Size imageSize = images.FirstOrDefault().Size();
//    List<List<Point2f>> imagePoints = new List<List<Point2f>>();
//    List<List<Point3f>> objectPoints = GetObjectPoints(patternSize, squareEdgeLength);

//    List<double[,]> cameraMatrices = new List<double[,]>();
//    List<double[]> distortionCoefficients = new List<double[]>();

//    foreach (Mat image in images)
//    {
//        //Cv2.ImWrite("Calibration_Input.png", image);  
//        Point2f[] corners;
//        bool found = Cv2.FindChessboardCorners(image, patternSize, out corners);

//        if (found)
//        {
//            TermCriteria criteria = new TermCriteria(CriteriaTypes.MaxIter, 30, 0.1);
//            var subCorners = Cv2.CornerSubPix(image, corners, new Size(10, 10), new Size(-1, -1), criteria);
//            if (subCorners?.Length > 0)
//            {
//                // PYTHON:  img_points.append(corners2)
//                imagePoints.Add(subCorners.ToList());
//            }
//            else
//            {
//                imagePoints.Add(corners.ToList());
//            }


//            Cv2.DrawChessboardCorners(image, patternSize, corners, found);
//        }
//        //Cv2.ImWrite("Calibration_ChessboardCorners.png", image);


//        double[,] cameraMatrix = new double[3, 3];
//        double[] distCoeffs = new double[5];


//        Vec3d[] rvecs = new Vec3d[] { };
//        Vec3d[] tvecs = new Vec3d[] { };

//        var ret = Cv2.CalibrateCamera(objectPoints, imagePoints, imageSize,
//                                        cameraMatrix, distCoeffs, out rvecs, out tvecs);

//    }
//    distCoeffs[0] = 1.4;
//    distCoeffs[1] = -14.0;
//    distCoeffs[2] = 0;
//    distCoeffs[3] = 0;
//    distCoeffs[4] = 0;

//    Mat cameraMatrixMat = new Mat(3, 3, MatType.CV_64F, cameraMatrix);
//    Mat distCoeffsMat = new Mat(5, 1, MatType.CV_64F, distCoeffs);

//    DistortionCoefficients = distCoeffsMat;
//    CameraMatrix = cameraMatrixMat;

//    Mat resultImage = new Mat();
//    Cv2.Undistort(images.LastOrDefault(), resultImage, cameraMatrixMat, distCoeffsMat);
//    Cv2.ImWrite("Calibration_UndistortedImage.png", resultImage);
//    Mat subtractImages = new Mat();
//    if (resultImage.Total() == images.LastOrDefault().Total())
//    {
//        Cv2.Subtract(resultImage, images.LastOrDefault(), subtractImages);
//        Cv2.ImWrite("Calibration_SubtractedImages.png", resultImage);
//        undistortedImage = resultImage;
//    }
//    else
//        undistortedImage = new Mat();

//    chessboardCornerImage = images.LastOrDefault();

//    return true;
//}
