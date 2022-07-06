// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "consts.h"
#include "interop.h"
#include "fastboot.h"
#include "vfs.h"

// Variables for script engine tick hook
typedef void runScriptCommand(void* scriptProc, void* arg2, void* arg3, float* arg4, char* arg5, char arg6);
runScriptCommand* ScriptProcessor = (runScriptCommand*)PROCESS_CMD;

/// Did break script engine loop? flag
bool breaked = false;

/// Hooked script engine tick
void ScriptProcessorHooked(void* scriptProc, void* arg2, void* arg3, float* arg4, char* arg5, char arg6) {
    // Get current Program Counter
    uint32_t currPC = (*((uint32_t*)SCPT_COUNTER)); 
    // Get next executed command
    uint32_t currCmd = *((uint32_t*)((uint32_t*)scriptProc + 3 + currPC));
    // Get next executed command arg if it has one (otherwise garbage value)
    uint32_t currCmdArg = *((uint32_t*)((uint32_t*)scriptProc + 4 + currPC));

    printf("[DSCDebug] PC: %u  -> CMD: %#02x\n", currPC, currCmd);

    // Either we're on our first tick, or we encountered the END_OF_DEBUG marker (TIME 1488)
    if (!breaked || (currCmd == 0x1 && currCmdArg == 1488)) {
        printf("[DSCDebug] halting the script engine\n");
        // Write a long delay in the start of the script
        *(uint32_t*)((uint32_t*)scriptProc + 4) = 0x1; // TIME
        *(int32_t*)((int32_t*)scriptProc + 5) = 2147483647; // int_max

        *(uint32_t*)SCPT_COUNTER = 1; // Rewind Program Counter
        *(char*)SCPT_IS_RUNNING = 0x0; // Stop updating the engine on every tick
        breaked = true; 
    }

    // Call back to original script engine
    ScriptProcessor(scriptProc, arg2, arg3, arg4, arg5, arg6);
}

/// Enqueues a DSC script and starts it running
void EnqueueCommand(void* command, int len) {
	// Copy the script into the program memory
    memcpy((void*)(SCPT_START), command, len);
	// Add an END OF DEBUG marker (TIME 1488) at the end of it
    *(uint32_t*)(SCPT_START + len) = 0x1;
    *(uint32_t*)(SCPT_START + len + 4) = 1488;
	// Rewind Program Counter
	*(uint32_t*)SCPT_COUNTER = 1;
	// Run the script engine
    *(char*)SCPT_IS_RUNNING = 0x1; 
}

// ---------------- Server Stuff ------------------------
WSADATA            wsaData;
SOCKET             ReceivingSocket;
SOCKADDR_IN        ReceiverAddr;
int                Port = 39399;
char				ReceiveBuf[1024];
int                BufLength = 1024;
SOCKADDR_IN        SenderAddr;
int                SenderAddrSize = sizeof(SenderAddr);
int                ByteReceived = 5, SelectTiming, ErrorCode;

int recvfromTimeOutUDP(SOCKET socket, long sec, long usec)

{
	// Setup timeval variable
	struct timeval timeout;
	struct fd_set fds;
	timeout.tv_sec = sec;
	timeout.tv_usec = usec;
	// Setup fd_set structure
	FD_ZERO(&fds);
	FD_SET(socket, &fds);
	// Return value:
	// -1: error occurred
	// 0: timed out
	// > 0: data ready to be read
	return select(0, &fds, 0, 0, &timeout);
}
void StartServer() {
	printf("[DSCDebug] Starting debug server\n");
	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
	{
		printf("[DSCDebug] WSAStartup failed with error %ld\n", WSAGetLastError());
		return;
	}
	ReceivingSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (ReceivingSocket == INVALID_SOCKET)
	{
		printf("[DSCDebug] Error in socket(): %ld\n", WSAGetLastError());
		WSACleanup();
		return;
	}

	ReceiverAddr.sin_family = AF_INET;
	ReceiverAddr.sin_port = htons(Port);
	// From all interface (0.0.0.0)
	ReceiverAddr.sin_addr.s_addr = htonl(INADDR_ANY);

	if (bind(ReceivingSocket, (SOCKADDR*)&ReceiverAddr, sizeof(ReceiverAddr)) == SOCKET_ERROR)
	{
		printf("[DSCDebug] bind() failed! Error: %ld.\n", WSAGetLastError());
		closesocket(ReceivingSocket);
		WSACleanup();
		return;
	}
	else
		printf("[DSCDebug] UDP Debug Server listening at port 39399\n");

	while (1) {
		SelectTiming = recvfromTimeOutUDP(ReceivingSocket, 10, 0);
		switch (SelectTiming)
		{
			case 0:
				// Timed out
				break;

			case -1:
				// Error occurred, maybe we should display an error message?
				break;

			default:
			{
				while (1)
				{
					ByteReceived = recvfrom(ReceivingSocket, ReceiveBuf, BufLength,
						0, (SOCKADDR*)&SenderAddr, &SenderAddrSize);
					if (ByteReceived > 0)
					{
						printf("[DSCDebug] Script received: %d bytes\n", ByteReceived);
						EnqueueCommand((void*)ReceiveBuf, ByteReceived);
					}
					else if (ByteReceived <= 0)
						printf("[DSCDebug] Connection closed with error code: %ld\n", WSAGetLastError());
					else
						printf("[DSCDebug] recvfrom() failed with error code: %d\n", WSAGetLastError());
					break;
				}
			}
		}
	}
}

extern "C" __declspec(dllexport) void InvokeBridge() {
	printf("[DSCDebug] Creating debug detours\n");
    DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());
    DetourAttach(&(PVOID&)ScriptProcessor, ScriptProcessorHooked);
    DetourTransactionCommit();

    StartServer();
}

extern "C" __declspec(dllexport) void StartFastboot() {
	printf("[DSCDebug] Invoke fast boot\n");
	StartFastbootEx();
}

extern "C" __declspec(dllexport) void Boink() {
	printf("[DSCDebug] Press a key\n");
	((InputState*)(*(uint64_t*)INPUT_STATE_PTR_ADDRESS))->Tapped.Buttons = JVS_CIRCLE;
	((InputState*)(*(uint64_t*)INPUT_STATE_PTR_ADDRESS))->Released.Buttons = JVS_CIRCLE;
	((InputState*)(*(uint64_t*)INPUT_STATE_PTR_ADDRESS))->Down.Buttons = JVS_CIRCLE;
}

bool isVfsOn = false;
extern "C" __declspec(dllexport) void VirtualPvPd(VfsInfo * path) {
	printf("[DSCDebug] Virtualize PVPD\n");
	if (!isVfsOn) {
		isVfsOn = true;
		EnableVfs();
	}
	SetVfsPvDb(path->pvDbPath, path->audioPath, path->dscPath, path->pvLstFarc);
}

void RevokeBridge() {
    DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());
    DetourDetach(&(PVOID&)ScriptProcessor, ScriptProcessorHooked);
    DetourTransactionCommit();
}

GameState curDbgGs = GS_STARTUP;
typedef void ChangeGameState(GameState);
ChangeGameState* changeGameState = (ChangeGameState*)CHANGE_MODE_ADDRESS;

typedef void ChangeSubState(GameState, SubGameState);
ChangeSubState* changeSubState = (ChangeSubState*)CHANGE_SUB_MODE_ADDRESS;
/**
 * Remotely callable procedure for experiments
 */
extern "C" __declspec(dllexport) void SetState(GameState s)
{
	printf("[DLL] Change GS %lu\n", s);
	curDbgGs = s;
	//changeGameState(s);
}
/**
 * Remotely callable procedure for experiments
 */
extern "C" __declspec(dllexport) void SetSubState(SubGameState s)
{
	printf("[DLL] Change SS %lu\n", s);
	changeGameState(curDbgGs);
	changeSubState(curDbgGs, s);
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
		break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

