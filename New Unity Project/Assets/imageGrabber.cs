using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class imageGrabber : MonoBehaviour
{

    private GameObject imagePlaceholder;
    private Texture2D panoTex;
    private int runningCoroutines = 0;
    private bool updateTexture = false;
    private int zoom = 4;
    private int tilex = 13;
    private int tiley = 7;
    private int tileDim = 512;
    private int panoWidth;
    private int panoHeight;

    IEnumerator Start()
    {
        Application.lowMemory += OnLowMemory;

        

        // The number of tiles changes with zoom level (zoom=4, x=12, y=6.  zoom=5, x=25, y=12)
        // Dimensions of final image depend on zoom as well, (416 * 2^zoom)x(416 * 2^(zoom - 1)) (??????)
        
        panoWidth = tilex * tileDim;
        panoHeight = tiley * tileDim - 256;

        panoTex = new Texture2D(panoWidth, panoHeight, TextureFormat.RGB24, false);

        // Starts a new coroutine to get each tile in the equirectangular image (width=26 tiles, height=13 tiles).
        for (int i = 0; i < tiley; i++)
        {
            for (int j = 0; j < tilex; j++)
            {
                // Limits the number of running coroutines to 50. If the limit is reached, wait for 1 second.
                // This prevents running out of memory when textures are being created.
                if (runningCoroutines > 30)
                {
                    yield return new WaitUntil(() => runningCoroutines < 30);
                }

                // Url containing the tiles.
                // PanoID is for the panorama of selected streetview location (this cannot be changed yet, but can be later once navigation is implemented).
                string url = "https://cbk0.google.com/cbk?output=tile&panoid=" + PanoID.GetPanoID() + "&zoom=" + zoom  + "&x=" + j + "&y=" + i;

                StartCoroutine(GetTexture(url, j, i));
                runningCoroutines++;
            }
        }

        yield return null;
    }

    // Gets images from the created urls and places them into textures, which are then placed into another texture.
    IEnumerator GetTexture(string url, int j, int i)
    {
        // Creates web request from url.
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            // Sends the web request and waits for response.
            yield return www.SendWebRequest();

            // If there is an error, starts a new coroutine calling GetTexture with the same parameters.
            if (www.isNetworkError || www.isHttpError)
            {
                StartCoroutine(GetTexture(url, j, i));
            }
            else
            {
                // Creates Temporary texture object to hold the downloaded texture, which is then scaled to lower memory usage.
                Texture2D tex = new Texture2D(4, 4, TextureFormat.RGB24, false);
                tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                //tex = TextureScaler.scaled(tex, 256, 256);

                // Gets the pixels from tex and places them in panoTex at the correct position based on the tile coordinates.
                int x = j * tileDim;
                int y = ((tiley - 1 - i) * tileDim) - 256;
                if (i == 6)
                {
                    y = 0;
                    panoTex.SetPixels(x, y, tileDim, tileDim - 256, tex.GetPixels(0, 256, 512, 256));
                }
                else
                {
                    panoTex.SetPixels(x, y, tileDim, tileDim, tex.GetPixels());
                }

                // Destroy the tex and webrequest object, lowering memory usage.
                Destroy(tex);
                www.Dispose();

                runningCoroutines--;
                updateTexture = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If there are no running coroutines and updateTexture is set to true, display the panorama on sphere.
        if (runningCoroutines == 0 && updateTexture)
        {
            imagePlaceholder = GameObject.Find("Sphere");
            updateTexture = false;

            // Creates a new texture object which loads the jpeg image data from panoTex
            // This only works because panoTex is encoded to JPG format and loaded into finalTex. Encoding to PNG takes much longer from my testing.
            // Using panoTex on the material or just setting finalTex to panoTex doesn't display.
            Texture2D finalTex = new Texture2D(4, 4);
            finalTex.LoadImage(panoTex.EncodeToJPG());
            imagePlaceholder.GetComponent<Renderer>().material.mainTexture = finalTex;
        }
    }

    // Unloads unused assets if the application is low on memory, helps to prevent crashing.
    private void OnLowMemory()
    {
        Resources.UnloadUnusedAssets();
    }
}