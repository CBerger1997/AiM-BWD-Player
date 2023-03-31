using System.ComponentModel;
using System.Runtime.CompilerServices;

internal class Program {
    List<AiM_Algorithm_Test.Scene> scenes = new List<AiM_Algorithm_Test.Scene> ();

    List<AiM_Algorithm_Test.Scene> currenSceneOrder = new List<AiM_Algorithm_Test.Scene> ();
    List<List<AiM_Algorithm_Test.Scene>> previousSceneOrders = new List<List<AiM_Algorithm_Test.Scene>> ();

    bool isValenceHigher;
    bool isArousalHigher;
    
    int sceneCount;
    int numOfScreenings;

    private static void Main ( string[] args ) {
        Console.WriteLine ( "Hello, World!" );

        Program program = new Program ();

        //Define the setup for each scene 1 - 10
        program.SceneSetup ();

        //Set the higher and lower values for valence and arousal
        program.SetValenceArousalValues ();

        //Configure the amount of screenings and the total current scene count
        program.ConfigureScreeningsAndSceneCount ();

        //Assign weights to scenes based on the higher and lower values for valence and arousal
        program.ModifyWeightsForValenceAndArousal ();

        //Create the scene order
        program.CreateSceneOrder ();
    }

    private void SceneSetup () {
        for ( int i = 0; i < 10; i++ ) {
            AiM_Algorithm_Test.Scene newScene = new AiM_Algorithm_Test.Scene ( i + 1 );
            scenes.Add ( newScene );
        }
    }

    private void SetValenceArousalValues () {
        isValenceHigher = Convert.ToBoolean ( new Random ().Next ( 2 ) );
        isArousalHigher = Convert.ToBoolean ( new Random ().Next ( 2 ) );
    }

    private void ConfigureScreeningsAndSceneCount () {
        /*
         * Here we want to configure how many screenings there are,
         * then define the scene count range,
         * and finally define the actual scene count for this screening
         */
    }

    private void ModifyWeightsForValenceAndArousal () {
        /*
         * Here we want to add directly to the weight values based on arousal responsibility graph and valence positivity graph
         */
    }

    private void CreateSceneOrder () {
        /*
         * Here we want to create a scene order, 
         * checking for first scene to apply the start weight,
         * then ignore start weight and go off current weight and future links,
         * then include end weight for final scene
         * 
         * Later apply previous scene orders as additional weightings for current scene order
         */
    }
}