using System.Collections.Generic;

namespace ValetParkingDAL.Models.NumberPlateRecogModels
{
    public class CameraFrameResponse
    {
        public double processing_time_ms { get; set; }
        public string data_type { get; set; }
        public string uuid { get; set; }
        public int version { get; set; }
        public long camera_id { get; set; }
        public List<FrameResult> results { get; set; }
        public string agent_version { get; set; }
        public bool error { get; set; }
        public string company_id { get; set; }
        public long epoch_time { get; set; }
        public string agent_uid { get; set; }
        public int img_width { get; set; }
        public List<RegionsOfInterest> regions_of_interest { get; set; }
        public string agent_type { get; set; }
        public int img_height { get; set; }

    }


    public class FrameResult
    {
        public int plate_index { get; set; }
        public List<Coordinate> coordinates { get; set; }
        public double processing_time_ms { get; set; }
        public string plate_crop_jpeg { get; set; }
        public string plate { get; set; }
        public VehicleRegion vehicle_region { get; set; }
        public string region { get; set; }
        public double confidence { get; set; }
        public int matches_template { get; set; }
        public int requested_topn { get; set; }
        public List<Candidate> candidates { get; set; }
        public int region_confidence { get; set; }
    }
}