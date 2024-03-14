using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;


public class VideoManager : MonoBehaviour
{
    //Managers
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private SettingsManager settingsManager;
    [SerializeField] private SceneOrderManager2 sceneOrderManager;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private TrackingManager trackingManager;

    //Video Management
    [SerializeField] public VideoPlayer[] VideoPlayers;
    public bool isLoadingNextVideo;
    public VideoClip[] videos;
    public int activePlayerIndex;

    //Clip counting
    public int currentClipCounter;
    public int nextClipCounter;
    private List<int> prevClipCounters;

    //This Video
    [SerializeField] RenderTexture videoTexture;    
    private int startFrame;
    public int endFrame;

    private int nextActivePlayerIndex;

    //private bool isInactivePaused;

  

    private void ResetVideoManager()
    {
        //Reset vars
        isLoadingNextVideo = false;
        //isInactivePaused = false;

        prevClipCounters = new List<int>();

        activePlayerIndex = 0;

        //Reset Video Players
        VideoPlayers[0].gameObject.GetComponent<RawImage>().enabled = true;
        VideoPlayers[1].gameObject.GetComponent<RawImage>().enabled = false;

        VideoPlayers[0].gameObject.GetComponent<VideoPlayer>().clip = null;
        VideoPlayers[1].gameObject.GetComponent<VideoPlayer>().clip = null;
    }

    private void Start()
    {
        videos = Resources.LoadAll<VideoClip>("BWD 2K");//, typeof(VideoClip));
        //Debug.Log($"[{GetType().Name}] Number of video files found : "+ videos.Length);

        ResetVideoManager();
    }

    void Update()
    {
        #if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
        // DEBUG ONLY
        // Press N key to skip to end of current clip
        if (currentClipCounter > 0 && VideoPlayers[activePlayerIndex].isPlaying && Input.GetKeyDown("n"))
        {
            VideoPlayers[activePlayerIndex].frame = (endFrame - 120);

            Debug.Log("N key pressed - Skip towards end of video");
        }
        #endif

        CheckPreloadNextVideo();
    }


    void CheckPreloadNextVideo()
    {
        if (VideoPlayers[activePlayerIndex].isPlaying)
        {
            if (((long)VideoPlayers[activePlayerIndex].frame >= (endFrame - 168)) && !isLoadingNextVideo)
            {
                //Debug.Log($"[{GetType().Name}] Preparing to play another video at end of this video...");

                //Get the nextVideoCounter value
                sceneOrderManager.IncrementScene();

                //Get the start frame for next video
                startFrame = GetTimingsForVideoCounter(nextClipCounter, true);
                //Debug.Log($"[{GetType().Name}] Start Frame: " + startFrame.ToString());

                PreLoadNextVideo();
            }
        }

        if (VideoPlayers[1].isPlaying)
        {
            //Debug.Log(VideoPlayers[1].frame);
        }
    }

    public void PreLoadNextVideo ()
    {
        if (sceneOrderManager.WatchedAllClips())
        {
            //DON'T PRELOAD A VIDEO - WEVE WATCHED THE CREDITS. GO BACK TO START SCREEN

            //Debug.Log($"[{GetType().Name}] <b>Coming to the end of the Video - Bsocial_Reset</b>");
            trackingManager.ResetTrackingManager();

            //AWAKE
            ResetVideoManager();

            //ONSHOW
            ResetAndPlayWhenPrepared();

            //Reset Scene Order
            sceneOrderManager.ResetOrderManager();

            //Return to Settings screen
            viewManager.DisplaySettingsScreen();
        }
        else if ( nextClipCounter < 12 )
        {
            //Set active player index
            SetActivePlayer();
            
            Debug.Log($"[{GetType().Name}] Prepare Video Player " + (nextActivePlayerIndex + 1) + " for use");

            //Set video loading to true
            isLoadingNextVideo = true;

            //Prepare the next video a few seconds before needed
            VideoPlayers[ nextActivePlayerIndex ].clip = videos[ nextClipCounter ];
            VideoPlayers[ nextActivePlayerIndex ].Prepare ();

            //Once the overlap time has ended, swap the video players
            StartCoroutine ( VideoIsEnding () );
        }
    }
    void SetActivePlayer()
    {
        if (activePlayerIndex == 0)
        {
            nextActivePlayerIndex = 1;
        }
        else
        {
            nextActivePlayerIndex = 0;
        }
    }

    IEnumerator VideoIsEnding()
    {
        Debug.Log($"[{GetType().Name}] VideoIsEnding...");

        bool waitingForVideoToEnd = true;

        SetActivePlayer();

        //Debug.Log($"[{GetType().Name}] Check For End Of Video - nextActivePlayerIndex : " + nextActivePlayerIndex);

        while ( waitingForVideoToEnd )
        {
            //Start playing next video
            if ( VideoPlayers[ activePlayerIndex ].frame >= endFrame - startFrame && !VideoPlayers[ nextActivePlayerIndex ].isPlaying )
            {
                VideoPlayers[ nextActivePlayerIndex ].Play ();
                Debug.Log($"[{GetType().Name}] VideoIsEnding - Preload :" +VideoPlayers[nextActivePlayerIndex].name);
            }

            //Once the overlap time has ended or the video stopped playing, swap the video players
            if ( VideoPlayers[ activePlayerIndex ].frame >= endFrame || !VideoPlayers[ activePlayerIndex ].isPlaying )
            {
                //Enable the video player
                VideoPlayers[ activePlayerIndex ].gameObject.GetComponent<RawImage> ().enabled = false;
                VideoPlayers[ nextActivePlayerIndex ].gameObject.GetComponent<RawImage> ().enabled = true;

                //Tidy up
                prevClipCounters.Add ( currentClipCounter );

                isLoadingNextVideo = false;

                waitingForVideoToEnd = false;


                //Prep vars for next video
                endFrame = GetTimingsForVideoCounter(nextClipCounter, false);

                activePlayerIndex = nextActivePlayerIndex;

                currentClipCounter = nextClipCounter;


                //= = = DEBUG LOGS = = =

                //Debug.Log($"[{GetType().Name}] CheckForEndOfVideo - End Frame : " + endFrame.ToString() + " of Frame Count : " + VideoPlayers[activePlayerIndex].clip.frameCount.ToString());

                string orderStr = ""; 
                int count = 0;
                foreach (Scene s in sceneOrderManager.currentSceneOrder)
                {
                    orderStr += s.index;
                    count++;
                    if (count < sceneOrderManager.currentSceneOrder.Count) orderStr += ",";
                }

                Debug.Log($"[{GetType().Name}] VideoIsEnding - Scene Order : " +orderStr+" ("+ sceneOrderManager.currentSceneOrder.Count + " total)");

                if (sceneOrderManager.currentSceneOrder.Count == 0) Debug.LogError($"[{GetType().Name}] VideoIsEnding - currentSceneOrder.Count == 0. NO SCENES ORDERED!");

                Debug.Log($"[{GetType().Name}] VideoIsEnding - <b>Current Video : " + currentClipCounter.ToString()+"</b>");

                if (sceneOrderManager.ClipsRemaining())
                {
                    Debug.Log($"[{GetType().Name}] VideoIsEnding- Next Video : " + sceneOrderManager.currentSceneOrder[sceneOrderManager.currentSceneNumber].index.ToString());
                }
            }

            yield return new WaitForEndOfFrame ();
        }
    }

    public bool isEndOfFirstScene()
    {
        return (long)VideoPlayers[activePlayerIndex].frame >= (endFrame - 288);
    }



    private int GetTimingsForVideoCounter(int val, bool isStart)
    {
        int frameVal = 0;

        switch (val)
        {
            case 0: if (isStart) { frameVal = 0; } else { frameVal = 1320; } break;
            case 1: if (isStart) { frameVal = 149; } else { frameVal = 6675; } break;
            case 2: if (isStart) { frameVal = 0; } else { frameVal = 2489; } break;
            case 3: if (isStart) { frameVal = 12; } else { frameVal = 3865; } break;
            case 4: if (isStart) { frameVal = 6; } else { frameVal = 2379; } break;
            case 5: if (isStart) { frameVal = 43; } else { frameVal = 2743; } break;
            case 6: if (isStart) { frameVal = 0; } else { frameVal = 3177; } break;
            case 7: if (isStart) { frameVal = 44; } else { frameVal = 1525; } break;
            case 8: if (isStart) { frameVal = 65; } else { frameVal = 2097; } break;
            case 9: if (isStart) { frameVal = 3; } else { frameVal = 2806; } break;
            case 10: 
                if (isStart) { frameVal = 63; } 
                else { 
                    //frameVal = 5104; Changed as video shorter than this frame count frameVal = 4968;
                     }
                     break;
            case 11: if (isStart) { frameVal = 0; } else { frameVal = 621; } break;
            case 12: Debug.Log("End Reached"); break;
            default: Debug.LogError("Video counter is outside the video range"); break;
        }

        return frameVal;
    }

    public void ResetAndPlayWhenPrepared()
    {
        currentClipCounter = 0;
        PrepareVideo(currentClipCounter);

        VideoPlayers[activePlayerIndex].prepareCompleted += PlayWhenPrepared;
    }

    private void PrepareVideo(int clipCounter)
    {
        VideoPlayers[activePlayerIndex].clip = videos[clipCounter];

        VideoPlayers[activePlayerIndex].Prepare();

        endFrame = GetTimingsForVideoCounter(currentClipCounter, false);

        Debug.Log($"[{GetType().Name}] Loading & Preparing Video : " + clipCounter.ToString());// + ", End Frame:"+ endFrame.ToString() + ", Frame Count:" + VideoPlayers[activePlayerIndex].clip.frameCount.ToString());
    }

    public void PlayWhenPrepared(VideoPlayer source)
    {
        foreach (VideoPlayer player in VideoPlayers)
        {
            if (player.clip) Debug.Log($"[{GetType().Name}] Play " + player.name + " : clip " + player.clip);

            if (player.isPrepared && (player == VideoPlayers[activePlayerIndex]))// || isInactivePaused ) )
            {
                //Debug.Log($"[{GetType().Name}] Player isPrepared. isInactivePaused? : " + isInactivePaused);

                if (player == VideoPlayers[activePlayerIndex == 0 ? 1 : 0])
                {
                    //isInactivePaused = false;
                    Debug.Log($"[{GetType().Name}] Active Player : " + activePlayerIndex);// + ", isInactivePaused? : " + isInactivePaused);
                }

                if (currentClipCounter == 0)
                {
                    //If first clip analyse
                    trackingManager.BeginAnalysis();
                }
            }

            player.playbackSpeed = 1;
            player.Play();
        }
    }
}
