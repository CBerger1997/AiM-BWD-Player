
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class BSocialUnity
{
	public struct BSocialFaceDetection
	{
		public int x;
		public int y;
		public int width;
		public int height;
		public bool valid;
	};

	public struct BSocialHeadPosition
	{
		public float x;
		public float y;
		public float z;
		public bool valid;
	};

	public struct BSocialHeadAngles
	{
		public float yaw;
		public float pitch;
		public float roll;
		public bool valid;
	};

	public struct BSocialLandmarks
	{
		public List<float> x;
		public List<float> y;
		public bool valid;
	};

	public struct BSocialGazeAngles
	{
		public float yaw;
		public float pitch;
	};

	public struct BSocialGazeVector
	{
		public float x;
		public float y;
		public float z;
	};

	public struct BSocialGaze
	{
		public BSocialGazeAngles angles;
		public BSocialGazeAngles anglesAverage;
		public BSocialGazeVector vector;
		public BSocialGazeVector vectorAverage;
		public bool valid;
	};

	public struct BSocialActionUnits
	{
		public List<string> classes;
		public List<float> predictions;
		public bool valid;
	};

	public struct BSocialAffect
	{
		public float valence;
		public float arousal;
		public float valence_smoothed;
		public float arousal_smoothed;
		public bool valid;
	};

	public struct BSocialImageQuality
	{
		public bool imageBlurry;
		public bool imageNoisy;
		public bool imageTooDark;
		public bool imageTooBright;
		public bool valid;
	};

	public struct BSocialBodyPoints
	{
		public List<float> x;
		public List<float> y;
		public bool valid;
	};

	public struct BSocialPredictions
	{
		public BSocialFaceDetection faceDetection;
		public BSocialLandmarks landmarks;
		public BSocialHeadAngles headAngles;
		public BSocialHeadPosition headPosition;
		public BSocialGaze gaze;
		public BSocialActionUnits actionUnits;
		public BSocialAffect affect;
		public BSocialImageQuality imageQuality;
		public BSocialBodyPoints bodyPoints;
	}

	public enum BSocialWrapper_Rotation : ushort
	{
		BM_NO_ROTATION = 1000,
		BM_ROTATE_90_CLOCKWISE = 1001,
		BM_ROTATE_90_COUNTERCLOCKWISE = 1002,
		BM_ROTATE_180 = 1003
	}

	static private NativeArray<Color32> InputImageData;
	static private bool InputImageDataReady;

	static public Texture2D OverlayTexture;
	static private int TargetOverlayTextureWidth;
	static private int TargetOverlayTextureHeight;
	static private bool OverlayTextureReady;

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_create();

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern int BSocialWrapper_load_offline_licence_key(string licenceKeyPath);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern int BSocialWrapper_load_online_licence_key(string licenceKeyPath);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_set_offline_licence_key(string licenceKey);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_set_online_licence_key(string licenceKey);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern int BSocialWrapper_init(string modelsPath);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern int BSocialWrapper_init_alternative(
		string faceDetectorModelPath,
		string faceTrackerModelsPath,
		string faceTrackerMeanPath,
		string faceTrackerSigmaPath,
		string auModelPath,
		string gazeModelPath,
		string valenceModelPath,
		string arousalModelPath,
		string bodyTrackerModelPath
	);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern int BSocialWrapper_init_embedded();

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_set_nthreads(int nthreads);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_set_min_face_diagonal(int value);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_set_body_tracking_enabled(bool enabled);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_set_predictions_postprocessing_enabled(bool enabled);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern int BSocialWrapper_run();

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_reset();

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public unsafe static extern bool BSocialWrapper_get_landmarks(
		float *lx, float *ly);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public unsafe static extern bool BSocialWrapper_get_aus(float *aprd);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern bool BSocialWrapper_get_affect(
		ref float valence, ref float arousal, ref float valence_smoothed, ref float arousal_smoothed);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public unsafe static extern bool BSocialWrapper_get_gaze(
		float *gazeAngles, float *gazeVectors);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern bool BSocialWrapper_get_head_angles(
		ref float yaw, ref float pitch, ref float roll);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern bool BSocialWrapper_get_head_position(
		ref float x, ref float y, ref float z);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern bool BSocialWrapper_get_face_detection(
		ref int x, ref int y, ref int width, ref int height);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern bool BSocialWrapper_get_image_quality(
		ref bool imageBlurry, ref bool imageNoisy, ref bool imageTooDark, ref bool imageTooBright);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public unsafe static extern bool BSocialWrapper_get_body_points(
		float *bx, float *by);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern int BSocialWrapper_start_write();

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_stop_write();

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_set_output_path(string outputPath);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public unsafe static extern void BSocialWrapper_set_image_unity_native(
		void *data, int width, int height, bool flip, bool display, BSocialWrapper_Rotation rotation);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public unsafe static extern void BSocialWrapper_set_image_unity(
		byte *data, int width, int height, bool flip, bool display, BSocialWrapper_Rotation rotation);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_set_image_unity_alternative(
		ref Color32[] data, int width, int height, bool flip);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_dealloc();

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public unsafe static extern void BSocialWrapper_overlay_unity_native(
		void *data, int width, int height, bool flip, bool display, BSocialWrapper_Rotation rotation);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public unsafe static extern void BSocialWrapper_overlay_unity(
		byte *data, int width, int height, bool flip, bool display, BSocialWrapper_Rotation rotation);

	#if UNITY_IOS
		[DllImport("__Internal")]
	#else
		[DllImport("libBM.BSocial-lib")]
	#endif
	public static extern void BSocialWrapper_overlay_unity_alternative(
		ref Color32[] data, int width, int height, bool flip);

	public unsafe static BSocialUnity.BSocialPredictions BSocialWrapper_get_predictions()
	{
		BSocialUnity.BSocialPredictions predictions;

		predictions.landmarks.x = new List<float>();
		predictions.landmarks.y = new List<float>();
		predictions.landmarks.valid = false;

		int nLandmarks = 66;

		float[] lx = new float[nLandmarks];
		float[] ly = new float[nLandmarks];

		fixed (float *lxp = lx)
		{
			fixed (float *lyp = ly)
			{
				predictions.landmarks.valid = BSocialWrapper_get_landmarks(lxp, lyp);
			}
		}

		for (int i = 0; i < nLandmarks; i++)
		{
			predictions.landmarks.x.Add(lx[i]);
			predictions.landmarks.y.Add(ly[i]);
		}

		predictions.actionUnits.classes = new List<string>();
		predictions.actionUnits.predictions = new List<float>();
		predictions.actionUnits.valid = false;

		int nActionUnits = 30;
		int bufferLength = 5;

		byte[] acls = new byte[nActionUnits * bufferLength];
		float[] aprd = new float[nActionUnits];

		fixed (float *aprdPointer = aprd)
		{
			predictions.actionUnits.valid = BSocialWrapper_get_aus(aprdPointer);
		}

		byte[] buffer = new byte[bufferLength];
		for (int i = 0; i < nActionUnits; i++)
		{
			predictions.actionUnits.predictions.Add(aprd[i]);

			/*

			for (int k = 0; k < bufferLength; k++)
			{
				buffer[k] = acls[i, k];
			}

			predictions.actionUnits.classes.Add(
				System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length));

			*/
		}

		predictions.actionUnits.classes.Add("AU01");
		predictions.actionUnits.classes.Add("AU01R");
		predictions.actionUnits.classes.Add("AU02");
		predictions.actionUnits.classes.Add("AU02R");
		predictions.actionUnits.classes.Add("AU04");
		predictions.actionUnits.classes.Add("AU04R");
		predictions.actionUnits.classes.Add("AU05");
		predictions.actionUnits.classes.Add("AU05R");
		predictions.actionUnits.classes.Add("AU06");
		predictions.actionUnits.classes.Add("AU06R");
		predictions.actionUnits.classes.Add("AU07");
		predictions.actionUnits.classes.Add("AU07R");
		predictions.actionUnits.classes.Add("AU09");
		predictions.actionUnits.classes.Add("AU09R");
		predictions.actionUnits.classes.Add("AU10");
		predictions.actionUnits.classes.Add("AU10R");
		predictions.actionUnits.classes.Add("AU12");
		predictions.actionUnits.classes.Add("AU12R");
		predictions.actionUnits.classes.Add("AU14");
		predictions.actionUnits.classes.Add("AU14R");
		predictions.actionUnits.classes.Add("AU15");
		predictions.actionUnits.classes.Add("AU15R");
		predictions.actionUnits.classes.Add("AU17");
		predictions.actionUnits.classes.Add("AU17R");
		predictions.actionUnits.classes.Add("AU23");
		predictions.actionUnits.classes.Add("AU23R");
		predictions.actionUnits.classes.Add("AU25");
		predictions.actionUnits.classes.Add("AU25R");
		predictions.actionUnits.classes.Add("AU45");
		predictions.actionUnits.classes.Add("AU45R");

		predictions.affect.valence = 0;
		predictions.affect.arousal = 0;
		predictions.affect.valence_smoothed = 0;
		predictions.affect.arousal_smoothed = 0;
		predictions.affect.valid = false;

		predictions.affect.valid = BSocialWrapper_get_affect(
			ref predictions.affect.valence, ref predictions.affect.arousal, ref predictions.affect.valence_smoothed, ref predictions.affect.arousal_smoothed);

		/* gaze */

		predictions.gaze.angles.yaw = 0;
		predictions.gaze.angles.pitch = 0;
		predictions.gaze.anglesAverage.yaw = 0;
		predictions.gaze.anglesAverage.pitch = 0;
		predictions.gaze.vector.x = 0;
		predictions.gaze.vector.y = 0;
		predictions.gaze.vector.z = 0;
		predictions.gaze.vectorAverage.x = 0;
		predictions.gaze.vectorAverage.y = 0;
		predictions.gaze.vectorAverage.z = 0;
		predictions.gaze.valid = false;

		float[] gazeAngles = new float[4];
		float[] gazeVectors = new float[6];

		fixed (float *gazeAnglesPointer = gazeAngles)
		{
			fixed (float *gazeVectorsPointer = gazeVectors)
			{
				predictions.gaze.valid = BSocialWrapper_get_gaze(
					gazeAnglesPointer, gazeVectorsPointer);
			}
		}

		predictions.gaze.angles.yaw = gazeAngles[0];
		predictions.gaze.angles.pitch = gazeAngles[1];

		predictions.gaze.anglesAverage.yaw = gazeAngles[2];
		predictions.gaze.anglesAverage.pitch = gazeAngles[3];

		predictions.gaze.vector.x = gazeVectors[0];
		predictions.gaze.vector.y = gazeVectors[1];
		predictions.gaze.vector.z = gazeVectors[2];

		predictions.gaze.vectorAverage.x = gazeVectors[3];
		predictions.gaze.vectorAverage.y = gazeVectors[4];
		predictions.gaze.vectorAverage.z = gazeVectors[5];

		/* head angles */

		predictions.headAngles.yaw = 0;
		predictions.headAngles.pitch = 0;
		predictions.headAngles.roll = 0;
		predictions.headAngles.valid = false;

		predictions.headAngles.valid = BSocialWrapper_get_head_angles(
			ref predictions.headAngles.yaw,
			ref predictions.headAngles.pitch,
			ref predictions.headAngles.roll
		);

		/* head position */

		predictions.headPosition.x = 0;
		predictions.headPosition.y = 0;
		predictions.headPosition.z = 0;
		predictions.headPosition.valid = false;

		predictions.headPosition.valid = BSocialWrapper_get_head_position(
			ref predictions.headPosition.x,
			ref predictions.headPosition.y,
			ref predictions.headPosition.z
		);

		/* face detection */

		predictions.faceDetection.x = 0;
		predictions.faceDetection.y = 0;
		predictions.faceDetection.width = 0;
		predictions.faceDetection.height = 0;
		predictions.faceDetection.valid = false;

		predictions.faceDetection.valid = BSocialWrapper_get_face_detection(
			ref predictions.faceDetection.x,
			ref predictions.faceDetection.y,
			ref predictions.faceDetection.width,
			ref predictions.faceDetection.height
		);

		/* Image Quality */

		predictions.imageQuality.imageBlurry = false;
		predictions.imageQuality.imageNoisy = false;
		predictions.imageQuality.imageTooBright = false;
		predictions.imageQuality.imageTooDark = false;
		predictions.imageQuality.valid = false;

		predictions.imageQuality.valid = BSocialWrapper_get_image_quality(
			ref predictions.imageQuality.imageBlurry,
			ref predictions.imageQuality.imageNoisy,
			ref predictions.imageQuality.imageTooDark,
			ref predictions.imageQuality.imageTooBright
		);

		/* Body Tracking */

		predictions.bodyPoints.x = new List<float>();
		predictions.bodyPoints.y = new List<float>();
		predictions.bodyPoints.valid = false;

		int nBodyPoints = 9;

		float[] bx = new float[nBodyPoints];
		float[] by = new float[nBodyPoints];

		fixed (float *bxp = bx)
		{
			fixed (float *byp = by)
			{
				predictions.bodyPoints.valid = BSocialWrapper_get_body_points(bxp, byp);
			}
		}

		for (int i = 0; i < nBodyPoints; i++)
		{
			predictions.bodyPoints.x.Add(bx[i]);
			predictions.bodyPoints.y.Add(by[i]);
		}

		return predictions;
	}

	public unsafe static void BSocialWrapper_set_image(
		ref Color32[] wTextureData, ref byte[] wTextureBytes, int width, int height, bool flip, bool display, BSocialWrapper_Rotation rotation)
	{
		int pixelIndex = 0;
		for (int i = wTextureData.Length - 1; i >= 0; i--)
		{
			byte r = wTextureData[i].r;
			byte g = wTextureData[i].g;
			byte b = wTextureData[i].b;

			/*
			 * Alpha channel is not used:
			 * 
			 * byte a = wTextureData[i].a;
			 * 
			 */

			wTextureBytes[pixelIndex] = r;
			pixelIndex++;

			wTextureBytes[pixelIndex] = g;
			pixelIndex++;

			wTextureBytes[pixelIndex] = b;
			pixelIndex++;
		}

		fixed (byte *wTexturePointer = wTextureBytes)
		{
			BSocialWrapper_set_image_unity(wTexturePointer, width, height, flip, display, rotation);
		}
	}

	public static void BSocialWrapper_set_image_native(
		ref Color32[] wTextureData, int width, int height, bool flip, bool display, BSocialWrapper_Rotation rotation)
	{

		if (InputImageData.Length != wTextureData.Length)
		{
			InputImageDataReady = false;
		}

		if (!InputImageDataReady)
		{
			if (InputImageData.IsCreated)
			{
				InputImageData.Dispose();
			}

			InputImageData = new NativeArray<Color32>(
				wTextureData.Length, Allocator.Persistent);
			InputImageDataReady = true;
		}

		/*

		InputImageData = new NativeArray<Color32>(
			wTextureData.Length, Allocator.Temp);

		*/

		InputImageData.CopyFrom(wTextureData);

		unsafe
		{
			void *imageDataPtr = NativeArrayUnsafeUtility.GetUnsafePtr(InputImageData);
			BSocialWrapper_set_image_unity_native(
				imageDataPtr, width, height, flip, display, rotation);
		}

	}

	public unsafe static void BSocialWrapper_overlay(
		ref Color32[] wTextureData, ref byte[] wTextureBytes, int width, int height, bool flip, bool display, BSocialWrapper_Rotation rotation)
	{
		int pixelIndex = 0;
		for (int i = wTextureData.Length - 1; i >= 0; i--)
		{
			byte r = wTextureData[i].r;
			byte g = wTextureData[i].g;
			byte b = wTextureData[i].b;

			/*
			 * Alpha channel is not used:
			 * 
			 * byte a = wTextureData[i].a;
			 * 
			 */

			wTextureBytes[pixelIndex] = r;
			pixelIndex++;

			wTextureBytes[pixelIndex] = g;
			pixelIndex++;

			wTextureBytes[pixelIndex] = b;
			pixelIndex++;
		}

		fixed (byte *wTexturePointer = wTextureBytes)
		{
			BSocialWrapper_overlay_unity(wTexturePointer, width, height, flip, display, rotation);
		}
	}

	public static void BSocialWrapper_overlay_native(
		ref Color32[] wTextureData, int width, int height, bool flip, bool display, BSocialWrapper_Rotation rotation)
	{

		if (InputImageData.Length != wTextureData.Length)
		{
			InputImageDataReady = false;
		}

		if (!InputImageDataReady)
		{
			if (InputImageData.IsCreated)
			{
				InputImageData.Dispose();
			}

			InputImageData = new NativeArray<Color32>(
				wTextureData.Length, Allocator.Persistent);
			InputImageDataReady = true;
		}

		if (rotation == BSocialWrapper_Rotation.BM_NO_ROTATION)
        {
			TargetOverlayTextureWidth = width;
			TargetOverlayTextureHeight = height;
        }

		if (rotation == BSocialWrapper_Rotation.BM_ROTATE_180)
		{
			TargetOverlayTextureWidth = width;
			TargetOverlayTextureHeight = height;
		}

		if (rotation == BSocialWrapper_Rotation.BM_ROTATE_90_CLOCKWISE)
		{
			TargetOverlayTextureWidth = height;
			TargetOverlayTextureHeight = width;
		}

		if (rotation == BSocialWrapper_Rotation.BM_ROTATE_90_COUNTERCLOCKWISE)
		{
			TargetOverlayTextureWidth = height;
			TargetOverlayTextureHeight = width;
		}

		if (OverlayTexture)
        {
			if (OverlayTexture.width != TargetOverlayTextureWidth)
			{
				OverlayTextureReady = false;
			}

			if (OverlayTexture.height != TargetOverlayTextureHeight)
			{
				OverlayTextureReady = false;
			}
		} else
        {
			OverlayTextureReady = false;
		}

		if (!OverlayTextureReady)
        {
			OverlayTexture = new Texture2D(
				TargetOverlayTextureWidth, TargetOverlayTextureHeight);
			OverlayTextureReady = true;
		}

		/*

		InputImageData = new NativeArray<Color32>(
			wTextureData.Length, Allocator.Temp);

		*/

		InputImageData.CopyFrom(wTextureData);

		unsafe
		{
			void *imageDataPtr = NativeArrayUnsafeUtility.GetUnsafePtr(InputImageData);
			BSocialWrapper_overlay_unity_native(
				imageDataPtr, width, height, flip, display, rotation);
		}

		OverlayTexture.SetPixels32(InputImageData.ToArray());
		OverlayTexture.Apply();

	}

}
