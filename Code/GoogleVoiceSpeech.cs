//	Copyright (c) 2016 steele of lowkeysoft.com
//        http://lowkeysoft.com
//
//	This software is provided 'as-is', without any express or implied warranty. In
//	no event will the authors be held liable for any damages arising from the use
//	of this software.
//
//	Permission is granted to anyone to use this software for any purpose,
//	including commercial applications, and to alter it and redistribute it freely,
//	subject to the following restrictions:
//
//	1. The origin of this software must not be misrepresented; you must not claim
//	that you wrote the original software. If you use this software in a product,
//	an acknowledgment in the product documentation would be appreciated but is not
//	required.
//
//	2. Altered source versions must be plainly marked as such, and must not be
//	misrepresented as being the original software.
//
//	3. This notice may not be removed or altered from any source distribution.
//
//  =============================================================================
//
// Acquired from https://github.com/steelejay/LowkeySpeech
//
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
//using coti_VR;
using UnityEngine.Networking;
using System.IO;


[RequireComponent(typeof(AudioSource))]

public class GoogleVoiceSpeech : MonoBehaviour
{

    const int HEADER_SIZE = 44;

    private int minFreq;
    private int maxFreq;

    string Response;
    public static string resultString = null;

    private bool micConnected = false;

    //A handle to the attached AudioSource
    public static AudioSource goAudioSource;
    public Text res; //references to text Obj
    public static Text res_;

    public static string filePath;

    // Use this for initialization
    void Start()
    {

        res_ = res;
        //Check if there is at least one microphone connected
        if (Microphone.devices.Length <= 0)
        {
            //Throw a warning message at the console if there isn't
            Debug.LogWarning("Microphone not connected!");
        }
        else //At least one microphone is present
        {
            //Set 'micConnected' to true
            micConnected = true;

            //Get the default microphone recording capabilities
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);

            //According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...
            if (minFreq == 0 && maxFreq == 0)
            {
                //...meaning 44100 Hz can be used as the recording sampling rate
                maxFreq = 44100;
            }

            //Get the attached AudioSource component
            goAudioSource = this.GetComponent<AudioSource>();
        }
    }



    public void BeginRecord()
    {
        if (micConnected)
        {
            if (!Microphone.IsRecording(null))
            {
                //Start recording and store the audio captured from the microphone at the AudioClip in the AudioSource
                goAudioSource.clip = Microphone.Start(null, true, 7, maxFreq); //Currently set for a 7 second clip
            }
        }
        else
        {
            Debug.LogError("Microphone not connected!");
        }

    }

    public void StopAndSend()
    {
        StartCoroutine(ConvertAndSend());
        //ConvertAndSend();
    }

    IEnumerator ConvertAndSend()
    {
        float filenameRand = UnityEngine.Random.Range(0.0f, 10.0f);
        string filename = "recordAudio" + filenameRand;
        Microphone.End(null); //Stop the audio recording
        Debug.Log("Recording Stopped");

        if (!filename.ToLower().EndsWith(".wav"))
        {
            filename += ".wav";
        }

        filePath = Path.Combine("recordings/", filename);
        filePath = Path.Combine(Application.persistentDataPath, filePath);

        SavWav sav = new SavWav(filePath, goAudioSource.clip);
        sav.Start();
        yield return sav.WaitFor();

        //Debug.Log("Saving @ " + filePath);
        string apiURL = "YOUR API KEY"; //put your api voice key
        //Debug.Log("Uploading " + filePath);


        StartCoroutine(HttpUploadFile(apiURL, filePath, "file", "audio/wav; rate=44100"));
    }

    public IEnumerator HttpUploadFile(string url, string file, string paramName, string contentType)
    {

        System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
        Debug.Log(string.Format("Uploading {0} to {1}", file, url));

        Byte[] bytes = File.ReadAllBytes(file);
        String file64 = null;
        file64 = Convert.ToBase64String(bytes, Base64FormattingOptions.None);
        while (file64 == null)
        {
            yield return null;
        }

        var json = "{ \"config\": { \"languageCode\" : \"en-US\" }, \"audio\" : { \"content\" : \"" + file64 + "\"}}";
        var uwr = new UnityWebRequest(url, "POST");

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            Debug.Log("Response String: " + uwr.downloadHandler.text);

            var jsonresponse = SimpleJSON.JSON.Parse(uwr.downloadHandler.text);

            if (jsonresponse != null)
            {
                resultString = jsonresponse["results"][0]["alternatives"][0]["transcript"];
                Mission.alreadyDO = false;
                res.text = "voce disse: " + resultString;

            }
            //Playback the recorded audio
            File.Delete(file); //Delete the Temporary Wav file



        }
    }
}
