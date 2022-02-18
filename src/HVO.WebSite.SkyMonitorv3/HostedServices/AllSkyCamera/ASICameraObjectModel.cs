using HVO.Hardware.Camera.ASISDK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HVO.WebSite.SkyMonitorv3.HostedServices.AllSkyCamera
{
    public static class ASICameras
    {
        private static readonly Camera[] _cameras = new Camera[16]; //16

        public static int Count
        {
            get { return ASICameraDLL2.GetNumOfConnectedCameras(); }
        }

        public static Camera GetCameraByIndex(int cameraIndex)
        {
            if (cameraIndex >= Count || cameraIndex < 0)
                throw new IndexOutOfRangeException();

            ASICameraDLL2.ASI_CAMERA_INFO_32 infoTemp = ASICameraDLL2.GetCameraProperties32(cameraIndex);
            int cameraId = infoTemp.CameraID;
            return _cameras[cameraId] ?? (_cameras[cameraId] = new Camera(cameraId));
        }

    }
    public enum ASI_STATUS
    {
        CLOSED = 0,
        OPENED,
        VIDEO,
        SNAP
    }
    public class Camera
    {
        private readonly int _cameraId;
        private string _cachedName = string.Empty;
        private List<CameraControl> _controls = null;
        private ASICameraDLL2.ASI_CAMERA_INFO_32? _info;
        private ASI_STATUS _status;
        public Camera(int cameraId)
        {
            _cameraId = cameraId;
        }

        private ASICameraDLL2.ASI_CAMERA_INFO_32 Info
        {
            // info is cached only while camera is open
            get { return _info ?? ASICameraDLL2.GetCameraProperties32(_cameraId); }
        }

        public ASI_STATUS status
        {
            get { return _status; }
        }

        public string Name
        {
            get { return Info.Name; }
        }

        public bool IsColor
        {
            get { return Info.IsColorCam != ASICameraDLL2.ASI_BOOL.ASI_FALSE; }
        }

        public bool HasST4
        {
            get { return Info.ST4Port != ASICameraDLL2.ASI_BOOL.ASI_FALSE; }
        }

        public bool HasShutter
        {
            get { return Info.MechanicalShutter != ASICameraDLL2.ASI_BOOL.ASI_FALSE; }
        }

        public bool HasCooler
        {
            get { return Info.IsCoolerCam != ASICameraDLL2.ASI_BOOL.ASI_FALSE; }
        }

        public bool IsUSB3
        {
            get { return Info.IsUSB3Host != ASICameraDLL2.ASI_BOOL.ASI_FALSE; }
        }

        public int CameraId
        {
            get { return _cameraId; }
        }

        public ASICameraDLL2.ASI_BAYER_PATTERN BayerPattern
        {
            get { return Info.BayerPattern; }
        }

        public Size Resolution
        {
            get
            {
                var info = Info;
                return new Size((int)info.MaxWidth, (int)info.MaxHeight);
            }
        }

        public double PixelSize
        {
            get { return Info.PixelSize; }
        }

        public List<int> SupportedBinFactors
        {
            get { return Info.SupportedBins.TakeWhile(x => x != 0).ToList(); }
        }

        public List<ASICameraDLL2.ASI_IMG_TYPE> SupportedImageTypes
        {
            get
            {

                return Info.SupportedVideoFormat.TakeWhile(x => x != ASICameraDLL2.ASI_IMG_TYPE.ASI_IMG_END).ToList();
            }
        }

        public ASICameraDLL2.ExposureStatus ExposureStatus
        {
            get
            {

                ASICameraDLL2.ExposureStatus status = ASICameraDLL2.GetExposureStatus(_cameraId);
                if (status != ASICameraDLL2.ExposureStatus.ExpWorking)
                    _status = ASI_STATUS.OPENED;
                return status;
            }
        }

        public void OpenCamera()
        {
            ASICameraDLL2.OpenCamera(_cameraId);
            _info = ASICameraDLL2.GetCameraProperties32(_cameraId);
            ASICameraDLL2.InitCamera(_cameraId);
            _status = ASI_STATUS.OPENED;
        }

        public void CloseCamera()
        {
            _info = null;
            _controls = null;
            ASICameraDLL2.CloseCamera(_cameraId);
            _status = ASI_STATUS.CLOSED;
        }

        public List<CameraControl> Controls
        {
            get
            {
                if (_controls == null || _cachedName != Name)
                {
                    _cachedName = Name;
                    int cc = ASICameraDLL2.GetNumOfControls(_cameraId);
                    _controls = new List<CameraControl>();
                    for (int i = 0; i < cc; i++)
                    {
                        _controls.Add(new CameraControl(_cameraId, i));
                    }
                }

                return _controls;
            }
        }

        public Point StartPos
        {
            get { return ASICameraDLL2.GetStartPos(_cameraId); }
            set { ASICameraDLL2.SetStartPos(_cameraId, value); }
        }

        public CaptureAreaInfo CaptureAreaInfo
        {
            get
            {
                int bin;
                ASICameraDLL2.ASI_IMG_TYPE imageType;
                var res = ASICameraDLL2.GetROIFormat(_cameraId, out bin, out imageType);
                return new CaptureAreaInfo(res, bin, imageType);
            }
            set
            {
                ASICameraDLL2.SetROIFormat(_cameraId, value.Size, value.Binning, value.ImageType);
            }
        }

        public int DroppedFrames
        {
            get { return ASICameraDLL2.GetDroppedFrames(_cameraId); }
        }

        public bool EnableDarkSubtract(string darkImageFilePath)
        {
            return ASICameraDLL2.EnableDarkSubtract(_cameraId, darkImageFilePath);
        }

        public void DisableDarkSubtract()
        {
            ASICameraDLL2.DisableDarkSubtract(_cameraId);
        }

        public void StartVideoCapture()
        {
            ASICameraDLL2.StartVideoCapture(_cameraId);
            _status = ASI_STATUS.VIDEO;
        }

        public void StopVideoCapture()
        {
            ASICameraDLL2.StopVideoCapture(_cameraId);
            _status = ASI_STATUS.OPENED;
        }

        public bool GetVideoData(IntPtr buffer, long bufferSize, int waitMs)
        {
            return ASICameraDLL2.GetVideoData(_cameraId, buffer, bufferSize, waitMs);
        }

        public void PulseGuideOn(ASICameraDLL2.ASI_GUIDE_DIRECTION direction)
        {
            ASICameraDLL2.PulseGuideOn(_cameraId, direction);
        }

        public void PulseGuideOff(ASICameraDLL2.ASI_GUIDE_DIRECTION direction)
        {
            ASICameraDLL2.PulseGuideOff(_cameraId, direction);
        }

        public void StartExposure(bool isDark)
        {
            ASICameraDLL2.StartExposure(_cameraId, isDark);
            _status = ASI_STATUS.SNAP;
        }

        public void StopExposure()
        {
            ASICameraDLL2.StopExposure(_cameraId);
        }

        public bool GetExposureData(IntPtr buffer, int bufferSize)
        {
            return ASICameraDLL2.GetDataAfterExp(_cameraId, buffer, bufferSize);
        }

        public CameraControl GetControl(ASICameraDLL2.ASI_CONTROL_TYPE controlType)
        {
            return Controls.FirstOrDefault(x => x.ControlType == controlType);
        }
    }

    public class CaptureAreaInfo
    {
        public Size Size { get; set; }
        public int Binning { get; set; }
        public ASICameraDLL2.ASI_IMG_TYPE ImageType { get; set; }

        public CaptureAreaInfo(Size size, int binning, ASICameraDLL2.ASI_IMG_TYPE imageType)
        {
            Size = size;
            Binning = binning;
            ImageType = imageType;
        }
    }

    public class CameraControl
    {
        private readonly int _cameraId;
        private ASICameraDLL2.ASI_CONTROL_CAPS_32 _props;
        private bool _auto;

        public CameraControl(int cameraId, int controlIndex)
        {
            _cameraId = cameraId;

            _props = ASICameraDLL2.GetControlCaps32(_cameraId, controlIndex);
            _auto = GetAutoSetting();

            Console.WriteLine($"Control Name: {_props.Name}, ControlType: {_props.ControlType}, DefaultValue: {_props.DefaultValue}, MinValue: {_props.MinValue}, MaxValue: {_props.MaxValue}, IsAutoSupported: {_props.IsAutoSupported}, Writeable: {_props.IsWritable}, Description: {_props.Description}");
        }

        public string Name { get { return _props.Name; } }
        public string Description { get { return _props.Description; } }
        public int MinValue { get { return (int)_props.MinValue; } }
        public int MaxValue { get { return (int)_props.MaxValue; } }
        public int DefaultValue { get { return (int)_props.DefaultValue; } }
        public ASICameraDLL2.ASI_CONTROL_TYPE ControlType { get { return _props.ControlType; } }
        public bool IsAutoAvailable { get { return _props.IsAutoSupported != ASICameraDLL2.ASI_BOOL.ASI_FALSE; } }
        public bool Writeable { get { return _props.IsWritable != ASICameraDLL2.ASI_BOOL.ASI_FALSE; } }

        public int Value
        {
            get
            {
                bool isAuto;
                return (int)ASICameraDLL2.GetControlValue32(_cameraId, _props.ControlType, out isAuto);
            }
            set
            {
                ASICameraDLL2.SetControlValue32(_cameraId, _props.ControlType, value, IsAuto);
            }
        }

        public bool IsAuto
        {
            get
            {
                return _auto;
            }
            set
            {
                _auto = value;
                ASICameraDLL2.SetControlValue32(_cameraId, _props.ControlType, Value, value);
            }
        }

        private bool GetAutoSetting()
        {
            bool isAuto;
            ASICameraDLL2.GetControlValue32(_cameraId, _props.ControlType, out isAuto);
            return isAuto;
        }
    }
}
