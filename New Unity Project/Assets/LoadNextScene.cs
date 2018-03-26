using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System;
using System.Net;
using UnityEngine.Networking;

public class LoadNextScene : MonoBehaviour {

    public void Search ()
    {
        string location = Input.compositionString;

        StartCoroutine(SearchLocation(location));
    }

    IEnumerator SearchLocation(string location)
    {
        double lat;
        double lng;
        string url = "https://maps.googleapis.com/maps/api/streetview/metadata?location=" + location + "&key=AIzaSyAr511nsGmQKDgZA-qmBVwXObp1m2KoDAo";

        using (WWW www = new WWW(url))
        {
            yield return www;
            if (www.error == null)
            {
                SearchObject searchObject = JsonConvert.DeserializeObject<SearchObject>(www.text);

                if (object.Equals(searchObject.status, "OK"))
                {
                    lat = searchObject.location.lat;
                    lng = searchObject.location.lng;
                    url = "https://cbks0.google.com/cbk?cb_client=apiv3&authuser=0&hl=en&output=polygon&it=1%3A1&rank=closest&ll=" + lat + "," + lng + "&radius=50";

                    using (WWW www2 = new WWW(url))
                    {
                        yield return www2;
                        if (www.error == null)
                        {
                            PanoObject panoObject = JsonConvert.DeserializeObject<PanoObject>(www2.text);
                            if(panoObject.result[0].id != null)
                            {
                                PanoID.SetPanoID(panoObject.result[0].id);
                                SceneManager.LoadScene(1);
                            }
                        }
                        else
                        {
                            Debug.Log("ERROR: " + www.error);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("ERROR: " + www.error);
            }
        }
    }

    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class SearchObject
    {
        public string copyright { get; set; }
        public string date { get; set; }
        public Location location { get; set; }
        public string pano_id { get; set; }
        public string status { get; set; }
    }

    public class CameraRotation
    {
        public double heading { get; set; }
        public double tilt { get; set; }
        public double roll { get; set; }
    }

    public class Result
    {
        public string id { get; set; }
        public double score { get; set; }
        public double yaw { get; set; }
        public int image_type { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public CameraRotation camera_rotation { get; set; }
    }

    public class PanoObject
    {
        public List<Result> result { get; set; }
    }
}
