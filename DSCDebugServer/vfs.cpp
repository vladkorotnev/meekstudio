#include <Windows.h>
#include "detours.h"
#include <stdio.h>
#include "pch.h"

LPWSTR pPvDbPath = NULL;
LPWSTR pDscPath = NULL;
LPWSTR pAudioPath = NULL;
LPWSTR pGmPvLstPath = NULL;

HANDLE(WINAPI* realCreateFile)(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode,
	LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttrs, HANDLE hTemplateFile) = CreateFileW;
HANDLE(WINAPI* realCreateFileA)(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode,
	LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttrs, HANDLE hTemplateFile) = CreateFileA;
HANDLE(WINAPI* realFindFirstFileExW)(
	LPCWSTR            lpFileName,
	FINDEX_INFO_LEVELS fInfoLevelId,
	LPVOID             lpFindFileData,
	FINDEX_SEARCH_OPS  fSearchOp,
	LPVOID             lpSearchFilter,
	DWORD              dwAdditionalFlags) = FindFirstFileExW;

bool CreateVfsPath(LPCWSTR lpFileName, LPWSTR lpLocalizedName) {
	if (lpFileName == NULL) return false;
	if (wcsstr(lpFileName, L"mdata") != 0) return false;
	if (wcsstr(lpFileName, L"pv_999") != 0) return false;
	if (wcsstr(lpFileName, L"pv_001.ogg") != 0) return false;

	if (wcsstr(lpFileName, L"pv_db.txt") != 0 && pPvDbPath != NULL)
	{
		wcscpy(lpLocalizedName, pPvDbPath);
		return true;
	}
	if (wcsstr(lpFileName, L"gm_pv_list_tbl.farc") != 0 && pGmPvLstPath != NULL)
	{
		wcscpy(lpLocalizedName, pGmPvLstPath);
		return true;
	}
	else if (wcsstr(lpFileName, L".dsc") != 0 && wcsstr(lpFileName, L"script") != 0) {
		if (pDscPath != NULL) {
			wcscpy(lpLocalizedName, pDscPath);
		}
		else {
			wcscpy(lpLocalizedName, L"rom/script/pv_999_normal.dsc");
		}
		return true;
	}
	else if (wcsstr(lpFileName, L".ogg") != 0 && wcsstr(lpFileName, L"song") != 0) {
		if (pAudioPath != NULL) {
			wcscpy(lpLocalizedName, pAudioPath);
		}
		else {
			wcscpy(lpLocalizedName, L"rom/sound/song/pv_001.ogg");
		}
		return true;
	}

	return false;
}

HANDLE WINAPI ComCreateFile(LPCSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttrs, HANDLE hTemplateFile) {
	HANDLE rslt = INVALID_HANDLE_VALUE;

	if (strstr(lpFileName, "COM") != 0) {
		// Disable COM ports
	}
	else {
		rslt = realCreateFileA(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttrs, hTemplateFile);
	}
	
	return rslt;
}

HANDLE WINAPI VfsCreateFile(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttrs, HANDLE hTemplateFile) {
	HANDLE rslt = INVALID_HANDLE_VALUE;


	wchar_t localizedPath[MAX_PATH] = L"";
	if (CreateVfsPath(lpFileName, localizedPath)) {
		printf("[VFS] CreateFile %ls -> %ls\n", lpFileName, localizedPath);
		rslt = realCreateFile(localizedPath, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttrs, hTemplateFile);
	}
	else {
		rslt = realCreateFile(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttrs, hTemplateFile);
	}

	return rslt;
}

HANDLE WINAPI VfsFindFirstFileExW(
	LPCWSTR            lpFileName,
	FINDEX_INFO_LEVELS fInfoLevelId,
	LPVOID             lpFindFileData,
	FINDEX_SEARCH_OPS  fSearchOp,
	LPVOID             lpSearchFilter,
	DWORD              dwAdditionalFlags
) {
	HANDLE rslt = INVALID_HANDLE_VALUE;

	wchar_t localizedPath[MAX_PATH] = L"";
	if (CreateVfsPath(lpFileName, localizedPath)) {
		printf("[VFS] FindFirstFileExW %ls -> %ls\n", lpFileName, localizedPath);
		rslt = realFindFirstFileExW(localizedPath, fInfoLevelId, lpFindFileData, fSearchOp, lpSearchFilter, dwAdditionalFlags);
	}
	else {
		rslt = realFindFirstFileExW(lpFileName, fInfoLevelId, lpFindFileData, fSearchOp, lpSearchFilter, dwAdditionalFlags);
	}

	return rslt;

}

void EnableVfs() {
	printf("[VFS] Attach \n");
	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());
	DetourAttach(&(PVOID&)realFindFirstFileExW, VfsFindFirstFileExW);
	DetourAttach(&(PVOID&)realCreateFile, VfsCreateFile);
	DetourAttach(&(PVOID&)realCreateFileA, ComCreateFile);
	DetourTransactionCommit();
}

void SetVfsPvDb(char* basePath, char* audioPath, char* dscPath, char* gmPvLstPath) {
	if (pPvDbPath != NULL) {
		free(pPvDbPath);
	}
	if (pDscPath != NULL) {
		free(pDscPath);
	}
	if (pAudioPath != NULL) {
		free(pAudioPath);
	}
	if (pGmPvLstPath != NULL) {
		free(pGmPvLstPath);
	}
	

	if (basePath == NULL) {
		printf("[VFS] Disable\n");
	}
	else {
		printf("[VFS] Set base: %s\n", basePath);
		int count = strlen(basePath);
		pPvDbPath = (LPWSTR)malloc(sizeof(WCHAR) * count);
		mbstowcs(pPvDbPath, basePath, count + 1);

		if (dscPath != NULL) {
			printf("[VFS] Set dsc: %s\n", dscPath);
			count = strlen(dscPath);
			pDscPath = (LPWSTR)malloc(sizeof(WCHAR) * count);
			mbstowcs(pDscPath, dscPath, count + 1);
		}

		if (audioPath != NULL) {
			printf("[VFS] Set audio: %s\n", audioPath);
			count = strlen(audioPath);
			pAudioPath = (LPWSTR)malloc(sizeof(WCHAR) * count);
			mbstowcs(pAudioPath, audioPath, count + 1);
		}

		if (gmPvLstPath != NULL) {
			printf("[VFS] Set PvList: %s\n", gmPvLstPath);
			count = strlen(gmPvLstPath);
			pGmPvLstPath = (LPWSTR)malloc(sizeof(WCHAR) * count);
			mbstowcs(pGmPvLstPath, gmPvLstPath, count + 1);
		}
	}
}