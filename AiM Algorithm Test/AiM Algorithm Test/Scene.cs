using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiM_Algorithm_Test {
    internal class Scene {
        public int startWeighting { get; set; }
        public int endWeighting { get; set; }
        public int futureLink { get; set; }
        public int weight { get; set; }
        public int index { get; set; }

        public Scene () {
            index = 0;
            startWeighting = 0;
            endWeighting = 0;
            futureLink = 0;
            weight = 0;
        }

        public Scene ( int sceneID ) {

            index = sceneID;

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

        public void UpdateWeightsForValenceArousal (bool isValenceHigher, bool isArousalHigher ) {
            switch ( index ) {
                case 1:
                    weight += isArousalHigher ? 80 : 30;
                    weight += isValenceHigher ? 60 : 60;
                    break;
                case 2:
                    weight += isArousalHigher ? 60 : 50;
                    weight += isValenceHigher ? 40 : 80;
                    break;
                case 3:
                    weight += isArousalHigher ? 20 : 90;
                    weight += isValenceHigher ? 20 : 100;
                    break;
                case 4:
                    weight += isArousalHigher ? 40 : 70;
                    weight += isValenceHigher ? 60 : 60;
                    break;
                case 5:
                    weight += isArousalHigher ? 10 : 100;
                    weight += isValenceHigher ? 50 : 40;
                    break;
                case 6:
                    weight += isArousalHigher ? 70 : 40;
                    weight += isValenceHigher ? 80 : 40;
                    break;
                case 7:
                    weight += isArousalHigher ? 100 : 10;
                    weight += isValenceHigher ? 100 : 20;
                    break;
                case 8:
                    weight += isArousalHigher ? 30 : 80;
                    weight += isValenceHigher ? 20 : 100;
                    break;
                case 9:
                    weight += isArousalHigher ? 50 : 60;
                    weight += isValenceHigher ? 40 : 80;
                    break;
                case 10:
                    weight += isArousalHigher ? 90 : 20;
                    weight += isValenceHigher ? 100 : 20;
                    break;
            }
        }
    }
}
