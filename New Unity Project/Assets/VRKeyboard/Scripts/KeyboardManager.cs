/***
 * Author: Yunhan Li 
 * Any issue please contact yunhn.lee@gmail.com
 ***/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

namespace VRKeyboard.Utils
{
    public class KeyboardManager : MonoBehaviour
    {
        #region Public Variables
        [Header("User defined")]
        [Tooltip("If the character is uppercase at the initialization")]
        public bool isUppercase = false;
        public int maxInputLength;

        [Header("UI Elements")]
        public Text inputText;

        [Header("Essentials")]
        public Transform characters;
        #endregion

        #region Private Variables
        private string Input
        {
            get { return inputText.text; }
            set { inputText.text = value; }
        }

        private Dictionary<GameObject, Text> keysDictionary = new Dictionary<GameObject, Text>();

        private bool capslockFlag;
        #endregion

        #region Monobehaviour Callbacks
        private void Awake()
        {

            for (int i = 0; i < characters.childCount; i++)
            {
                GameObject key = characters.GetChild(i).gameObject;
                Text _text = key.GetComponentInChildren<Text>();
                keysDictionary.Add(key, _text);

                key.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GenerateInput(_text.text);
                });
            }

            capslockFlag = isUppercase;
            CapsLock();
        }
        #endregion

        #region Public Methods
        public void Backspace()
        {
            if (Input.Length > 0)
            {
                Input = Input.Remove(Input.Length - 1);
            }
            else
            {
                return;
            }
        }

        public void Clear()
        {
            Input = "";
        }

        public void CapsLock()
        {
            if (capslockFlag)
            {
                foreach (var pair in keysDictionary)
                {
                    pair.Value.text = ToUpperCase(pair.Value.text);
                }
            }
            else
            {
                foreach (var pair in keysDictionary)
                {
                    pair.Value.text = ToLowerCase(pair.Value.text);
                }
            }
            capslockFlag = !capslockFlag;
        }
        #endregion

        #region Private Methods
        public void GenerateInput(string s)
        {
            if (Input.Length > maxInputLength) { return; }
            Input += s;
        }

        private string ToLowerCase(string s)
        {
            return s.ToLower();
        }

        private string ToUpperCase(string s)
        {
            return s.ToUpper();
        }
        #endregion

        #region New Methods
        public void Search()
        {
            StartCoroutine(SearchLocation());
        }

        IEnumerator SearchLocation()
        {
            double lat;
            double lng;
            string url = "https://maps.googleapis.com/maps/api/streetview/metadata?location=" + Input + "&key=AIzaSyAr511nsGmQKDgZA-qmBVwXObp1m2KoDAo";

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
                                if (panoObject.result[0].id != null)
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
    #endregion
}