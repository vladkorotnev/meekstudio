#include "pch.h"
#include "interop.h"
#include "fastboot.h"
#include <Windows.h>
#include "detours.h"
#include <stdio.h>


const int updatesPerFrame = 39; // Task updates per engine refresh
GameState currentGameState;
GameState previousGameState;
bool dataInitialized = false;
bool fastbootEnabled = false;
HANDLE gsReadySemaphore; // Semaphore for "app is ready" signal
HANDLE gsStartSemaphore;
HANDLE gsEndSemaphore;
void* tickHook = (void*)ENGINE_UPDATE_HOOK_TARGET_ADDRESS; // Hook for system refresh function
DoneBootCallback* onBootCallback = NULL;

// Hooked version of frame tick handler
void UpdateTick()
{
    if (dataInitialized) // Once initialization is done, remove the hook
    {
        DetourTransactionBegin();
        DetourUpdateThread(GetCurrentThread());
        DetourDetach(&(PVOID&)tickHook, UpdateTick);
        DetourTransactionCommit();
        return;
    }

    previousGameState = currentGameState;
    currentGameState = *(GameState*)CURRENT_GAME_STATE_ADDRESS;

    if (currentGameState == GS_STARTUP)
    {
            // While application is booting...
            typedef void UpdateTask();
            UpdateTask* updateTask = (UpdateTask*)UPDATE_TASKS_ADDRESS;

            // speed up TaskSystemStartup
            for (int i = 0; i < updatesPerFrame; i++)
                updateTask();

            const int DATA_INITIALIZED = 3;

            // skip TaskDataInit
            *(int*)(DATA_INIT_STATE_ADDRESS) = DATA_INITIALIZED;

            // skip TaskWarning
            *(int*)(SYSTEM_WARNING_ELAPSED_ADDRESS) = 3939;
    }
    else if (previousGameState == GS_STARTUP)
    {
        // Once boot is done, set a flag
        dataInitialized = true;

        if (onBootCallback != NULL) {
            onBootCallback();
            onBootCallback = NULL;
        }

        // Signal outside that boot is complete
        ReleaseSemaphore(gsReadySemaphore, 1, NULL);
        printf("[DLL] XPC signaling that app is ready.\n");
        CloseHandle(gsReadySemaphore);
    }
}

void StartFastbootEx()
{
    gsReadySemaphore = OpenSemaphore(SEMAPHORE_ALL_ACCESS, TRUE, L"dscDbgAppRdy");

    DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());
    DetourAttach(&(PVOID&)tickHook, UpdateTick);
    DetourTransactionCommit();
}



