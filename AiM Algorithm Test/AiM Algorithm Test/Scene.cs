using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiM_Algorithm_Test {
    internal class Scene {
        int startWeighting;
        int endWeighting;
        int futureLink;
        int pastConnection;
        float weight;

        public Scene () {
            startWeighting = 0;
            endWeighting = 0;
            futureLink = 0;
            pastConnection = 0;
            weight = 0.0f;
        }

        public Scene ( int sceneID ) {
            switch ( sceneID ) {
                case 1:
                    startWeighting = 100;
                    futureLink = 6;
                    break;
                case 2:
                    startWeighting = 100;
                    endWeighting = 80;
                    futureLink = 7;
                    break;
                case 3:
                    startWeighting = 80;
                    futureLink = 8;
                    break;
                case 4:
                    startWeighting = 60;
                    endWeighting = 80;
                    futureLink = 9;
                    break;
                case 5:
                    endWeighting = 100;
                    futureLink = 10;
                    break;
                case 6:
                    startWeighting = 80;
                    break;
                case 7:
                    startWeighting = 100;
                    endWeighting = 100;
                    break;
                case 8:
                    startWeighting = 80;
                    break;
                case 10:
                    startWeighting = 100;
                    endWeighting = 100;
                    break;
            }
        }
    }
}
