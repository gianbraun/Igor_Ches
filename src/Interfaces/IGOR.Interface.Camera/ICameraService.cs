using IGOR.Interface.Game;
using OpenCvSharp;
using System;
using System.Collections.Generic; 
using System.Threading.Tasks;

namespace IGOR.Interface.Camera
{
    public interface ICameraService
    {

        double ScreenWidth { get; }

        double ScreenHeight { get; }

        Mat CameraMatrix { get;  set; }

        Mat DistortionCoefficients { get;  set; }

        void Initialize();

        Mat ReadImage(bool undistortImage = true);

        IEnumerable<Stone> ReadStonePositions(Dictionary<FieldPosition, (double x, double y)> validPositions, uint tolerance);

        bool CalibrateCamera(Size patternSize, out Mat chessboardCornerImage, out Mat imageUndistorted);

        (double x, double y) PixelsToPosition(double x, double y);
        (double x, double y) PositionToPixels(double x, double y);
        public FieldPosition ToValidPos(Point2f point, uint tolerance, Dictionary<FieldPosition, (double x, double y)> validPositions);
         
        void SetScreenSize(double width, double height);
    }
}
