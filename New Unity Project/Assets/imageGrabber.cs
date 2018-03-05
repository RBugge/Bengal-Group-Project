using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class imageGrabber : MonoBehaviour
{

    private GameObject imagePlaceholder;
    private Texture2D panoTex;
    private int runningCoroutines = 0;
    private bool updateTexture = false;

    IEnumerator Start()
    {
        Application.lowMemory += OnLowMemory;

        panoTex = new Texture2D(256 * 26, 256 * 13, TextureFormat.RGB24, false);

        // Starts a new coroutine to get each tile in the equirectangular image (width=26 tiles, height=13 tiles).
        for (int i = 0; i < 13; i++)
        {
            for (int j = 0; j < 26; j++)
            {
                // Limits the number of running coroutines to 50. If the limit is reached, wait for 1 second.
                // This prevents running out of memory when textures are being created.
                if (runningCoroutines > 50)
                {
                    yield return new WaitForSeconds(1);
                }

                // Url containing the tiles.
                // PanoID is for the panorama of selected streetview location (this cannot be changed yet, but can be later once navigation is implemented).
                string url = "https://cbk0.google.com/cbk?output=tile&panoid=lKxUOImSaCYAAAQIt71GFQ&zoom=5&x=" + j + "&y=" + i;

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
                tex = TextureScaler.scaled(tex, 256, 256);

                // Gets the pixels from tex and places them in panoTex at the correct position based on the tile coordinates.
                int x = j * 256;
                int y = (12 - i) * 256;
                panoTex.SetPixels(x, y, 256, 256, tex.GetPixels());

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
        if (runningCoroutines == 0 && updateTexture == true)
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