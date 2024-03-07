using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneOrderManager2 {

    List<Scene> startScenes = new List<Scene> ();
    List<Scene> middleScenes = new List<Scene> ();
    List<Scene> endScenes = new List<Scene> ();
    List<Scene> allScenes = new List<Scene> ();

    public List<Scene> currentSceneOrder = new List<Scene> ();
    List<List<Scene>> previousSceneOrders = new List<List<Scene>> ();
    List<Scene> requiredScenes = new List<Scene> ();


    bool isValenceHigher;
    bool isArousalHigher;

    int currentSceneIndex;
    int sceneCount;
    int sceneCountMin;
    int sceneCountMax;
    int numOfScreenings;
    int currentScreeningIndex = 0;

    public SceneOrderManager2 ( int screenings ) {
        //Define the setup for each scene 1 - 10
        SceneSetup ();

        ConfigureScreeningsAndSceneCount ( screenings );
    }

    public void ResetSceneOrderForNextScreening() {
        previousSceneOrders.Add ( currentSceneOrder );

        currentSceneOrder = new List<Scene> ();

        currentScreeningIndex++;

        startScenes = new List<Scene> ();
        middleScenes = new List<Scene> ();
        endScenes = new List<Scene> ();
        allScenes = new List<Scene> ();

        SceneSetup ();
    }

    private void SceneSetup () {
        //Debug.Log($"[{GetType().Name}] Scenes Setup");

        allScenes.Add ( new Scene ( 1 ) );
        allScenes.Add ( new Scene ( 2 ) );
        allScenes.Add ( new Scene ( 3 ) );
        allScenes.Add ( new Scene ( 4 ) );
        allScenes.Add ( new Scene ( 5 ) );
        allScenes.Add ( new Scene ( 6 ) );
        allScenes.Add ( new Scene ( 7 ) );
        allScenes.Add ( new Scene ( 8 ) );
        allScenes.Add ( new Scene ( 9 ) );
        allScenes.Add ( new Scene ( 10 ) );

        middleScenes.Add ( new Scene ( 1 ) );
        middleScenes.Add ( new Scene ( 2 ) );
        middleScenes.Add ( new Scene ( 3 ) );
        middleScenes.Add ( new Scene ( 4 ) );
        middleScenes.Add ( new Scene ( 6 ) );
        middleScenes.Add ( new Scene ( 8 ) );
        middleScenes.Add ( new Scene ( 9 ) );

        startScenes.Add ( new Scene ( 1 ) );
        startScenes.Add ( new Scene ( 2 ) );
        startScenes.Add ( new Scene ( 3 ) );
        startScenes.Add ( new Scene ( 4 ) );
        startScenes.Add ( new Scene ( 6 ) );
        startScenes.Add ( new Scene ( 7 ) );
        startScenes.Add ( new Scene ( 8 ) );
        startScenes.Add ( new Scene ( 10 ) );

        endScenes.Add ( new Scene ( 2 ) );
        endScenes.Add ( new Scene ( 4 ) );
        endScenes.Add ( new Scene ( 5 ) );
        endScenes.Add ( new Scene ( 7 ) );
        endScenes.Add ( new Scene ( 10 ) );
    }
    /// <summary>
    /// Set the higher and lower values for valence and arousal
    /// </summary>
    /// <param name="valenceHigher"></param>
    /// <param name="arousalHigher"></param>
    public void SetValenceArousalValues ( bool valenceHigher, bool arousalHigher ) {
        isValenceHigher = valenceHigher;
        isArousalHigher = arousalHigher;

        //Configure the amount of screenings and the total current scene count
        ModifyWeightsForValenceAndArousal ();
    }

    private void ConfigureScreeningsAndSceneCount ( int screenings ) {

        //Debug.Log($"[{GetType().Name}] Configured Scene Count");

        /*
         * Here we want to configure how many screenings there are,
         * then define the scene count range,
         * and finally define the actual scene count for this screening
         */

        numOfScreenings = screenings;

        switch ( numOfScreenings ) {
            case < 2:
                sceneCountMin = 7;
                sceneCountMax = 7;
                break;
            case < 3:
                sceneCountMin = 5;
                sceneCountMax = 7;
                break;
            case < 4:
                sceneCountMin = 4;
                sceneCountMax = 6;
                break;
            case >= 4:
                sceneCountMin = 3;
                sceneCountMax = 5;
                break;
        }
    }

    private void ModifyWeightsForValenceAndArousal () {
        /*
         * Here we want to add directly to the weight values based on arousal responsibility graph and valence positivity graph
         */

        foreach ( Scene scene in startScenes ) {
            scene.UpdateWeightsForValenceArousal ( isValenceHigher, isArousalHigher );
        }

        foreach ( Scene scene in middleScenes ) {
            scene.UpdateWeightsForValenceArousal ( isValenceHigher, isArousalHigher );
        }

        foreach ( Scene scene in startScenes ) {
            scene.UpdateWeightsForValenceArousal ( isValenceHigher, isArousalHigher );
        }
    }

    public void CreateSceneOrder () {
        /*
         * Here we want to create a scene order, 
         * checking for first scene to apply the start weight,
         * then ignore start weight and go off current weight and future links,
         * then include end weight for final scene
         * 
         * Later apply previous scene orders as additional weightings for current scene order
         */

        Debug.Log($"[{GetType().Name}] Create Scene Order");

        if ( numOfScreenings == 1 ) {
            sceneCount = 7;
        } else if ( currentScreeningIndex == numOfScreenings - 1 ) {
            List<int> sceneIndexes = new List<int> ();

            List<Scene> modifiedStartScenes = new List<Scene> ();
            List<Scene> modifiedMiddleScenes = new List<Scene> ();
            List<Scene> modifiedEndScenes = new List<Scene> ();

            modifiedStartScenes = startScenes;
            modifiedMiddleScenes = middleScenes;
            modifiedEndScenes = endScenes;

            bool isSceneAlreadyIncluded = false;

            foreach ( List<Scene> sceneOrders in previousSceneOrders ) {
                foreach ( Scene scene in sceneOrders ) {
                    isSceneAlreadyIncluded = false;
                    foreach ( int index in sceneIndexes ) {
                        if ( scene.index == index ) {
                            isSceneAlreadyIncluded = true;
                        }
                    }

                    if ( !isSceneAlreadyIncluded ) {
                        sceneIndexes.Add ( scene.index );
                    }
                }
            }

            foreach ( Scene scene in allScenes ) {
                requiredScenes.Add ( scene );
            }

            if ( sceneIndexes.Count < allScenes.Count ) {
                foreach ( int index in sceneIndexes ) {
                    foreach ( Scene scene in requiredScenes ) {
                        if ( scene.index == index ) {
                            requiredScenes.Remove ( scene );
                            break;
                        }
                    }
                }
            }
        } else {
            sceneCount = ( int ) Random.Range ( sceneCountMin, sceneCountMax + 1 );
        }

        Debug.Log($"[{GetType().Name}] Scene Count from Algorithm : " + sceneCount );

        for ( int i = 0; i < sceneCount; i++ ) {
            //Check if this is the first screening

            foreach ( Scene scene in currentSceneOrder ) {
                foreach ( Scene midScene in middleScenes ) {
                    if ( scene.index == midScene.index ) {
                        middleScenes.Remove ( midScene );
                        break;
                    }
                }
                foreach ( Scene endScene in endScenes ) {
                    if ( scene.index == endScene.index ) {
                        endScenes.Remove ( endScene );
                        break;
                    }
                }
            }

            if ( i == 0 ) {
                Debug.Log($"[{GetType().Name}] First Scene set" );
                ConfigureFirstScene ();
            } else if ( currentSceneIndex == sceneCount - 1 ) {
                Debug.Log($"[{GetType().Name}] End Scene set" );
                ConfigureEndScene ();
            } else {
                Debug.Log($"[{GetType().Name}] Middle Scenes set" );
                ConfigureNextScene ();
            }
        }

        currentSceneOrder.Add ( new Scene ( 11 ) );
    }

    private void ConfigureFirstScene () {
        int weightSum = 0;
        int startSceneRandomWeightVal;
        bool isSceneWeightReduced;

        if ( currentScreeningIndex == numOfScreenings - 1 ) {
            foreach ( Scene scene in requiredScenes ) {
                foreach (Scene startScene in startScenes)
                {
                    if(startScene.index == scene.index)
                    {
                        currentSceneOrder.Add ( scene );
                        requiredScenes.Remove ( scene );
                        currentSceneIndex++;
                        return;
                    }
                }
                weightSum += scene.startWeighting * 2;
            }

            if ( weightSum > 0 ) {
                startSceneRandomWeightVal = ( int ) Random.Range ( 0, weightSum + 1 );

                weightSum = 0;

                foreach ( Scene scene in requiredScenes ) {
                    weightSum += scene.startWeighting * 2;

                    if ( startSceneRandomWeightVal <= weightSum ) {
                        currentSceneOrder.Add ( scene );
                        requiredScenes.Remove ( scene );
                        currentSceneIndex++;
                        return;
                    }
                }
            }
        }

        //Calculate the initial weightSum
        foreach ( Scene scene in startScenes ) {
            isSceneWeightReduced = false;

            if ( currentScreeningIndex > 0 ) {
                foreach ( List<Scene> sceneOrder in previousSceneOrders ) {
                    if ( scene.index == sceneOrder.First ().index ) {
                        weightSum += ( scene.startWeighting / 5 );
                        isSceneWeightReduced = true;
                        break;
                    }
                }
            }

            if ( !isSceneWeightReduced ) {
                weightSum += scene.startWeighting * 2;
            }
        }

        //Define the randomWeightValue within the total weightSum
        startSceneRandomWeightVal = ( int ) Random.Range ( 0, weightSum + 1 );

        //Calculate which scene falls within the randomWeightValue and at to scene order
        weightSum = 0;
        foreach ( Scene scene in startScenes ) {
            isSceneWeightReduced = false;

            if ( currentScreeningIndex > 0 ) {
                foreach ( List<Scene> sceneOrder in previousSceneOrders ) {
                    if ( scene.index == sceneOrder.First ().index ) {
                        weightSum += ( scene.startWeighting / 5 );
                        isSceneWeightReduced = true;
                        break;
                    }
                }
            }

            if ( !isSceneWeightReduced ) {
                weightSum += scene.startWeighting * 2;
            }

            if ( startSceneRandomWeightVal <= weightSum ) {
                currentSceneOrder.Add ( scene );
                currentSceneIndex++;
                return;
            }
        }
    }

    private void ConfigureNextScene () {
        int weightSum = 0;
        int startSceneRandomWeightVal = 0;

        bool isSceneInOrder = false;
        bool isSceneWeightReduced = false;

        if ( currentScreeningIndex == numOfScreenings - 1 ) {
            foreach ( Scene scene in requiredScenes ) {
                foreach ( Scene midScene in middleScenes )
                {
                    if ( midScene.index == scene.index )
                    {
                        currentSceneOrder.Add ( scene );
                        requiredScenes.Remove ( scene );
                        currentSceneIndex++;
                        return;
                    }
                }
                weightSum += scene.weight;
            }

            if ( weightSum > 0 ) {
                startSceneRandomWeightVal = ( int ) Random.Range ( 0, weightSum + 1 );

                weightSum = 0;

                foreach ( Scene scene in requiredScenes ) {
                    weightSum += scene.weight;

                    if ( startSceneRandomWeightVal <= weightSum ) {
                        currentSceneOrder.Add ( scene );
                        requiredScenes.Remove ( scene );
                        currentSceneIndex++;
                        return;
                    }
                }
            }
        }

        foreach ( Scene scene in middleScenes ) {
            isSceneInOrder = false;
            isSceneWeightReduced = false;

            foreach ( Scene curOrderScene in currentSceneOrder ) {
                if ( curOrderScene.index == scene.index ) {
                    isSceneInOrder = true;
                }
            }

            //Continues if the scene is not within the current scene order
            if ( !isSceneInOrder ) {
                //Checks whether the current screening is after the first
                if ( currentScreeningIndex > 0 ) {
                    //Loop through eachh previous scene order
                    foreach ( List<Scene> sceneOrder in previousSceneOrders ) {
                        //Loop through each scene in the current previous scene order
                        for ( int i = 0; i < sceneOrder.Count; i++ ) {
                            //Check if the index of the current potential scene is the same as the current previous scene order scene
                            if ( scene.index == sceneOrder[ i ].index ) {
                                //Check if the current previous scene order scene is not the first in that order
                                //and check if the current scene order previous scene is the same as the current previous scene order previous scene
                                if ( i > 0 && currentSceneOrder[ currentSceneOrder.Count - 1 ].index == sceneOrder[ i - 1 ].index ) {
                                    //Reduce the scene weight by a fifth
                                    weightSum += ( scene.weight / 5 );
                                    isSceneWeightReduced = true;
                                }
                            }
                            //Break loop if scene weight has been reduced
                            if ( isSceneWeightReduced ) {
                                break;
                            }
                        }

                        //Break loop if scene weight has been reduced
                        if ( isSceneWeightReduced ) {
                            break;
                        }
                    }
                }

                if ( !isSceneWeightReduced ) {
                    //Add current scene weight to the weightSum
                    weightSum += scene.weight;
                }
                //Check if the current scene index is a future link to the previous scene index
                //and check that the current scene weight has not been reduced already
                if ( scene.index == currentSceneOrder.Last ().futureLink && !isSceneWeightReduced ) {
                    weightSum += 80;
                }
            }
        }

        //calculate the startSceneRandomWeightVal based off the weightSum
        startSceneRandomWeightVal = ( int ) Random.Range ( 0, weightSum + 1 );

        //Reset the weightSum
        weightSum = 0;

        //Loop through each scene in all possible scenes
        foreach ( Scene scene in middleScenes ) {
            //reset booleans
            isSceneInOrder = false;
            isSceneWeightReduced = false;

            //Loop through each scene in the current scene order
            foreach ( Scene curOrderScene in currentSceneOrder ) {
                //Check if the current scene index is already in the current scene order
                if ( curOrderScene.index == scene.index ) {
                    //Set scene in order to true
                    isSceneInOrder = true;
                }
            }

            //Continues if the scene is not within the current scene order
            if ( !isSceneInOrder ) {
                //Checks whether the current screening is after the first
                if ( currentScreeningIndex > 0 ) {
                    //Loop through eachh previous scene order
                    foreach ( List<Scene> sceneOrder in previousSceneOrders ) {
                        //Loop through each scene in the current previous scene order
                        for ( int i = 0; i < sceneOrder.Count; i++ ) {
                            //Check if the index of the current potential scene is the same as the current previous scene order scene
                            if ( scene.index == sceneOrder[ i ].index ) {
                                //Check if the current previous scene order scene is not the first in that order
                                //and check if the current scene order previous scene is the same as the current previous scene order previous scene
                                if ( i > 0 && currentSceneOrder[ currentSceneOrder.Count - 1 ].index == sceneOrder[ i - 1 ].index ) {
                                    //Reduce the scene weight by a fifth
                                    weightSum += ( scene.weight / 5 );
                                    isSceneWeightReduced = true;
                                }
                            }
                            //Break loop if scene weight has been reduced
                            if ( isSceneWeightReduced ) {
                                break;
                            }
                        }

                        //Break loop if scene weight has been reduced
                        if ( isSceneWeightReduced ) {
                            break;
                        }
                    }
                }

                if ( !isSceneWeightReduced ) {
                    //Add current scene weight to the weightSum
                    weightSum += scene.weight;
                }
                //Check if the current scene index is a future link to the previous scene index
                //and check that the current scene weight has not been reduced already
                if ( scene.index == currentSceneOrder.Last ().futureLink && !isSceneWeightReduced ) {
                    weightSum += 80;
                }

                //Check if the startSceneRandomWeightVal is less than the current weightSum
                if ( startSceneRandomWeightVal <= weightSum ) {

                    //Add the current scene to the scene order
                    currentSceneOrder.Add ( scene );

                    //increment the current scene index by 1

                    currentSceneIndex++;
                    //return from the function
                    return;
                }
            }
        }
    }

    private void ConfigureEndScene () {
        int weightSum = 0;
        int startSceneRandomWeightVal = 0;

        bool isSceneInOrder = false;
        bool isSceneWeightReduced = false;


        if ( currentScreeningIndex == numOfScreenings - 1 ) {
            foreach ( Scene scene in requiredScenes ) {
                foreach ( Scene endScene in endScenes )
                {
                    if ( endScene.index == scene.index )
                    {
                        currentSceneOrder.Add ( scene );
                        requiredScenes.Remove ( scene );
                        currentSceneIndex++;
                        return;
                    }
                }
                weightSum += scene.endWeighting;
            }

            if ( weightSum > 0 ) {
                startSceneRandomWeightVal = ( int ) Random.Range ( 0, weightSum + 1 );

                weightSum = 0;

                foreach ( Scene scene in requiredScenes ) {
                    weightSum += scene.endWeighting;

                    if ( startSceneRandomWeightVal <= weightSum ) {
                        currentSceneOrder.Add ( scene );
                        requiredScenes.Remove ( scene );
                        currentSceneIndex++;
                        return;
                    }
                }
            }
        }

        foreach ( Scene scene in endScenes ) {
            isSceneInOrder = false;
            isSceneWeightReduced = false;

            foreach ( Scene curOrderScene in currentSceneOrder ) {
                if ( curOrderScene.index == scene.index ) {
                    isSceneInOrder = true;
                }
            }

            if ( !isSceneInOrder ) {
                if ( currentScreeningIndex > 0 ) {
                    foreach ( List<Scene> sceneOrder in previousSceneOrders ) {
                        if ( scene.index == sceneOrder.Last ().index ) {
                            weightSum += ( scene.endWeighting / 5 );
                            isSceneWeightReduced = true;
                            break;
                        }
                    }
                }

                if ( !isSceneWeightReduced ) {
                    weightSum += scene.endWeighting;
                }
            }
        }

        startSceneRandomWeightVal = ( int ) Random.Range ( 0, weightSum + 1 );

        weightSum = 0;

        foreach ( Scene scene in endScenes ) {
            isSceneInOrder = false;
            isSceneWeightReduced = false;

            foreach ( Scene curOrderScene in currentSceneOrder ) {
                if ( curOrderScene.index == scene.index ) {
                    isSceneInOrder = true;
                }
            }

            if ( !isSceneInOrder ) {
                if ( currentScreeningIndex > 0 ) {
                    foreach ( List<Scene> sceneOrder in previousSceneOrders ) {
                        if ( scene.index == sceneOrder.Last ().index ) {
                            weightSum += ( scene.endWeighting / 5 );
                            isSceneWeightReduced = true;
                            break;
                        }
                    }
                }

                if ( !isSceneWeightReduced ) {
                    weightSum += scene.endWeighting;
                }

                if ( startSceneRandomWeightVal <= weightSum ) {
                    currentSceneOrder.Add ( scene );
                    currentSceneIndex++;
                    return;
                }
            }
        }
    }
}
