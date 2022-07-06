#pragma once

#include <stdint.h>

const uint64_t CHANGE_MODE_ADDRESS = 0x00000001401953D0;
const uint64_t CHANGE_SUB_MODE_ADDRESS = 0x0000000140195260;
const uint64_t START_PV_PROC = 0x1400dfaf8;
const uint64_t END_PV_PROC = 0x1400DFEC8;
const uint64_t ENGINE_UPDATE_HOOK_TARGET_ADDRESS = 0x000000014018CC40; // App frame update routine
const uint64_t CURRENT_GAME_STATE_ADDRESS = 0x0000000140EDA810; // App state enum address
const uint64_t UPDATE_TASKS_ADDRESS = 0x000000014019B980; // App task scheduler update routine
const uint64_t DATA_INIT_STATE_ADDRESS = 0x0000000140EDA7A8; // App data init state flag address
const uint64_t SYSTEM_WARNING_ELAPSED_ADDRESS = (0x00000001411A1430 + 0x68); // App task WARNING elapsed time address
const uint64_t INPUT_STATE_PTR_ADDRESS = 0x0000000140EDA330;


enum GameState : uint32_t
{
	GS_STARTUP,
	GS_ADVERTISE,
	GS_GAME,
	GS_DATA_TEST,
	GS_TEST_MODE,
	GS_APP_ERROR,
	GS_MAX,
};

enum SubGameState : uint32_t
{
	SUB_DATA_INITIALIZE,
	SUB_SYSTEM_STARTUP,
	SUB_SYSTEM_STARTUP_ERROR,
	SUB_WARNING,
	SUB_LOGO,
	SUB_RATING,
	SUB_DEMO,
	SUB_TITLE,
	SUB_RANKING,
	SUB_SCORE_RANKING,
	SUB_CM,
	SUB_PHOTO_MODE_DEMO,
	SUB_SELECTOR,
	SUB_GAME_MAIN,
	SUB_GAME_SEL,
	SUB_STAGE_RESULT,
	SUB_SCREEN_SHOT_SEL,
	SUB_SCREEN_SHOT_RESULT,
	SUB_GAME_OVER,
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
	SUB_TEST_MODE_MAIN,
	SUB_APP_ERROR,
	SUB_MAX,
};

enum JvsButtons : uint32_t
{
	JVS_NONE = 0 << 0x00, // 0x0

	JVS_TEST = 1 << 0x00, // 0x1
	JVS_SERVICE = 1 << 0x01, // 0x2

	JVS_START = 1 << 0x02, // 0x4
	JVS_TRIANGLE = 1 << 0x07, // 0x80
	JVS_SQUARE = 1 << 0x08, // 0x100
	JVS_CROSS = 1 << 0x09, // 0x200
	JVS_CIRCLE = 1 << 0x0A, // 0x400
	JVS_L = 1 << 0x0B, // 0x800
	JVS_R = 1 << 0x0C, // 0x1000

	JVS_SW1 = 1 << 0x12, // 0x40000
	JVS_SW2 = 1 << 0x13, // 0x80000
};

union ButtonState
{
	JvsButtons Buttons;
	uint32_t State[4];
};

// total sizeof() == 0x20E0
struct InputState
{
	ButtonState Tapped;
	ButtonState Released;

	ButtonState Down;
	uint32_t Padding_20[4];

	ButtonState DoubleTapped;
	uint32_t Padding_30[4];

	ButtonState IntervalTapped;
	uint32_t Padding_38[12];

	int32_t MouseX;
	int32_t MouseY;
	int32_t MouseDeltaX;
	int32_t MouseDeltaY;

	uint32_t Padding_AC[8];
	uint8_t Padding_D0[3];
	char Key;
};