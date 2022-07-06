using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuASM.Interop
{
    /// <summary>
    /// Game app state descriptor
    /// </summary>
    public enum AppState : uint
    {
        /// <summary>
        /// App is starting up
        /// </summary>
        GS_STARTUP,
        /// <summary>
        /// App is idle in advertise mode (ADV)
        /// </summary>
        GS_ADVERTISE,
        /// <summary>
        /// App is active in game mode
        /// </summary>
        GS_GAME,
        /// <summary>
        /// App is in data debug mode
        /// </summary>
        GS_DATA_TEST,
        /// <summary>
        /// App is in the test menu
        /// </summary>
        GS_TEST_MODE,
        /// <summary>
        /// App is in an error state
        /// </summary>
        GS_APP_ERROR,
        /// <summary>
        /// Dummy max value
        /// </summary>
        GS_MAX,
    }

    /// <summary>
    /// Game app substate descriptor
    /// </summary>
    public enum AppSubstate : uint
    {
        /// <summary>
        /// App is initializing the data
        /// </summary>
        SUB_DATA_INITIALIZE, //0
        /// <summary>
        /// App is starting up
        /// </summary>
        SUB_SYSTEM_STARTUP, //1
        /// <summary>
        /// App has failed to start up
        /// </summary>
        SUB_SYSTEM_STARTUP_ERROR, //2
        /// <summary>
        /// App is showing the copyright warning screen
        /// </summary>
        SUB_WARNING, //3
        /// <summary>
        /// App is showing the logo screen
        /// </summary>
        SUB_LOGO, //4
        /// <summary>
        /// App is showing the high score list screen
        /// </summary>
        SUB_RATING, //5
        /// <summary>
        /// App is showing a PV on ADV
        /// </summary>
        SUB_DEMO, //6
        /// <summary>
        /// App is showing the title screen?
        /// </summary>
        SUB_TITLE, //7
        /// <summary>
        /// App is showing the ranking?
        /// </summary>
        SUB_RANKING, //8
        /// <summary>
        /// App is showing the score ranking?
        /// </summary>
        SUB_SCORE_RANKING, //9
        /// <summary>
        /// App is playing a commercial
        /// </summary>
        SUB_CM, //A
        /// <summary>
        /// App is showing a demo of the photo studio mode
        /// </summary>
        SUB_PHOTO_MODE_DEMO, //B
        /// <summary>
        /// App is displaying a game selector?
        /// </summary>
        SUB_SELECTOR, //C
        /// <summary>
        /// App is playing through a level
        /// </summary>
        SUB_GAME_MAIN, //D
        /// <summary>
        /// App is displaying a song selector?
        /// </summary>
        SUB_GAME_SEL, //E
        /// <summary>
        /// App is showing the user's playthrough results
        /// </summary>
        SUB_STAGE_RESULT, //F
        /// <summary>
        /// App is showing the screenshot selection screen
        /// </summary>
        SUB_SCREEN_SHOT_SEL, //10
        /// <summary>
        /// App is showing the screenshot upload screen?
        /// </summary>
        SUB_SCREEN_SHOT_RESULT, //11
        /// <summary>
        /// App is playing the game over animation
        /// </summary>
        SUB_GAME_OVER, //12

        // ----- no need to handle these -----
        SUB_DATA_TEST_MAIN,
        SUB_DATA_TEST_MISC,
        SUB_DATA_TEST_OBJ,
        SUB_DATA_TEST_STG,
        SUB_DATA_TEST_MOT,
        SUB_DATA_TEST_COLLISION,
        SUB_DATA_TEST_SPR,
        SUB_DATA_TEST_AET,
        SUB_DATA_TEST_AUTH_3D,
        SUB_DATA_TEST_CHR,
        SUB_DATA_TEST_ITEM,
        SUB_DATA_TEST_PERF,
        SUB_DATA_TEST_PVSCRIPT,
        SUB_DATA_TEST_PRINT,
        SUB_DATA_TEST_CARD,
        SUB_DATA_TEST_OPD,
        SUB_DATA_TEST_SLIDER,
        SUB_DATA_TEST_GLITTER,
        SUB_DATA_TEST_GRAPHICS,
        SUB_DATA_TEST_COLLECTION_CARD,
        // ---------------------------------

        /// <summary>
        /// App is showing the test mode screen
        /// </summary>
        SUB_TEST_MODE_MAIN,
        /// <summary>
        /// App is showing a fatal error screen
        /// </summary>
        SUB_APP_ERROR,
        /// <summary>
        /// Dummy max value
        /// </summary>
        SUB_MAX,
    }
}
